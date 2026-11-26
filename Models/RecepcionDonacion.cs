using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConectaInventory.Models;

public class RecepcionDonacion
{
    public int Id { get; set; }

    // El radicado se deriva del Id: es unico y solo existe despues de guardar
    [NotMapped]
    public string Radicado => $"REC-{Id:D6}";

    public int DonanteId { get; set; }
    public Donante Donante { get; set; } = null!;

    public DateTime Fecha { get; set; }

    [Required, StringLength(50)]
    public string Lote { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    public int CantidadFisica { get; set; }

    public EstadoRecepcion Estado { get; set; } = EstadoRecepcion.EnRecepcion;

    public int ResponsableId { get; set; }
    public Usuario Responsable { get; set; } = null!;

    public ICollection<Equipo> Equipos { get; set; } = new List<Equipo>();
}
