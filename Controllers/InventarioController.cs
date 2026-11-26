using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ConectaInventory.Models;
using ConectaInventory.Services;
using ConectaInventory.ViewModels;

namespace ConectaInventory.Controllers
{
    // Recepcion, alta, reubicacion y stock. Los permisos siguen el documento (seccion 9.4.2):
    // Bodeguero: recepcion, reubicacion y consulta | Administrador: alta | todos: consulta de stock
    public class InventarioController : Controller
    {
        private readonly InventarioService _servicio;

        public InventarioController(InventarioService servicio)
        {
            _servicio = servicio;
        }

        // ----- CU1.01 Registrar recepcion de donacion -----

        [Authorize(Roles = RolNombre.Bodeguero)]
        [HttpGet]
        public IActionResult Recepcion() => View(new RecepcionVM());

        [Authorize(Roles = RolNombre.Bodeguero)]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Recepcion(RecepcionVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var resultado = await _servicio.RegistrarRecepcionAsync(vm, User.GetUserId());
            if (!resultado.Exito)
            {
                ModelState.AddModelError("", resultado.Mensaje);
                return View(vm);
            }

            // La confirmacion solo se muestra despues de guardar
            TempData["Ok"] = resultado.Mensaje;
            return RedirectToAction(nameof(Recepcion));
        }

        // ----- CU1.02 Dar de alta equipo -----

        [Authorize(Roles = RolNombre.Administrador)]
        [HttpGet]
        public async Task<IActionResult> Alta()
        {
            var vm = new AltaVM();
            await CargarListasAltaAsync(vm);
            return View(vm);
        }

        [Authorize(Roles = RolNombre.Administrador)]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Alta(AltaVM vm)
        {
            if (ModelState.IsValid)
            {
                var resultado = await _servicio.DarDeAltaAsync(vm);
                if (resultado.Exito)
                {
                    TempData["Ok"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Alta));
                }
                ModelState.AddModelError("", resultado.Mensaje);
            }

            await CargarListasAltaAsync(vm);
            return View(vm);
        }

        // ----- CU1.09 Registrar reubicacion -----

        [Authorize(Roles = RolNombre.Bodeguero)]
        [HttpGet]
        public async Task<IActionResult> Reubicar(string? serial = null)
        {
            var vm = new ReubicarVM { Serial = serial ?? string.Empty };
            vm.Ubicaciones = await _servicio.UbicacionesHabilitadasAsync();
            return View(vm);
        }

        [Authorize(Roles = RolNombre.Bodeguero)]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reubicar(ReubicarVM vm)
        {
            if (ModelState.IsValid)
            {
                var resultado = await _servicio.ReubicarAsync(vm, User.GetUserId());
                if (resultado.Exito)
                {
                    TempData["Ok"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Reubicar));
                }
                ModelState.AddModelError("", resultado.Mensaje);
            }

            vm.Ubicaciones = await _servicio.UbicacionesHabilitadasAsync();
            return View(vm);
        }

        // ----- CU1.10 Consultar stock (cualquier rol con sesion) -----

        [Authorize(Roles = RolNombre.Bodeguero + "," + RolNombre.Administrador + "," + RolNombre.SoporteTecnico)]
        [HttpGet]
        public async Task<IActionResult> Stock(StockVM vm, [FromServices] PrestamoService prestamos)
        {
            vm.Categorias = await _servicio.CategoriasAsync();
            vm.Ubicaciones = await _servicio.UbicacionesTodasAsync();
            vm.Resultados = await _servicio.ConsultarStockAsync(vm);
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var disponibilidad = await prestamos.Disponibilidad(hoy, hoy);
            vm.DisponibilidadHoy = vm.Categorias.ToDictionary(c=>c.Text,c=>disponibilidad.GetValueOrDefault(int.Parse(c.Value)));
            return View(vm);
        }

        private async Task CargarListasAltaAsync(AltaVM vm)
        {
            vm.Recepciones = await _servicio.RecepcionesPendientesAsync();
            vm.Categorias = await _servicio.CategoriasAsync();
            vm.Ubicaciones = await _servicio.UbicacionesHabilitadasAsync();
        }
    }
}
