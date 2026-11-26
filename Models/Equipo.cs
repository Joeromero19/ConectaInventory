using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConectaInventory.Models;

public class Equipo
{
    public int Id { get; set; }

    [NotMapped]
    public string Identificador => $"EQU-{Id:D4}";

    // Serial normalizado (mayusculas, sin espacios en los extremos) y unico
    [Required, StringLength(100)]
    public string Serial { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Marca { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Modelo { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Especificaciones { get; set; }

    public EstadoEquipo Estado { get; set; } = EstadoEquipo.Registrado;

    public int RecepcionDonacionId { get; set; }
    public RecepcionDonacion RecepcionDonacion { get; set; } = null!;

    public int CategoriaEquipoId { get; set; }
    public CategoriaEquipo Categoria { get; set; } = null!;

    public int UbicacionBodegaId { get; set; }
    public UbicacionBodega Ubicacion { get; set; } = null!;

    public CodigoQR? CodigoQR { get; set; }
    public ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();
    public ICollection<DetallePrestamo> DetallesPrestamo { get; set; } = new List<DetallePrestamo>();
    [Timestamp] public byte[] Version { get; set; } = null!;
}

// Un equipo tiene como maximo un QR; reimprimir no cambia el valor
public class CodigoQR
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Valor { get; set; } = string.Empty;

    public int EquipoId { get; set; }
    public Equipo Equipo { get; set; } = null!;

    public int Reimpresiones { get; set; }
    public DateTime? UltimaReimpresion { get; set; }
}

// Historial de traslados: origen, destino, responsable y fecha (HU-0102)
public class MovimientoInventario
{
    public int Id { get; set; }

    public int EquipoId { get; set; }
    public Equipo Equipo { get; set; } = null!;

    public int UbicacionOrigenId { get; set; }
    public UbicacionBodega UbicacionOrigen { get; set; } = null!;

    public int UbicacionDestinoId { get; set; }
    public UbicacionBodega UbicacionDestino { get; set; } = null!;

    public DateTime Fecha { get; set; }

    [Required, StringLength(300)]
    public string Motivo { get; set; } = string.Empty;

    public int ResponsableId { get; set; }
    public Usuario Responsable { get; set; } = null!;
}

