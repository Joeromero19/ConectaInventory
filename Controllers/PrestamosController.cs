using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConectaInventory.Data;
using ConectaInventory.Models;
using ConectaInventory.Services;
namespace ConectaInventory.Controllers;
[Authorize(Roles=RolNombre.Beneficiario+","+RolNombre.Administrador+","+RolNombre.Bodeguero)]
[AutoValidateAntiforgeryToken]
public class PrestamosController(ConectaInventoryContext db,PrestamoService servicio):Controller
{
    int UsuarioId=>User.GetUserId();
    bool Admin=>User.IsInRole(RolNombre.Administrador);
    bool Benef=>User.IsInRole(RolNombre.Beneficiario);
    bool Bodega=>User.IsInRole(RolNombre.Bodeguero);
    IQueryable<SolicitudPrestamo> Autorizadas(){var uid=UsuarioId;return Benef?db.SolicitudesPrestamo.Where(s=>s.Beneficiario.UsuarioId==uid):db.SolicitudesPrestamo;}
    [HttpGet] public async Task<IActionResult> Index(string? estado,int? beneficiario,DateOnly? desde,DateOnly? hasta,int? categoria)
    {
        var q=Autorizadas().Include(s=>s.Beneficiario).ThenInclude(b=>b.Usuario).Include(s=>s.Prestamo).AsQueryable();
        if(beneficiario.HasValue && !Benef)q=q.Where(s=>s.BeneficiarioId==beneficiario);
        if(desde.HasValue)q=q.Where(s=>s.FechaInicioPrevista>=desde);if(hasta.HasValue)q=q.Where(s=>s.FechaFinPrevista<=hasta);
        if(categoria.HasValue)q=q.Where(s=>s.Detalles.Any(d=>d.CategoriaEquipoId==categoria));
        var lista=await q.AsNoTracking().OrderByDescending(s=>s.Id).ToListAsync();
        if(!string.IsNullOrWhiteSpace(estado))lista=lista.Where(s=>s.Estado.ToString()==estado||s.Prestamo?.EstadoActual.ToString()==estado).ToList();
        ViewBag.Categorias=await db.Categorias.AsNoTracking().ToListAsync();ViewBag.Beneficiarios=Benef?new List<Beneficiario>():await db.Beneficiarios.Include(b=>b.Usuario).AsNoTracking().ToListAsync();return View(lista);
    }
    [HttpGet] public async Task<IActionResult> Detalle(int id)
    {
        var s=await Autorizadas().AsNoTracking().Include(x=>x.Beneficiario).ThenInclude(b=>b.Usuario).Include(x=>x.UsuarioDecision).Include(x=>x.Detalles).ThenInclude(d=>d.Categoria).Include(x=>x.Prestamo).ThenInclude(p=>p!.Detalles).ThenInclude(d=>d.Equipo).Include(x=>x.Prestamo).ThenInclude(p=>p!.Entrega).ThenInclude(e=>e!.Acta).Include(x=>x.Prestamo).ThenInclude(p=>p!.Entrega).ThenInclude(e=>e!.Responsable).Include(x=>x.Prestamo).ThenInclude(p=>p!.Devolucion).ThenInclude(d=>d!.Detalles).ThenInclude(d=>d.Equipo).Include(x=>x.Prestamo).ThenInclude(p=>p!.Devolucion).ThenInclude(d=>d!.Responsable).AsSplitQuery().SingleOrDefaultAsync(x=>x.Id==id);
        if(s==null)return NotFound();
        ViewBag.Eventos=await db.EventosPrestamo.Where(e=>e.SolicitudPrestamoId==id).Include(e=>e.Usuario).AsNoTracking().OrderBy(e=>e.Fecha).ToListAsync();
        ViewBag.Disponibilidad=await servicio.Disponibilidad(s.FechaInicioPrevista,s.FechaFinPrevista,s.Id);
        if(Bodega && s.Estado==EstadoSolicitudPrestamo.Aprobada){var cats=s.Detalles.Select(d=>d.CategoriaEquipoId).ToList();ViewBag.Equipos=await db.Equipos.Include(e=>e.Categoria).Include(e=>e.CodigoQR).AsNoTracking().Where(e=>cats.Contains(e.CategoriaEquipoId)&&e.Estado==EstadoEquipo.Disponible&&e.CodigoQR!=null).OrderBy(e=>e.Serial).ToListAsync();}
        return View(s);
    }
    [Authorize(Roles=RolNombre.Beneficiario),HttpGet] public async Task<IActionResult> Editar(int id=0)
    {
        var hoy=DateOnly.FromDateTime(DateTime.Today);var s=id==0?new SolicitudPrestamo{FechaInicioPrevista=hoy,FechaFinPrevista=hoy.AddDays(1)}:await Autorizadas().AsNoTracking().Include(x=>x.Detalles).SingleOrDefaultAsync(x=>x.Id==id);
        if(s==null || (id!=0 && s.Estado is not (EstadoSolicitudPrestamo.Pendiente or EstadoSolicitudPrestamo.PendienteAjuste or EstadoSolicitudPrestamo.EnRevision)))return NotFound();
        ViewBag.Categorias=await db.Categorias.AsNoTracking().OrderBy(c=>c.Nombre).ToListAsync();ViewBag.Disponibilidad=await servicio.Disponibilidad(s.FechaInicioPrevista,s.FechaFinPrevista,id);return View(s);
    }
    [HttpGet] public async Task<IActionResult> Disponibilidad(DateOnly inicio,DateOnly fin,int id=0){if(fin<inicio || inicio==default)return BadRequest();if(id!=0&&!await Autorizadas().AnyAsync(s=>s.Id==id))return NotFound();var datos=await servicio.Disponibilidad(inicio,fin,id);return Json(datos);}
    IActionResult Respuesta(Resultado r,int id,string fallo="Detalle"){TempData[r.Exito?"Ok":"Error"]=r.Mensaje;return RedirectToAction(r.Exito?"Detalle":fallo,new{id=r.Exito?r.Id:id});}
    [Authorize(Roles=RolNombre.Beneficiario),HttpPost] public async Task<IActionResult> Guardar(int id,DateOnly inicio,DateOnly fin,string? proposito,string? institucion,Dictionary<int,int>? cantidades){if(!ModelState.IsValid){TempData["Error"]="Datos inválidos.";return RedirectToAction("Editar",new{id});}return Respuesta(await servicio.GuardarSolicitud(UsuarioId,id,inicio,fin,proposito,institucion,cantidades),id,"Editar");}
    [Authorize(Roles=RolNombre.Beneficiario+","+RolNombre.Administrador),HttpPost] public async Task<IActionResult> Cancelar(int id)=>!ModelState.IsValid?BadRequest():Respuesta(await servicio.Cancelar(UsuarioId,id,Admin),id);
    [Authorize(Roles=RolNombre.Administrador),HttpPost] public async Task<IActionResult> Evaluar(int id,string decision,string? motivo)=>!ModelState.IsValid?BadRequest():Respuesta(await servicio.Evaluar(UsuarioId,id,decision,motivo),id);
    [Authorize(Roles=RolNombre.Bodeguero),HttpGet] public async Task<IActionResult> ActaBorrador(int id)
    {
        var s=await db.SolicitudesPrestamo.AsNoTracking().Include(x=>x.Beneficiario).ThenInclude(b=>b.Usuario).Include(x=>x.Detalles).ThenInclude(d=>d.Categoria).SingleOrDefaultAsync(x=>x.Id==id && x.Estado==EstadoSolicitudPrestamo.Aprobada);
        return s==null?NotFound():View(s);
    }
    [HttpGet] public async Task<IActionResult> DescargarActa(int id)
    {
        var acta=await Autorizadas().Where(s=>s.Id==id && s.Prestamo!=null && s.Prestamo.Entrega!=null && s.Prestamo.Entrega.Acta!=null).Select(s=>s.Prestamo!.Entrega!.Acta!).AsNoTracking().SingleOrDefaultAsync();
        if(acta==null || acta.DocumentoFirmado.Length==0)return NotFound();
        return File(acta.DocumentoFirmado,"application/pdf",acta.NombreDocumento);
    }
    [Authorize(Roles=RolNombre.Bodeguero),HttpPost,RequestSizeLimit(6*1024*1024)] public async Task<IActionResult> Entregar(int id,int receptor,string? identidad,string? referencia,bool firma,bool inspeccion,int[]? equipos,IFormFile? documento)
    {
        if(!ModelState.IsValid || documento==null || documento.Length<5 || documento.Length>5*1024*1024){TempData["Error"]="Datos inválidos o falta adjuntar el PDF firmado (máximo 5 MB).";return RedirectToAction("Detalle",new{id});}
        using var ms=new MemoryStream();await documento.CopyToAsync(ms);
        return Respuesta(await servicio.Entregar(UsuarioId,id,receptor,identidad,referencia,firma,inspeccion,equipos,ms.ToArray()),id);
    }
    [Authorize(Roles=RolNombre.Bodeguero),HttpPost] public async Task<IActionResult> Programar(int id,int prestamoId,DateOnly fecha){if(!ModelState.IsValid){TempData["Error"]="Fecha inválida.";return RedirectToAction("Detalle",new{id});}return Respuesta(await servicio.Programar(UsuarioId,prestamoId,fecha),id);}
    [Authorize(Roles=RolNombre.Bodeguero),HttpPost] public async Task<IActionResult> Devolver(int id,int prestamoId,Dictionary<int,string>? condiciones,Dictionary<int,string>? novedades,string? observacion){if(!ModelState.IsValid){TempData["Error"]="Datos inválidos.";return RedirectToAction("Detalle",new{id});}return Respuesta(await servicio.Devolver(UsuarioId,prestamoId,condiciones,novedades,observacion),id);}
}
