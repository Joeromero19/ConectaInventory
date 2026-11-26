using System.Data;
using ConectaInventory.Data;
using ConectaInventory.Models;
using Microsoft.EntityFrameworkCore;
namespace ConectaInventory.Services;
public class MantenimientoService(ConectaInventoryContext db) {
 int Actor; bool Admin;
 public void Identificar(int actor,bool admin){Actor=actor;Admin=admin;}
 async Task<bool> Puede(Mantenimiento m)=>Admin || (m.TecnicoId==Actor && await db.PerfilesTecnicos.AnyAsync(p=>p.UsuarioId==Actor&&p.Activo));
 Resultado Error(string mensaje,int? id=null)=>Resultado.Error(mensaje);
 Resultado Ok(string mensaje,int? id=null)=>Resultado.Ok(id??0,mensaje);
 void Evento(Mantenimiento m,string detalle)=>m.Historial.Add(new HistorialMantenimiento{ResponsableId=Actor,Detalle=detalle});
 async Task<bool> Guardar(){try{await db.SaveChangesAsync();return true;}catch(DbUpdateException){return false;}}
 public async Task<Resultado> Reportar(string serial,string falla,int? detalleDevolucionId) {
 if(string.IsNullOrWhiteSpace(serial)||string.IsNullOrWhiteSpace(falla)||falla.Length>1000)return Error("Complete serial y falla (máximo 1000 caracteres).");
 await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
 var e=await db.Equipos.FirstOrDefaultAsync(x=>x.Serial==serial.Trim().ToUpperInvariant());
 if(e==null || e.Estado is EstadoEquipo.BajaDefinitiva or EstadoEquipo.Prestado or EstadoEquipo.Recibido)return Error("Equipo inexistente o estado no permitido.");
 if(await db.DetallesPrestamo.AnyAsync(p=>p.EquipoId==e.Id && (p.Prestamo.Estado==EstadoPrestamo.Activo || p.Prestamo.Estado==EstadoPrestamo.Vencido)))return Error("Existe un préstamo pendiente.");
 if(await db.Mantenimientos.AnyAsync(m=>m.EquipoId==e.Id&&m.Abierto))return Error("Ya existe un mantenimiento abierto.");
 if(await db.SolicitudesBaja.AnyAsync(b=>b.EquipoId==e.Id && b.Estado==EstadoSolicitudBaja.Solicitada))return Error("Existe solicitud de baja pendiente.");
 if(!detalleDevolucionId.HasValue && e.Estado==EstadoEquipo.EnMantenimiento) detalleDevolucionId=await db.DetallesDevolucion.Where(d=>d.EquipoId==e.Id&&!d.InspeccionFavorable&&!db.Mantenimientos.Any(m=>m.DetalleDevolucionId==d.Id)).OrderByDescending(d=>d.Id).Select(d=>(int?)d.Id).FirstOrDefaultAsync();
 if(detalleDevolucionId.HasValue && e.Estado!=EstadoEquipo.EnMantenimiento)return Error("El equipo no está remitido a mantenimiento.");
 if(detalleDevolucionId.HasValue && !await db.DetallesDevolucion.AnyAsync(p=>p.Id==detalleDevolucionId&&p.EquipoId==e.Id&&!p.InspeccionFavorable&&!db.Mantenimientos.Any(m=>m.DetalleDevolucionId==p.Id)))return Error("Préstamo devuelto no válido.");
 var m=new Mantenimiento{EquipoId=e.Id,ReportadoPorId=Actor,Falla=falla.Trim(),DetalleDevolucionId=detalleDevolucionId};
 Evento(m,"Reporte registrado"); db.Mantenimientos.Add(m); e.Estado=EstadoEquipo.EnMantenimiento;
 if(!await Guardar())return Error("No se pudo registrar el reporte."); await tx.CommitAsync(); return Ok("Reporte registrado",m.Id);
 }
 public async Task<Resultado> Asignar(int id,int tecnicoId) {
 await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
 var m=await db.Mantenimientos.Include(x=>x.Historial).FirstOrDefaultAsync(x=>x.Id==id);
 if(!Admin)return Error("Solo administrador.",id);
 if(m==null||!m.Abierto)return Error("Mantenimiento no disponible.");
 if(!await db.Usuarios.AnyAsync(u=>u.Id==tecnicoId&&u.Rol==RolUsuario.SoporteTecnico))return Error("Técnico inexistente",id);
 var perfil=await db.PerfilesTecnicos.FirstOrDefaultAsync(p=>p.UsuarioId==tecnicoId);
 if(perfil==null||!perfil.Activo||!perfil.Disponible)return Error("Técnico no habilitado o no disponible",id);
 Evento(m,$"Asignación: {m.TecnicoId?.ToString()??"sin asignar"} → {tecnicoId}"); m.TecnicoId=tecnicoId; if(m.Estado==EstadoMantenimiento.Pendiente)m.Estado=EstadoMantenimiento.Asignado;
 if(!await Guardar())return Error("Error al guardar",id); await tx.CommitAsync(); return Ok("Asignación registrada",id);
 }
 public async Task<Resultado> Actualizar(int id,EstadoMantenimiento estado,string observacion) {
 await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
 var m=await db.Mantenimientos.Include(x=>x.Equipo).Include(x=>x.Historial).FirstOrDefaultAsync(x=>x.Id==id);
 if(m==null)return Error("Reporte inexistente."); if(!await Puede(m))return Error("No tiene permiso.",id);
 if(!m.Abierto||m.Equipo.Estado!=EstadoEquipo.EnMantenimiento||string.IsNullOrWhiteSpace(observacion)||observacion.Length>1000)return Error("Estado u observación inválidos",id);
 if(!((m.Estado==EstadoMantenimiento.Asignado&&estado==EstadoMantenimiento.EnRevision)||(m.Estado==EstadoMantenimiento.EnRevision&&estado is EstadoMantenimiento.Reparado or EstadoMantenimiento.Irreparable)))return Error("Transición no permitida",id);
 if(await db.DetallesPrestamo.AnyAsync(p=>p.EquipoId==m.EquipoId && (p.Prestamo.Estado==EstadoPrestamo.Activo || p.Prestamo.Estado==EstadoPrestamo.Vencido)))return Error("Existe un préstamo pendiente",id);
 Evento(m,$"{(Admin?"Intervención administrativa: ":"")}{m.Estado} → {estado}: {observacion.Trim()}"); m.Estado=estado;
 if(estado is EstadoMantenimiento.Reparado or EstadoMantenimiento.Irreparable){m.Abierto=false; m.Equipo.Estado=estado==EstadoMantenimiento.Reparado?EstadoEquipo.Disponible:EstadoEquipo.EnDiagnostico; db.Diagnosticos.Add(new DiagnosticoTecnico{EquipoId=m.EquipoId,Fecha=DateTime.Now,Hallazgos=observacion.Trim(),ResponsableId=Actor,Resultado=estado==EstadoMantenimiento.Reparado?ResultadoDiagnostico.Apto:ResultadoDiagnostico.NoApto});}
 if(!await Guardar())return Error("Error al guardar",id); await tx.CommitAsync(); return Ok("Estado actualizado",id);
 }
 public async Task<Resultado> Repuesto(int id,string nombre,int cantidad,decimal costoUnitario,string actividad) {
 await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
 var m=await db.Mantenimientos.Include(x=>x.Historial).Include(x=>x.Repuestos).FirstOrDefaultAsync(x=>x.Id==id);
 if(m==null)return Error("Reporte inexistente."); if(!await Puede(m))return Error("No tiene permiso.",id);
 if(m.Estado!=EstadoMantenimiento.EnRevision||string.IsNullOrWhiteSpace(actividad)||actividad.Length>1000||(!string.IsNullOrWhiteSpace(nombre)&&(nombre.Length>150||cantidad<1||costoUnitario<0||costoUnitario>999999999)))return Error("Datos inválidos o mantenimiento fuera de revisión",id);
 if(!string.IsNullOrWhiteSpace(nombre))m.Repuestos.Add(new RepuestoMantenimiento{Nombre=nombre.Trim(),Cantidad=cantidad,CostoUnitario=costoUnitario}); Evento(m,actividad.Trim());
 if(!await Guardar())return Error("Error al guardar",id); await tx.CommitAsync(); return Ok("Intervención registrada",id);
 }
}
