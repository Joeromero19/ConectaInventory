using Microsoft.EntityFrameworkCore;
using ConectaInventory.Data;
using ConectaInventory.Models;
using ConectaInventory.ViewModels;

namespace ConectaInventory.Services;

// Diagnostico tecnico y baja definitiva (HU-0105)
public class BajasService
{
    private readonly ConectaInventoryContext _db;

    // Estados desde los cuales se puede iniciar o repetir un diagnostico (DE-S01-01)
    private static readonly EstadoEquipo[] EstadosDiagnosticables =
    {
        EstadoEquipo.Registrado, EstadoEquipo.Disponible,
        EstadoEquipo.EnDiagnostico, EstadoEquipo.EnMantenimiento
    };

    public BajasService(ConectaInventoryContext db)
    {
        _db = db;
    }

    // CU1.03: registrar diagnostico
    public async Task<Resultado> RegistrarDiagnosticoAsync(DiagnosticoVM vm, int responsableId)
    {
        var serial = vm.Serial.NormalizarSerial();
        var equipo = await _db.Equipos.FirstOrDefaultAsync(e => e.Serial == serial);
        if (equipo == null)
            return Resultado.Error("El equipo no existe.");
        if (await _db.Mantenimientos.AnyAsync(m => m.EquipoId == equipo.Id && m.Abierto) || await _db.DetallesDevolucion.AnyAsync(d => d.EquipoId == equipo.Id && !d.InspeccionFavorable && equipo.Estado == EstadoEquipo.EnMantenimiento && !_db.Mantenimientos.Any(m => m.DetalleDevolucionId == d.Id)))
            return Resultado.Error("Gestione la falla desde Mantenimiento.");
        if (!EstadosDiagnosticables.Contains(equipo.Estado))
            return Resultado.Error($"No se puede diagnosticar un equipo en estado {equipo.Estado.Texto()}.");

        var diagnostico = new DiagnosticoTecnico
        {
            EquipoId = equipo.Id,
            Fecha = DateTime.Now,
            Hallazgos = vm.Hallazgos.Trim(),
            Resultado = vm.Resultado,
            ResponsableId = responsableId
        };
        _db.Diagnosticos.Add(diagnostico);

        // Una falla NO causa baja automatica: un equipo No Apto queda en diagnostico
        // esperando la solicitud de baja y la decision del administrador
        equipo.Estado = vm.Resultado switch
        {
            ResultadoDiagnostico.Apto => EstadoEquipo.Disponible,
            ResultadoDiagnostico.Reparable => EstadoEquipo.EnMantenimiento,
            _ => EstadoEquipo.EnDiagnostico
        };

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Resultado.Error("No se pudo registrar el diagnostico.");
        }

        return Resultado.Ok(diagnostico.Id, $"Diagnostico registrado. Estado del equipo: {equipo.Estado.Texto()}.");
    }

    // CU1.04: solicitar baja (solo con diagnostico vigente No Apto)
    public async Task<Resultado> SolicitarBajaAsync(SolicitarBajaVM vm, int solicitanteId)
    {
        var serial = vm.Serial.NormalizarSerial();
        var equipo = await _db.Equipos.FirstOrDefaultAsync(e => e.Serial == serial);
        if (equipo == null)
            return Resultado.Error("El equipo no existe.");
        if (equipo.Estado == EstadoEquipo.BajaDefinitiva)
            return Resultado.Error("El equipo ya fue dado de baja.");
        if (equipo.Estado == EstadoEquipo.Prestado || await _db.DetallesPrestamo.AnyAsync(d=>d.EquipoId==equipo.Id && (d.Prestamo.Estado==EstadoPrestamo.Activo || d.Prestamo.Estado==EstadoPrestamo.Vencido)))
            return Resultado.Error("No se puede solicitar baja de un equipo prestado.");

        if (await _db.Mantenimientos.AnyAsync(m => m.EquipoId == equipo.Id && m.Abierto)) return Resultado.Error("Existe mantenimiento abierto.");
        var diagnostico = await _db.Diagnosticos
            .Where(d => d.EquipoId == equipo.Id)
            .OrderByDescending(d => d.Fecha).ThenByDescending(d => d.Id)
            .FirstOrDefaultAsync();
        if (diagnostico == null || diagnostico.Resultado != ResultadoDiagnostico.NoApto)
            return Resultado.Error("La baja requiere un diagnostico vigente con resultado No Apto.");

        var yaPendiente = await _db.SolicitudesBaja.AnyAsync(s =>
            s.EquipoId == equipo.Id && s.Estado == EstadoSolicitudBaja.Solicitada);
        if (yaPendiente)
            return Resultado.Error("Ya existe una solicitud de baja pendiente para este equipo.");

        var solicitud = new SolicitudBaja
        {
            EquipoId = equipo.Id,
            DiagnosticoTecnicoId = diagnostico.Id,
            Motivo = vm.Motivo.Trim(),
            Estado = EstadoSolicitudBaja.Solicitada,
            FechaSolicitud = DateTime.Now,
            SolicitanteId = solicitanteId
        };
        _db.SolicitudesBaja.Add(solicitud);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Resultado.Error("No se pudo registrar la solicitud.");
        }

        return Resultado.Ok(solicitud.Id, "Solicitud de baja registrada. Queda pendiente de decision administrativa.");
    }

    // CU1.05: autorizar o rechazar
    public async Task<Resultado> EvaluarAsync(EvaluarBajaVM vm, int administradorId)
    {
        var solicitud = await _db.SolicitudesBaja.Include(s => s.Equipo)
            .FirstOrDefaultAsync(s => s.Id == vm.SolicitudId);
        if (solicitud == null)
            return Resultado.Error("La solicitud no existe.");
        if (solicitud.Estado != EstadoSolicitudBaja.Solicitada)
            return Resultado.Error("La solicitud ya fue evaluada.");

        if (!vm.Aprobar)
        {
            if (string.IsNullOrWhiteSpace(vm.Motivo))
                return Resultado.Error("Indique el motivo del rechazo.");

            RegistrarDecision(solicitud, vm, administradorId);
            solicitud.Estado = EstadoSolicitudBaja.Rechazada;
            return await GuardarAsync(solicitud.Id, "Solicitud rechazada.");
        }

        if (await _db.Mantenimientos.AnyAsync(m => m.EquipoId == solicitud.EquipoId && m.Abierto)) return Resultado.Error("Existe mantenimiento abierto.");
        // Regla de integracion con el Modulo 02: no se da de baja un equipo con prestamo activo
        var conPrestamo = await _db.DetallesPrestamo.AnyAsync(p => p.EquipoId == solicitud.EquipoId &&
            (p.Prestamo.Estado == EstadoPrestamo.Activo || p.Prestamo.Estado == EstadoPrestamo.Vencido));
        if (conPrestamo)
            return Resultado.Error("El equipo tiene un prestamo activo o vencido: la baja queda bloqueada.");

        if (solicitud.Equipo.Estado is not (EstadoEquipo.EnDiagnostico or EstadoEquipo.EnMantenimiento))
            return Resultado.Error($"El equipo esta en estado {solicitud.Equipo.Estado.Texto()} y no admite baja.");

        // Baja y acta se guardan en una sola transaccion (un unico SaveChanges)
        RegistrarDecision(solicitud, vm, administradorId);
        solicitud.Estado = EstadoSolicitudBaja.Ejecutada;
        solicitud.Equipo.Estado = EstadoEquipo.BajaDefinitiva;
        _db.Actas.Add(new ActaBaja { Solicitud = solicitud, Fecha = DateTime.Now });

        return await GuardarAsync(solicitud.Id, "Baja autorizada y ejecutada. Acta emitida.");
    }

    // Se aplica solo cuando ya se validaron todas las reglas
    private static void RegistrarDecision(SolicitudBaja solicitud, EvaluarBajaVM vm, int administradorId)
    {
        solicitud.FechaDecision = DateTime.Now;
        solicitud.DecididaPorId = administradorId;
        solicitud.DecisionMotivo = vm.Motivo?.Trim();
    }

    private async Task<Resultado> GuardarAsync(int solicitudId, string mensajeExito)
    {
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Resultado.Error("Error al guardar la decision. No se aplico ningun cambio.");
        }
        return Resultado.Ok(solicitudId, mensajeExito);
    }

    public async Task<List<SolicitudBaja>> ListarSolicitudesAsync() =>
        await _db.SolicitudesBaja.AsNoTracking()
            .Include(s => s.Equipo).Include(s => s.Solicitante).Include(s => s.Acta)
            .OrderByDescending(s => s.FechaSolicitud)
            .ToListAsync();

    public async Task<SolicitudBaja?> ObtenerSolicitudAsync(int id) =>
        await _db.SolicitudesBaja.AsNoTracking()
            .Include(s => s.Equipo).Include(s => s.Diagnostico).Include(s => s.Solicitante)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<ActaBaja?> ObtenerActaAsync(int solicitudId) =>
        await _db.Actas.AsNoTracking()
            .Include(a => a.Solicitud).ThenInclude(s => s.Equipo)
            .Include(a => a.Solicitud).ThenInclude(s => s.Diagnostico)
            .Include(a => a.Solicitud).ThenInclude(s => s.Solicitante)
            .Include(a => a.Solicitud).ThenInclude(s => s.DecididaPor)
            .FirstOrDefaultAsync(a => a.SolicitudBajaId == solicitudId);
}
