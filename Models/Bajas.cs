using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConectaInventory.Models;

public class DiagnosticoTecnico
{
    public int Id { get; set; }

    public int EquipoId { get; set; }
    public Equipo Equipo { get; set; } = null!;

    public DateTime Fecha { get; set; }

    [Required, StringLength(1000)]
    public string Hallazgos { get; set; } = string.Empty;

    public ResultadoDiagnostico Resultado { get; set; }

    public int ResponsableId { get; set; }
    public Usuario Responsable { get; set; } = null!;
}

public class SolicitudBaja
{
    public int Id { get; set; }

    public int EquipoId { get; set; }
    public Equipo Equipo { get; set; } = null!;

    // Diagnostico que sustenta la solicitud
    public int DiagnosticoTecnicoId { get; set; }
    public DiagnosticoTecnico Diagnostico { get; set; } = null!;

    [Required, StringLength(500)]
    public string Motivo { get; set; } = string.Empty;

    public EstadoSolicitudBaja Estado { get; set; } = EstadoSolicitudBaja.Solicitada;
    public DateTime FechaSolicitud { get; set; }

    public int SolicitanteId { get; set; }
    public Usuario Solicitante { get; set; } = null!;

    // Datos de la decision del administrador
    [StringLength(500)]
    public string? DecisionMotivo { get; set; }
    public DateTime? FechaDecision { get; set; }
    public int? DecididaPorId { get; set; }
    public Usuario? DecididaPor { get; set; }

    public ActaBaja? Acta { get; set; }
}

// Se emite un acta individual por cada baja autorizada (HU-0105)
public class ActaBaja
{
    public int Id { get; set; }

    [NotMapped]
    public string Numero => $"ACT-{Id:D6}";

    public int SolicitudBajaId { get; set; }
    public SolicitudBaja Solicitud { get; set; } = null!;

    public DateTime Fecha { get; set; }
}
