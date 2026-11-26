using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using ConectaInventory.Data;
using ConectaInventory.Models;
using ConectaInventory.Services;

var builder = WebApplication.CreateBuilder(args);

// Toda la aplicacion exige sesion iniciada; el login se marca con [AllowAnonymous]
builder.Services.AddControllersWithViews(opciones =>
{
    var politica = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    opciones.Filters.Add(new AuthorizeFilter(politica));
});

// Base de datos: cadena DefaultConnection de appsettings.json
builder.Services.AddDbContext<ConectaInventoryContext>(opciones =>
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Autenticacion por cookie. El rol viaja como claim y se valida en cada controlador
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.LoginPath = "/Account/Login";
        opciones.AccessDeniedPath = "/Account/AccessDenied";
        opciones.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// Servicios de negocio (reglas del Modulo 01)
builder.Services.AddScoped<InventarioService>();
builder.Services.AddScoped<IdentificacionService>();
builder.Services.AddScoped<BajasService>();

var app = builder.Build();

// Crear la base y sembrar catalogos (y usuarios de prueba solo en desarrollo)
using (var alcance = app.Services.CreateScope())
{
    var db = alcance.ServiceProvider.GetRequiredService<ConectaInventoryContext>();
    var hasher = alcance.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();
    await DbInitializer.InicializarAsync(db, hasher, app.Environment.IsDevelopment());
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// El orden importa: primero identificar al usuario y luego autorizar
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
