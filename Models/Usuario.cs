using System.ComponentModel.DataAnnotations;

namespace ConectaInventory.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string NombreCompleto { get; set; } = string.Empty;

    // Nunca se guarda la clave en texto plano, solo su hash
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public RolUsuario Rol { get; set; }
}
