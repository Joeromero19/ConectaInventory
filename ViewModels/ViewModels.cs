using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using ConectaInventory.Models;

namespace ConectaInventory.ViewModels;

public class LoginVM
{
    [Required(ErrorMessage = "Ingrese el usuario."), Display(Name = "Usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese la clave."), DataType(DataType.Password), Display(Name = "Clave")]
    public string Clave { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

// HU-0101 / CU1.01
public class RecepcionVM
{
    [Required(ErrorMessage = "El donante es obligatorio."), StringLength(100), Display(Name = "Donante")]
    public string DonanteNombre { get; set; } = string.Empty;

    [StringLength(100), Display(Name = "Contacto del donante")]
    public string? DonanteContacto { get; set; }

    [Required(ErrorMessage = "El lote es obligatorio."), StringLength(50), Display(Name = "Lote")]
    public string Lote { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripcion es obligatoria."), StringLength(500), Display(Name = "Descripcion de equipos")]
    public string Descripcion { get; set; } = string.Empty;

    [Range(1, 10000, ErrorMessage = "La cantidad debe estar entre 1 y 10000."), Display(Name = "Cantidad fisica")]
    public int CantidadFisica { get; set; }
}

// HU-0101 / CU1.02
public class AltaVM
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una recepcion."), Display(Name = "Recepcion")]
    public int RecepcionId { get; set; }

    [Required(ErrorMessage = "El serial es obligatorio."), StringLength(100), Display(Name = "Serial")]
    public string Serial { get; set; } = string.Empty;

    [Required(ErrorMessage = "La marca es obligatoria."), StringLength(80), Display(Name = "Marca")]
    public string Marca { get; set; } = string.Empty;

    [Required(ErrorMessage = "El modelo es obligatorio."), StringLength(80), Display(Name = "Modelo")]
    public string Modelo { get; set; } = string.Empty;

    [StringLength(500), Display(Name = "Especificaciones")]
    public string? Especificaciones { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una categoria."), Display(Name = "Categoria")]
    public int CategoriaId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una ubicacion."), Display(Name = "Ubicacion inicial")]
    public int UbicacionId { get; set; }

    [ValidateNever] public List<SelectListItem> Recepciones { get; set; } = new();
    [ValidateNever] public List<SelectListItem> Categorias { get; set; } = new();
    [ValidateNever] public List<SelectListItem> Ubicaciones { get; set; } = new();
}

// HU-0102 / CU1.09
public class ReubicarVM
{
    [Required(ErrorMessage = "Ingrese el serial del equipo."), StringLength(100), Display(Name = "Serial del equipo")]
    public string Serial { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione la ubicacion destino."), Display(Name = "Ubicacion destino")]
    public int UbicacionDestinoId { get; set; }

    [Required(ErrorMessage = "El motivo es obligatorio."), StringLength(300), Display(Name = "Motivo")]
    public string Motivo { get; set; } = string.Empty;

    [ValidateNever] public List<SelectListItem> Ubicaciones { get; set; } = new();
}

// HU-0104 / CU1.10: se llena por query string
public class StockVM
{
    [Display(Name = "Categoria")] public int? CategoriaId { get; set; }
    [Display(Name = "Estado")] public EstadoEquipo? Estado { get; set; }
    [Display(Name = "Ubicacion")] public int? UbicacionId { get; set; }
    [Display(Name = "Solo unidades físicamente disponibles")] public bool SoloDisponibles { get; set; }

    [ValidateNever] public List<SelectListItem> Categorias { get; set; } = new();
    [ValidateNever] public List<SelectListItem> Ubicaciones { get; set; } = new();
    [ValidateNever] public List<StockItem> Resultados { get; set; } = new();
    [ValidateNever] public Dictionary<string,int> DisponibilidadHoy { get; set; } = new();
}

public class StockItem
{
    public int Id { get; set; }
    public string Identificador => $"EQU-{Id:D4}";
    public string Serial { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public EstadoEquipo Estado { get; set; }

    // Disponibilidad efectiva: estado Disponible y sin prestamo activo ni reservado
    public bool Disponible { get; set; }
}

// HU-0103
public class EtiquetaVM
{
    public Equipo Equipo { get; set; } = null!;
    public string Valor { get; set; } = string.Empty;
    public string Svg { get; set; } = string.Empty;
    public int Reimpresiones { get; set; }
}

public class EscanearVM
{
    [Required(ErrorMessage = "Escanee o escriba el codigo."), Display(Name = "Codigo leido")]
    public string Codigo { get; set; } = string.Empty;

    [ValidateNever] public Equipo? Equipo { get; set; }
}

// HU-0105 / CU1.03
public class DiagnosticoVM
{
    [Required(ErrorMessage = "Ingrese el serial del equipo."), StringLength(100), Display(Name = "Serial del equipo")]
    public string Serial { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los hallazgos son obligatorios."), StringLength(1000), Display(Name = "Hallazgos de inspeccion")]
    public string Hallazgos { get; set; } = string.Empty;

    [Display(Name = "Resultado")]
    public ResultadoDiagnostico Resultado { get; set; }
}

// HU-0105 / CU1.04
public class SolicitarBajaVM
{
    [Required(ErrorMessage = "Ingrese el serial del equipo."), StringLength(100), Display(Name = "Serial del equipo")]
    public string Serial { get; set; } = string.Empty;

    [Required(ErrorMessage = "La justificacion es obligatoria."), StringLength(500), Display(Name = "Justificacion tecnica")]
    public string Motivo { get; set; } = string.Empty;
}

// HU-0105 / CU1.05
public class EvaluarBajaVM
{
    public int SolicitudId { get; set; }

    [Display(Name = "Decision")]
    public bool Aprobar { get; set; }

    [StringLength(500), Display(Name = "Motivo de la decision")]
    public string? Motivo { get; set; }

    [ValidateNever] public SolicitudBaja? Solicitud { get; set; }
}
