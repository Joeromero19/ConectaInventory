using System.Security.Claims;
using ConectaInventory.Data;
using ConectaInventory.Models;
using ConectaInventory.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace ConectaInventory.Controllers;
[Authorize(Roles=RolNombre.Administrador+","+RolNombre.SoporteTecnico)]
[AutoValidateAntiforgeryToken]
public class MantenimientoController(ConectaInventoryContext db,MantenimientoService servicio,IPasswordHasher<Usuario> hasher):Controller {
 int Actor=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 bool Admin=>User.IsInRole(RolNombre.Administrador);
 async Task<bool> Habilitado()=>Admin||await db.PerfilesTecnicos.AnyAsync(p=>p.UsuarioId==Actor&&p.Activo);
 async Task<IActionResult> Ejecutar(Func<Task<Resultado>> accion,int? id=null){if(!await Habilitado())return Forbid(); servicio.Identificar(Actor,Admin); var r=ModelState.IsValid?await accion():Resultado.Error("Datos inválidos.");TempData[r.Exito?"Ok":"Error"]=r.Mensaje;return r.Exito&&r.Id>0?RedirectToAction(nameof(Detalle),new{id=r.Id}):id.HasValue?RedirectToAction(nameof(Detalle),new{id}):RedirectToAction(nameof(Index));}
 public async Task<IActionResult> Index(){if(!await Habilitado())return Forbid(); ViewBag.Remisiones=await db.DetallesDevolucion.Include(d=>d.Equipo).Where(d=>!d.InspeccionFavorable&&d.Equipo.Estado==EstadoEquipo.EnMantenimiento&&!db.Mantenimientos.Any(m=>m.DetalleDevolucionId==d.Id)).ToListAsync();return View(await db.Mantenimientos.Include(m=>m.Equipo).Include(m=>m.Tecnico).OrderByDescending(m=>m.Fecha).ToListAsync());}
 public async Task<IActionResult> Detalle(int id){if(!await Habilitado())return Forbid();var m=await db.Mantenimientos.Include(x=>x.Equipo).Include(x=>x.Historial).Include(x=>x.Repuestos).FirstOrDefaultAsync(x=>x.Id==id);if(m==null)return NotFound();ViewBag.Tecnicos=await db.PerfilesTecnicos.Include(p=>p.Usuario).Where(p=>p.Activo&&p.Disponible).ToListAsync();return View(m);}
 [HttpPost] public Task<IActionResult> Reportar(string serial,string falla,int? detalleDevolucionId)=>Ejecutar(()=>servicio.Reportar(serial,falla,detalleDevolucionId));
 [HttpPost,Authorize(Roles=RolNombre.Administrador)] public Task<IActionResult> Asignar(int id,int tecnicoId)=>Ejecutar(()=>servicio.Asignar(id,tecnicoId),id);
 [HttpPost] public Task<IActionResult> Actualizar(int id,EstadoMantenimiento estado,string observacion)=>Ejecutar(()=>servicio.Actualizar(id,estado,observacion),id);
 [HttpPost] public Task<IActionResult> Repuesto(int id,string? nombre,int cantidad,decimal costoUnitario,string actividad)=>Ejecutar(()=>servicio.Repuesto(id,nombre??"",cantidad,costoUnitario,actividad),id);
 public async Task<IActionResult> Alertas(bool vencidas=false){if(!await Habilitado())return Forbid();var q=db.AlertasPreventivas.Include(a=>a.Equipo).Where(a=>!a.Atendida&&a.Equipo.Estado!=EstadoEquipo.BajaDefinitiva);if(vencidas)q=q.Where(a=>a.FechaProgramada<=DateTime.Today);return View(await q.OrderBy(a=>a.FechaProgramada).ToListAsync());}
 [Authorize(Roles=RolNombre.Administrador)] public async Task<IActionResult> Tecnicos()=>View(await db.PerfilesTecnicos.Include(p=>p.Usuario).ToListAsync());
 [HttpPost,Authorize(Roles=RolNombre.Administrador)] public async Task<IActionResult> Perfil(int id,string especialidad,bool activo,bool disponible){var p=await db.PerfilesTecnicos.FindAsync(id);if(p==null)return NotFound();if(string.IsNullOrWhiteSpace(especialidad)||especialidad.Length>100)return BadRequest();p.Especialidad=especialidad.Trim();p.Activo=activo;p.Disponible=disponible;await db.SaveChangesAsync();return RedirectToAction(nameof(Tecnicos));}
 [HttpPost,Authorize(Roles=RolNombre.Administrador)] public async Task<IActionResult> CrearTecnico(string login,string nombre,string clave,string especialidad){if(!ModelState.IsValid||string.IsNullOrWhiteSpace(login)||login.Length>50||string.IsNullOrWhiteSpace(nombre)||nombre.Length>100||string.IsNullOrWhiteSpace(especialidad)||especialidad.Length>100||clave==null||clave.Length<10||await db.Usuarios.AnyAsync(u=>u.NombreUsuario==login.Trim())){TempData["Error"]="Datos inválidos o usuario existente.";return RedirectToAction(nameof(Tecnicos));}var u=new Usuario{NombreUsuario=login.Trim(),NombreCompleto=nombre.Trim(),Rol=RolUsuario.SoporteTecnico};u.PasswordHash=hasher.HashPassword(u,clave);db.PerfilesTecnicos.Add(new PerfilTecnico{Usuario=u,Especialidad=especialidad.Trim()});await db.SaveChangesAsync();return RedirectToAction(nameof(Tecnicos));}
}
