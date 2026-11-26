using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ConectaInventory.Models;
using ConectaInventory.Services;
using ConectaInventory.ViewModels;

namespace ConectaInventory.Controllers;

// HU-0103: el Administrador genera y reimprime etiquetas; el Bodeguero las escanea y valida
public class IdentificacionController : Controller
{
    private readonly IdentificacionService _servicio;

    public IdentificacionController(IdentificacionService servicio)
    {
        _servicio = servicio;
    }

    [Authorize(Roles = RolNombre.Administrador)]
    public async Task<IActionResult> Index() => View(await _servicio.ListarParaEtiquetadoAsync());

    // CU1.06
    [Authorize(Roles = RolNombre.Administrador)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Generar(int id)
    {
        var resultado = await _servicio.GenerarAsync(id);
        if (!resultado.Exito)
        {
            TempData["Error"] = resultado.Mensaje;
            return RedirectToAction(nameof(Index));
        }
        TempData["Ok"] = resultado.Mensaje;
        return RedirectToAction(nameof(Etiqueta), new { id });
    }

    // CU1.07
    [Authorize(Roles = RolNombre.Administrador)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reimprimir(int id)
    {
        var resultado = await _servicio.ReimprimirAsync(id);
        if (!resultado.Exito)
        {
            TempData["Error"] = resultado.Mensaje;
            return RedirectToAction(nameof(Index));
        }
        TempData["Ok"] = resultado.Mensaje;
        return RedirectToAction(nameof(Etiqueta), new { id });
    }

    // Etiqueta lista para imprimir desde el navegador
    [Authorize(Roles = RolNombre.Administrador)]
    public async Task<IActionResult> Etiqueta(int id)
    {
        var equipo = await _servicio.ObtenerConQrAsync(id);
        if (equipo?.CodigoQR == null) return NotFound();

        return View(new EtiquetaVM
        {
            Equipo = equipo,
            Valor = equipo.CodigoQR.Valor,
            Svg = IdentificacionService.GenerarSvg(equipo.CodigoQR.Valor),
            Reimpresiones = equipo.CodigoQR.Reimpresiones
        });
    }

    // CU1.08: un lector USB de QR escribe el codigo como si fuera teclado
    [Authorize(Roles = RolNombre.Bodeguero)]
    [HttpGet]
    public IActionResult Escanear() => View(new EscanearVM());

    [Authorize(Roles = RolNombre.Bodeguero)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Escanear(EscanearVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var (resultado, equipo) = await _servicio.ValidarAsync(vm.Codigo);
        if (!resultado.Exito)
        {
            ModelState.AddModelError("", resultado.Mensaje);
            return View(vm);
        }

        vm.Equipo = equipo;
        return View(vm);
    }
}
