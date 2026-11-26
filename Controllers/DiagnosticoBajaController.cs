using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ConectaInventory.Models;
using ConectaInventory.Services;
using ConectaInventory.ViewModels;

namespace ConectaInventory.Controllers;

// HU-0105: Soporte Tecnico diagnostica y solicita la baja; el Administrador decide
public class DiagnosticoBajaController : Controller
{
    private readonly BajasService _servicio;

    public DiagnosticoBajaController(BajasService servicio)
    {
        _servicio = servicio;
    }

    // ----- CU1.03 Registrar diagnostico -----

    [Authorize(Roles = RolNombre.SoporteTecnico)]
    [HttpGet]
    public IActionResult Diagnostico() => View(new DiagnosticoVM());

    [Authorize(Roles = RolNombre.SoporteTecnico)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Diagnostico(DiagnosticoVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var resultado = await _servicio.RegistrarDiagnosticoAsync(vm, User.GetUserId());
        if (!resultado.Exito)
        {
            ModelState.AddModelError("", resultado.Mensaje);
            return View(vm);
        }

        TempData["Ok"] = resultado.Mensaje;
        return RedirectToAction(nameof(Diagnostico));
    }

    // ----- CU1.04 Solicitar baja -----

    [Authorize(Roles = RolNombre.SoporteTecnico)]
    [HttpGet]
    public IActionResult SolicitarBaja(string? serial = null) =>
        View(new SolicitarBajaVM { Serial = serial ?? string.Empty });

    [Authorize(Roles = RolNombre.SoporteTecnico)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SolicitarBaja(SolicitarBajaVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var resultado = await _servicio.SolicitarBajaAsync(vm, User.GetUserId());
        if (!resultado.Exito)
        {
            ModelState.AddModelError("", resultado.Mensaje);
            return View(vm);
        }

        TempData["Ok"] = resultado.Mensaje;
        return RedirectToAction(nameof(SolicitarBaja));
    }

    // ----- CU1.05 Autorizar o rechazar baja -----

    [Authorize(Roles = RolNombre.Administrador)]
    public async Task<IActionResult> Solicitudes() => View(await _servicio.ListarSolicitudesAsync());

    [Authorize(Roles = RolNombre.Administrador)]
    [HttpGet]
    public async Task<IActionResult> Evaluar(int id)
    {
        var solicitud = await _servicio.ObtenerSolicitudAsync(id);
        if (solicitud == null) return NotFound();
        return View(new EvaluarBajaVM { SolicitudId = id, Solicitud = solicitud });
    }

    [Authorize(Roles = RolNombre.Administrador)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Evaluar(EvaluarBajaVM vm)
    {
        if (ModelState.IsValid)
        {
            var resultado = await _servicio.EvaluarAsync(vm, User.GetUserId());
            if (resultado.Exito)
            {
                TempData["Ok"] = resultado.Mensaje;
                // Si se aprobo, se muestra el acta; si se rechazo, vuelve a la lista
                return vm.Aprobar
                    ? RedirectToAction(nameof(Acta), new { id = resultado.Id })
                    : RedirectToAction(nameof(Solicitudes));
            }
            ModelState.AddModelError("", resultado.Mensaje);
        }

        vm.Solicitud = await _servicio.ObtenerSolicitudAsync(vm.SolicitudId);
        if (vm.Solicitud == null) return NotFound();
        return View(vm);
    }

    // Acta individual de una baja autorizada (id = id de la solicitud)
    [Authorize(Roles = RolNombre.Administrador)]
    public async Task<IActionResult> Acta(int id)
    {
        var acta = await _servicio.ObtenerActaAsync(id);
        if (acta == null) return NotFound();
        return View(acta);
    }
}
