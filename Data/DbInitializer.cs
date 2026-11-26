using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ConectaInventory.Models;

namespace ConectaInventory.Data;

public static class DbInitializer
{
    // Crea la base si no existe y siembra catalogos basicos.
    // Los usuarios de prueba solo se crean en desarrollo.
    public static async Task InicializarAsync(
        ConectaInventoryContext db, IPasswordHasher<Usuario> hasher, bool sembrarUsuariosDemo)
    {
        if (!db.Database.GetMigrations().Any())
            throw new InvalidOperationException("No hay migraciones. Genere la migración inicial o conserve las de su rama antes de iniciar.");
        await db.Database.MigrateAsync();

        if (!await db.Categorias.AnyAsync())
        {
            db.Categorias.AddRange(
                new CategoriaEquipo { Nombre = "Portatil" },
                new CategoriaEquipo { Nombre = "Tableta" },
                new CategoriaEquipo { Nombre = "Componente de red" });
        }

        if (!await db.Ubicaciones.AnyAsync())
        {
            db.Ubicaciones.AddRange(
                new UbicacionBodega { Nombre = "Zona de recepcion" },
                new UbicacionBodega { Nombre = "Estante A1" },
                new UbicacionBodega { Nombre = "Estante A2" },
                new UbicacionBodega { Nombre = "Estante B1" });
        }

        if (sembrarUsuariosDemo && !await db.Usuarios.AnyAsync())
        {
            // SOLO DESARROLLO: claves de prueba, cambiarlas antes de cualquier despliegue
            db.Usuarios.AddRange(
                CrearUsuario(hasher, "bodeguero", "Bodeguero Demo", "Bodeguero123*", RolUsuario.Bodeguero),
                CrearUsuario(hasher, "admin", "Administrador Demo", "Admin123*", RolUsuario.Administrador),
                CrearUsuario(hasher, "soporte", "Soporte Tecnico Demo", "Soporte123*", RolUsuario.SoporteTecnico));
        }

        await db.SaveChangesAsync();
        if (sembrarUsuariosDemo)
        {
            foreach (var demo in new[] { (Login: "beneficiario", Nombre: "Beneficiario Demo", Clave: "Beneficiario123*"), (Login: "beneficiario2", Nombre: "Beneficiario Demo 2", Clave: "Beneficiario234*") })
            {
                var usuario = await db.Usuarios.SingleOrDefaultAsync(u=>u.NombreUsuario==demo.Login);
                if (usuario == null)
                {
                    usuario = CrearUsuario(hasher, demo.Login, demo.Nombre, demo.Clave, RolUsuario.Beneficiario);
                    db.Usuarios.Add(usuario);
                    await db.SaveChangesAsync();
                }
                if (usuario.Rol == RolUsuario.Beneficiario && !await db.Beneficiarios.AnyAsync(b=>b.UsuarioId==usuario.Id))
                {
                    db.Beneficiarios.Add(new Beneficiario {UsuarioId=usuario.Id});
                    await db.SaveChangesAsync();
                }
            }
        }
    }

    private static Usuario CrearUsuario(IPasswordHasher<Usuario> hasher, string login, string nombre, string clave, RolUsuario rol)
    {
        var usuario = new Usuario { NombreUsuario = login, NombreCompleto = nombre, Rol = rol };
        usuario.PasswordHash = hasher.HashPassword(usuario, clave);
        return usuario;
    }
}
