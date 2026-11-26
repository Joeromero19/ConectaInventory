using System.Data;
using Microsoft.EntityFrameworkCore;
using ConectaInventory.Data;
using ConectaInventory.Models;
namespace ConectaInventory.Services;
public class PrestamoService(ConectaInventoryContext db, ILogger<PrestamoService> logger)
{
    static DateOnly Hoy => DateOnly.FromDateTime(DateTime.Today);
    static bool Pendiente(EstadoSolicitudPrestamo e) => e is EstadoSolicitudPrestamo.Pendiente or EstadoSolicitudPrestamo.PendienteAjuste or EstadoSolicitudPrestamo.EnRevision;
    static bool Abierto(Prestamo p) => p.Estado is EstadoPrestamo.Activo or EstadoPrestamo.Vencido;
    public async Task<Dictionary<int,int>> Disponibilidad(DateOnly inicio, DateOnly fin, int excluir = 0)
    {
        // Estimación conservadora: no prometer equipos aún prestados, ni retornos futuros no inspeccionados.
        var fisicos = await db.Equipos.Where(e=>e.Estado==EstadoEquipo.Disponible && !e.DetallesPrestamo.Any(d=>d.Prestamo.Estado==EstadoPrestamo.Activo || d.Prestamo.Estado==EstadoPrestamo.Vencido)).GroupBy(e=>e.CategoriaEquipoId).Select(g=>new {Id=g.Key,N=g.Count()}).ToDictionaryAsync(x=>x.Id,x=>x.N);
        var reservas = await db.DetallesSolicitud.Where(d=>d.SolicitudPrestamoId!=excluir && d.Solicitud.Estado==EstadoSolicitudPrestamo.Aprobada && d.Solicitud.FechaInicioPrevista<=fin && d.Solicitud.FechaFinPrevista>=inicio).GroupBy(d=>d.CategoriaEquipoId).Select(g=>new {Id=g.Key,N=g.Sum(d=>d.CantidadReservada)}).ToListAsync();
        foreach(var r in reservas) fisicos[r.Id]=Math.Max(0,fisicos.GetValueOrDefault(r.Id)-r.N);
        return fisicos;
    }
    async Task<Resultado> Transaccion(Func<Task<Resultado>> accion)
    {
        try {
            await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var r=await accion();
            if(r.Exito) await tx.CommitAsync(); else {await tx.RollbackAsync();db.ChangeTracker.Clear();}
            return r;
        } catch(Exception ex) when(ex is DbUpdateException or System.Data.Common.DbException or InvalidOperationException) {
            logger.LogError(ex,"Operación de préstamo revertida"); db.ChangeTracker.Clear(); return Resultado.Error("No se pudo completar la operación. No se confirmó ningún cambio; actualice y vuelva a intentar.");
        }
    }
    void Evento(SolicitudPrestamo s,int usuario,string mensaje) => db.EventosPrestamo.Add(new EventoPrestamo{Solicitud=s,UsuarioId=usuario,Fecha=DateTime.Now,Mensaje=mensaje});
    public Task<Resultado> GuardarSolicitud(int usuario,int id,DateOnly inicio,DateOnly fin,string? proposito,string? institucion,Dictionary<int,int>? cantidades) => Transaccion(async()=> {
        var b=await db.Beneficiarios.SingleOrDefaultAsync(x=>x.UsuarioId==usuario);
        if(b==null) return Resultado.Error("No existe perfil de beneficiario.");
        if(inicio<Hoy || fin<inicio || string.IsNullOrWhiteSpace(proposito) || proposito.Length>350 || institucion?.Length>160 || cantidades==null || cantidades.Count==0 || cantidades.Any(x=>x.Value<0 || x.Value>10000)) return Resultado.Error("Revise fechas, propósito y cantidades.");
        var items=cantidades.Where(x=>x.Value>0).ToDictionary(x=>x.Key,x=>x.Value);
        if(items.Count==0 || await db.Categorias.CountAsync(c=>items.Keys.Contains(c.Id))!=items.Count) return Resultado.Error("Seleccione categorías válidas y cantidades positivas.");
        if(await db.SolicitudesPrestamo.AnyAsync(s=>s.Id!=id && s.BeneficiarioId==b.Id && s.FechaInicioPrevista==inicio && s.FechaFinPrevista==fin && (s.Estado==EstadoSolicitudPrestamo.Pendiente || s.Estado==EstadoSolicitudPrestamo.PendienteAjuste || s.Estado==EstadoSolicitudPrestamo.EnRevision))) return Resultado.Error("Ya existe una solicitud pendiente para ese periodo.");
        SolicitudPrestamo s;
        if(id==0) {s=new(){BeneficiarioId=b.Id,FechaSolicitud=DateTime.Now};db.SolicitudesPrestamo.Add(s);}
        else {var actual=await db.SolicitudesPrestamo.Include(x=>x.Detalles).SingleOrDefaultAsync(x=>x.Id==id && x.BeneficiarioId==b.Id);if(actual==null || !Pendiente(actual.Estado)) return Resultado.Error("No puede modificar esta solicitud.");s=actual;
            // Actualizar en sitio para no violar el índice único de categoría.
            foreach(var d in s.Detalles.ToList()) if(!items.ContainsKey(d.CategoriaEquipoId)) {db.DetallesSolicitud.Remove(d);s.Detalles.Remove(d);}
        }
        foreach(var item in items) {var d=s.Detalles.FirstOrDefault(x=>x.CategoriaEquipoId==item.Key);if(d==null){d=new(){CategoriaEquipoId=item.Key};s.Detalles.Add(d);}d.CantidadSolicitada=item.Value;d.CantidadAprobada=null;d.CantidadReservada=0;d.CantidadPropuesta=null;}
        s.FechaInicioPrevista=inicio;s.FechaFinPrevista=fin;s.Proposito=proposito.Trim();s.InstitucionRepresentada=string.IsNullOrWhiteSpace(institucion)?null:institucion.Trim();s.Estado=EstadoSolicitudPrestamo.Pendiente;s.FechaActualizacion=DateTime.Now;
        var disponibilidad=await Disponibilidad(inicio,fin,id);bool falta=items.Any(x=>x.Value>disponibilidad.GetValueOrDefault(x.Key));
        Evento(s,usuario,id==0?"Solicitud radicada":"Solicitud modificada y enviada a revisión");
        await db.SaveChangesAsync();return Resultado.Ok(s.Id,$"{s.Radicado} guardada."+(falta?" Disponibilidad orientativa insuficiente; se enviará a evaluación.":""));
    });
    public Task<Resultado> Cancelar(int usuario,int id,bool admin) => Transaccion(async()=> {
        var s=await db.SolicitudesPrestamo.Include(x=>x.Beneficiario).Include(x=>x.Detalles).SingleOrDefaultAsync(x=>x.Id==id);
        if(s==null || (!admin && s.Beneficiario.UsuarioId!=usuario) || !(Pendiente(s.Estado)||s.Estado==EstadoSolicitudPrestamo.Aprobada)) return Resultado.Error("No se puede cancelar esta solicitud.");
        s.Estado=EstadoSolicitudPrestamo.Cancelada;s.FechaActualizacion=DateTime.Now;foreach(var d in s.Detalles)d.CantidadReservada=0;Evento(s,usuario,"Solicitud cancelada; reservas liberadas");await db.SaveChangesAsync();return Resultado.Ok(id,"Solicitud cancelada.");
    });
    public Task<Resultado> Evaluar(int usuario,int id,string decision,string? motivo) => Transaccion(async()=> {
        var s=await db.SolicitudesPrestamo.Include(x=>x.Detalles).SingleOrDefaultAsync(x=>x.Id==id);
        if(s==null || s.Estado is not (EstadoSolicitudPrestamo.Pendiente or EstadoSolicitudPrestamo.EnRevision)) return Resultado.Error("La solicitud no está pendiente de evaluación.");
        if(motivo?.Length>500) return Resultado.Error("Motivo demasiado largo.");
        var disp=await Disponibilidad(s.FechaInicioPrevista,s.FechaFinPrevista,s.Id);
        bool insuficiente=s.Detalles.Any(d=>d.CantidadSolicitada>disp.GetValueOrDefault(d.CategoriaEquipoId));
        if(decision=="aprobar") {
            if(insuficiente) return Resultado.Error("Disponibilidad insuficiente. Proponga un ajuste.");
            if(s.FechaInicioPrevista<Hoy) return Resultado.Error("Periodo vencido: solicite ajuste o rechace justificadamente.");
            foreach(var d in s.Detalles){d.CantidadAprobada=d.CantidadSolicitada;d.CantidadReservada=d.CantidadSolicitada;d.CantidadPropuesta=null;}s.Estado=EstadoSolicitudPrestamo.Aprobada;
        } else if(decision=="ajustar") {
            foreach(var d in s.Detalles)d.CantidadPropuesta=Math.Min(d.CantidadSolicitada,disp.GetValueOrDefault(d.CategoriaEquipoId));s.Estado=EstadoSolicitudPrestamo.PendienteAjuste;
        } else if(decision=="rechazar") {
            if(string.IsNullOrWhiteSpace(motivo)) return Resultado.Error("Indique un motivo de rechazo.");s.Estado=EstadoSolicitudPrestamo.Rechazada;
        } else return Resultado.Error("Decisión inválida.");
        s.UsuarioDecisionId=usuario;s.FechaDecision=DateTime.Now;s.FechaActualizacion=DateTime.Now;s.MotivoDecision=motivo?.Trim();Evento(s,usuario,$"Decisión: {s.Estado}. {s.MotivoDecision}");await db.SaveChangesAsync();return Resultado.Ok(id,$"Solicitud {s.Estado}.");
    });
    public Task<Resultado> Entregar(int usuario,int id,int receptor,string? identidad,string? referencia,bool firma,bool inspeccion,int[]? equipos,byte[]? documento) => Transaccion(async()=> {
        var s=await db.SolicitudesPrestamo.Include(x=>x.Detalles).SingleOrDefaultAsync(x=>x.Id==id);
        if(s==null || s.Estado!=EstadoSolicitudPrestamo.Aprobada || s.BeneficiarioId!=receptor || await db.Prestamos.AnyAsync(p=>p.SolicitudPrestamoId==id))return Resultado.Error("Solicitud no autorizada o receptor incorrecto.");
        if(Hoy<s.FechaInicioPrevista || Hoy>s.FechaFinPrevista)return Resultado.Error("Entrega fuera del periodo autorizado.");
        if(!firma || !inspeccion || string.IsNullOrWhiteSpace(identidad) || identidad.Length>100 || string.IsNullOrWhiteSpace(referencia) || referencia.Length>500)return Resultado.Error("Verifique identidad, inspección y acta firmada.");
        if(documento==null || documento.Length<5 || documento.Length>5*1024*1024 || !documento.AsSpan(0,5).SequenceEqual("%PDF-"u8))return Resultado.Error("Adjunte el acta firmada en PDF (máximo 5 MB).");
        if(equipos==null || equipos.Length==0 || equipos.Distinct().Count()!=equipos.Length)return Resultado.Error("Seleccione equipos físicos sin duplicados.");
        var seleccion=await db.Equipos.Include(e=>e.CodigoQR).Where(e=>equipos.Contains(e.Id)).ToListAsync();
        if(seleccion.Count!=equipos.Length || seleccion.Any(e=>e.Estado!=EstadoEquipo.Disponible || e.CodigoQR==null) || await db.DetallesPrestamo.AnyAsync(d=>equipos.Contains(d.EquipoId)&&(d.Prestamo.Estado==EstadoPrestamo.Activo||d.Prestamo.Estado==EstadoPrestamo.Vencido)))return Resultado.Error("Existe un equipo no disponible o sin identificación QR.");
        var conteo=seleccion.GroupBy(e=>e.CategoriaEquipoId).ToDictionary(g=>g.Key,g=>g.Count());
        if(conteo.Count!=s.Detalles.Count || s.Detalles.Any(d=>d.CantidadReservada<=0 || conteo.GetValueOrDefault(d.CategoriaEquipoId)!=d.CantidadReservada))return Resultado.Error("La entrega debe coincidir exactamente con las cantidades aprobadas.");
        var disp=await Disponibilidad(Hoy,s.FechaFinPrevista,s.Id);if(conteo.Any(c=>c.Value>disp.GetValueOrDefault(c.Key)))return Resultado.Error("Disponibilidad actual insuficiente para entregar sin afectar otras reservas.");
        var p=new Prestamo{Solicitud=s,BeneficiarioId=receptor,TipoPrestamo=s.InstitucionRepresentada==null?"Individual":"Institucional",InstitucionRepresentada=s.InstitucionRepresentada,FechaEntrega=DateTime.Now,FechaDevolucionPrevista=s.FechaFinPrevista};
        foreach(var e in seleccion){p.Detalles.Add(new DetallePrestamo{Equipo=e});e.Estado=EstadoEquipo.Prestado;}
        p.Entrega=new Entrega{ResponsableId=usuario,BeneficiarioReceptorId=receptor,FechaEntrega=p.FechaEntrega,IdentidadVerificada=identidad.Trim(),Acta=new ActaEntrega{FechaFirma=DateTime.Now,ReferenciaDocumento=referencia.Trim(),NombreDocumento=$"Acta-{s.Radicado}.pdf",DocumentoFirmado=documento,HuellaSha256=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(documento))}};
        db.Prestamos.Add(p);s.Estado=EstadoSolicitudPrestamo.Materializada;s.FechaActualizacion=DateTime.Now;foreach(var d in s.Detalles)d.CantidadReservada=0;Evento(s,usuario,"Entrega completa registrada con acta firmada");await db.SaveChangesAsync();return Resultado.Ok(id,$"Préstamo PRE-{p.Id:D6} creado.");
    });
    public Task<Resultado> Programar(int usuario,int id,DateOnly fecha) => Transaccion(async()=> {
        var p=await db.Prestamos.Include(x=>x.Solicitud).SingleOrDefaultAsync(x=>x.Id==id);
        if(p==null || !Abierto(p) || fecha<Hoy)return Resultado.Error("Préstamo no activo o fecha inválida.");
        p.FechaProgramadaDevolucion=fecha;Evento(p.Solicitud,usuario,$"Devolución programada: {fecha:yyyy-MM-dd}");await db.SaveChangesAsync();return Resultado.Ok(p.SolicitudPrestamoId,"Fecha programada. No modifica el vencimiento contractual.");
    });
    public Task<Resultado> Devolver(int usuario,int id,Dictionary<int,string>? condiciones,Dictionary<int,string>? novedades,string? observacion) => Transaccion(async()=> {
        var p=await db.Prestamos.Include(x=>x.Solicitud).Include(x=>x.Detalles).ThenInclude(x=>x.Equipo).SingleOrDefaultAsync(x=>x.Id==id);
        if(p==null || !Abierto(p) || await db.Devoluciones.AnyAsync(d=>d.PrestamoId==id))return Resultado.Error("Préstamo no abierto o ya devuelto.");
        condiciones??=new();novedades??=new();
        var esperados=p.Detalles.Select(d=>d.EquipoId).ToHashSet();
        if(condiciones.Keys.Any(k=>!esperados.Contains(k)) || novedades.Keys.Any(k=>!esperados.Contains(k)) || condiciones.Values.Any(c=>c is not ("Apto" or "Danado" or "Faltante")) || novedades.Values.Any(n=>n?.Length>500) || observacion?.Length>2000)return Resultado.Error("Inspección inválida.");
        var faltantes=esperados.Where(k=>!condiciones.ContainsKey(k)||condiciones[k]=="Faltante").ToList();
        if(faltantes.Count>0){if(string.IsNullOrWhiteSpace(observacion))return Resultado.Error("Registre la observación de faltantes.");p.ObservacionesFaltantes=$"{p.ObservacionesFaltantes}\n{DateTime.Now:g}: faltantes {string.Join(", ",faltantes)}. {observacion.Trim()}";if(p.EstadoActual==EstadoPrestamo.Vencido)p.Estado=EstadoPrestamo.Vencido;Evento(p.Solicitud,usuario,"Recepción incompleta: préstamo permanece abierto");await db.SaveChangesAsync();return Resultado.Ok(p.SolicitudPrestamoId,"Faltantes registrados; no se cerró el préstamo ni se liberaron equipos.");}
        var dev=new Devolucion{PrestamoId=id,ResponsableId=usuario,FechaRealRecepcion=DateTime.Now,Tardia=Hoy>p.FechaDevolucionPrevista};
        foreach(var d in p.Detalles){var c=condiciones[d.EquipoId];var n=novedades.GetValueOrDefault(d.EquipoId);if(c=="Danado" && string.IsNullOrWhiteSpace(n))return Resultado.Error("Describa las novedades de cada equipo dañado.");bool apto=c=="Apto";d.Equipo.Estado=apto?EstadoEquipo.Disponible:EstadoEquipo.EnMantenimiento;dev.Detalles.Add(new DetalleDevolucion{PrestamoId=id,EquipoId=d.EquipoId,Condicion=c,Novedades=n,InspeccionFavorable=apto,DestinoEquipo=d.Equipo.Estado.ToString()});}
        db.Devoluciones.Add(dev);p.Estado=EstadoPrestamo.Devuelto;Evento(p.Solicitud,usuario,"Devolución completa inspeccionada"+(dev.Tardia?" (tardía)":""));await db.SaveChangesAsync();return Resultado.Ok(p.SolicitudPrestamoId,"Devolución registrada; inventario actualizado.");
    });
}
