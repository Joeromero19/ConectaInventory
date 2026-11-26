using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConectaInventory.Data;
using ConectaInventory.Models;
using ConectaInventory.ViewModels;

namespace ConectaInventory.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly ConectaInventoryContext _db;
    private readonly IPasswordHasher<Usuario> _hasher;

    public AccountController(ConectaInventoryContext db, IPasswordHasher<Usuario> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginVM { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var login = vm.NombreUsuario.Trim();
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == login);
        var valido = usuario != null &&
            _hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, vm.Clave) != PasswordVerificationResult.Failed;

        if (!valido)
        {
            ModelState.AddModelError("", "Usuario o clave incorrectos.");
            return View(vm);
        }

        // El rol del usuario se guarda como claim y se valida con [Authorize(Roles = ...)]
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario!.Id.ToString()),
            new(ClaimTypes.Name, usuario.NombreCompleto),
            new(ClaimTypes.Role, usuario.Rol.ToString())
        };
        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidad));

        if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied() => View();
}
