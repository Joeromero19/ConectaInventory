using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ConectaInventory.Data;
using ConectaInventory.Models;
using ConectaInventory.ViewModels;

namespace ConectaInventory.Services;

// Reglas de recepcion, alta, reubicacion y consulta de stock (HU-0101, HU-0102, HU-0104)
public class InventarioService
{
    private readonly ConectaInventoryContext _db;

    public InventarioService(ConectaInventoryContext db)
    {
        _db = db;
    }

    // ---------- Listas para los formularios ----------

    public async Task<List<SelectListItem>> CategoriasAsync() =>
        await _db.Categorias.OrderBy(c => c.Nombre)
            .Select(c => new SelectListItem(c.Nombre, c.Id.ToString())).ToListAsync();

    public async Task<List<SelectListItem>> UbicacionesHabilitadasAsync() =>
        await _db.Ubicaciones.Where(u => u.Habilitada).OrderBy(u => u.Nombre)
            .Select(u => new SelectListItem(u.Nombre, u.Id.ToString())).ToListAsync();

    public async Task<List<SelectListItem>> UbicacionesTodasAsync() =>
        await _db.Ubicaciones.OrderBy(u => u.Nombre)
            .Select(u => new SelectListItem(u.Nombre, u.Id.ToString())).ToListAsync();

    // Recepciones que aun tienen equipos por dar de alta
    public async Task<List<SelectListItem>> RecepcionesPendientesAsync()
    {
        var datos = await _db.Recepciones
            .Where(r => r.Estado == EstadoRecepcion.EnRecepcion)
            .OrderByDescending(r => r.Id)
            .Select(r => new { r.Id, Donante = r.Donante.Nombre, r.Lote, r.CantidadFisica, Altas = r.Equipos.Count })
            .ToListAsync();

        return datos.Select(d => new SelectListItem(
            $"REC-{d.Id:D6} | {d.Donante} | Lote {d.Lote} | {d.Altas}/{d.CantidadFisica} con alta",
            d.Id.ToString())).ToList();
    }

    // ---------- HU-0101 / CU1.01: registrar recepcion ----------

    public async Task<Resultado> RegistrarRecepcionAsync(RecepcionVM vm, int responsableId)
    {
        var nombre = vm.DonanteNombre.Trim();
        var lote = vm.Lote.Trim();

        // El donante se reutiliza si ya existe; si no, se registra junto con la recepcion
        var donante = await _db.Donantes.FirstOrDefaultAsync(d => d.Nombre == nombre);
        if (donante != null)
        {
            var duplicada = await _db.Recepciones.AnyAsync(r => r.DonanteId == donante.Id && r.Lote == lote);
            if (duplicada)
                return Resultado.Error("Ya existe una recepcion de este donante con el mismo lote.");
        }
        else
        {
            donante = new Donante { Nombre = nombre, Contacto = vm.DonanteContacto?.Trim() };
            _db.Donantes.Add(donante);
        }

        var recepcion = new RecepcionDonacion
        {
            Donante = donante,
            Fecha = DateTime.Now,
            Lote = lote,
            Descripcion = vm.Descripcion.Trim(),
            CantidadFisica = vm.CantidadFisica,
            Estado = EstadoRecepcion.EnRecepcion, // recibir no implica disponibilidad para prestamo
            ResponsableId = responsableId
        };
        _db.Recepciones.Add(recepcion);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Resultado.Error("No se pudo guardar la recepcion. No se genero radicado.");
        }

        return Resultado.Ok(recepcion.Id, $"Recepcion guardada. Radicado: {recepcion.Radicado}.");
    }

    // ---------- HU-0101 / CU1.02: alta de equipo ----------

    public async Task<Resultado> DarDeAltaAsync(AltaVM vm)
    {
        var recepcion = await _db.Recepciones.FirstOrDefaultAsync(r => r.Id == vm.RecepcionId);
        if (recepcion == null)
            return Resultado.Error("La recepcion seleccionada no existe.");
        if (recepcion.Estado != EstadoRecepcion.EnRecepcion)
            return Resultado.Error("La recepcion ya no admite altas de equipos.");

        // Consistencia de cantidades: no se pueden dar de alta mas equipos de los recibidos
        var altas = await _db.Equipos.CountAsync(e => e.RecepcionDonacionId == recepcion.Id);
        if (altas >= recepcion.CantidadFisica)
            return Resultado.Error("Ya se dieron de alta todos los equipos declarados en la recepcion.");

        if (await _db.Categorias.FindAsync(vm.CategoriaId) == null)
            return Resultado.Error("La categoria seleccionada no existe.");

        var ubicacion = await _db.Ubicaciones.FindAsync(vm.UbicacionId);
        if (ubicacion == null || !ubicacion.Habilitada)
            return Resultado.Error("La ubicacion no existe o no esta habilitada.");

        var serial = vm.Serial.NormalizarSerial();
        if (await _db.Equipos.AnyAsync(e => e.Serial == serial))
            return Resultado.Error("El serial ya esta registrado. Ingrese un serial distinto.");

        var equipo = new Equipo
        {
            Serial = serial,
            Marca = vm.Marca.Trim(),
            Modelo = vm.Modelo.Trim(),
            Especificaciones = vm.Especificaciones?.Trim(),
            Estado = EstadoEquipo.Registrado,
            RecepcionDonacionId = recepcion.Id,
            CategoriaEquipoId = vm.CategoriaId,
            UbicacionBodegaId = vm.UbicacionId
        };
        _db.Equipos.Add(equipo);

        // Con el ultimo equipo, la recepcion queda completa
        if (altas + 1 >= recepcion.CantidadFisica)
            recepcion.Estado = EstadoRecepcion.Registrada;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Cubre la carrera de dos altas simultaneas con el mismo serial (indice unico)
            return Resultado.Error("No se pudo dar de alta el equipo. Verifique que el serial no este repetido.");
        }

        return Resultado.Ok(equipo.Id, $"Equipo dado de alta. Identificador: {equipo.Identificador}.");
    }

    // ---------- HU-0102 / CU1.09: reubicar ----------

    public async Task<Resultado> ReubicarAsync(ReubicarVM vm, int responsableId)
    {
        var serial = vm.Serial.NormalizarSerial();
        var equipo = await _db.Equipos.FirstOrDefaultAsync(e => e.Serial == serial);
        if (equipo == null)
            return Resultado.Error("El equipo no existe.");
        if (equipo.Estado == EstadoEquipo.BajaDefinitiva)
            return Resultado.Error("Un equipo dado de baja no se puede reubicar.");
        if (equipo.Estado == EstadoEquipo.Prestado)
            return Resultado.Error("Un equipo prestado no se puede reubicar.");

        var destino = await _db.Ubicaciones.FindAsync(vm.UbicacionDestinoId);
        if (destino == null || !destino.Habilitada)
            return Resultado.Error("La ubicacion destino no existe o no esta habilitada.");
        if (destino.Id == equipo.UbicacionBodegaId)
            return Resultado.Error("El equipo ya se encuentra en esa ubicacion.");

        // Regla de integracion con el Modulo 02: no se traslada un equipo prestado o reservado
        var conPrestamo = await _db.DetallesPrestamo.AnyAsync(p => p.EquipoId == equipo.Id &&
            (p.Prestamo.Estado == EstadoPrestamo.Activo || p.Prestamo.Estado == EstadoPrestamo.Vencido));
        if (conPrestamo)
            return Resultado.Error("El equipo tiene un prestamo activo o vencido: traslado bloqueado.");

        _db.Movimientos.Add(new MovimientoInventario
        {
            EquipoId = equipo.Id,
            UbicacionOrigenId = equipo.UbicacionBodegaId,
            UbicacionDestinoId = destino.Id,
            Fecha = DateTime.Now,
            Motivo = vm.Motivo.Trim(),
            ResponsableId = responsableId
        });
        equipo.UbicacionBodegaId = destino.Id;

        try
        {
            // Un unico SaveChanges es una unica transaccion: historial y ubicacion van juntos o no van
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Resultado.Error("Error al guardar el traslado. No se aplico ningun cambio.");
        }

        return Resultado.Ok(equipo.Id, $"Equipo {equipo.Identificador} reubicado en {destino.Nombre}.");
    }

    // ---------- HU-0104 / CU1.10: consultar stock ----------

    public async Task<List<StockItem>> ConsultarStockAsync(StockVM filtro)
    {
        IQueryable<Equipo> consulta = _db.Equipos.AsNoTracking();

        if (filtro.CategoriaId.HasValue)
            consulta = consulta.Where(e => e.CategoriaEquipoId == filtro.CategoriaId.Value);
        if (filtro.Estado.HasValue)
            consulta = consulta.Where(e => e.Estado == filtro.Estado.Value);
        if (filtro.UbicacionId.HasValue)
            consulta = consulta.Where(e => e.UbicacionBodegaId == filtro.UbicacionId.Value);

        var items = await consulta
            .OrderBy(e => e.Serial)
            .Select(e => new StockItem
            {
                Id = e.Id,
                Serial = e.Serial,
                Marca = e.Marca,
                Modelo = e.Modelo,
                Categoria = e.Categoria.Nombre,
                Ubicacion = e.Ubicacion.Nombre,
                Estado = e.Estado,
                // Stock fisico != disponibilidad efectiva: se excluyen prestados y reservados
                Disponible = e.Estado == EstadoEquipo.Disponible &&
                    !e.DetallesPrestamo.Any(p => p.Prestamo.Estado == EstadoPrestamo.Activo || p.Prestamo.Estado == EstadoPrestamo.Vencido)
            })
            .ToListAsync();

        if (filtro.SoloDisponibles)
            items = items.Where(i => i.Disponible).ToList();

        return items;
    }
}
