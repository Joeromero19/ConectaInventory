using System.ComponentModel.DataAnnotations;

namespace ConectaInventory.Models;

public class CategoriaEquipo
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Nombre { get; set; } = string.Empty;
}

public class UbicacionBodega
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Nombre { get; set; } = string.Empty;

    // Solo se puede reubicar hacia ubicaciones habilitadas
    public bool Habilitada { get; set; } = true;
}

public class Donante
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Contacto { get; set; }
}
