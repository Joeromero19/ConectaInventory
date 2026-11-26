using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ConectaInventory.Models;
public enum EstadoMantenimiento { Pendiente, Asignado, EnRevision, Reparado, Irreparable }
public class Mantenimiento {
 public int Id {get;set;}
 public int EquipoId {get;set;}
 public Equipo Equipo {get;set;} = null!;
 public int ReportadoPorId {get;set;}
 public Usuario ReportadoPor {get;set;} = null!;
 public int? TecnicoId {get;set;}
 public Usuario? Tecnico {get;set;}
 [Timestamp] public byte[] Version {get;set;} = null!;
 public int? DetalleDevolucionId {get;set;}
 public DetalleDevolucion? DetalleDevolucion {get;set;}
 [Required, StringLength(1000)] public string Falla {get;set;} = "";
 public DateTime Fecha {get;set;} = DateTime.Now;
 public EstadoMantenimiento Estado {get;set;}
 public bool Abierto {get;set;} = true;
 public List<HistorialMantenimiento> Historial {get;set;} = new();
 public List<RepuestoMantenimiento> Repuestos {get;set;} = new();
}
public class HistorialMantenimiento {
 public int Id {get;set;}
 public int MantenimientoId {get;set;}
 public int ResponsableId {get;set;}
 public Usuario Responsable {get;set;} = null!;
 public DateTime Fecha {get;set;} = DateTime.Now;
 [Required, StringLength(1500)] public string Detalle {get;set;} = "";
}
public class RepuestoMantenimiento {
 public int Id {get;set;}
 public int MantenimientoId {get;set;}
 [Required, StringLength(150)] public string Nombre {get;set;} = "";
 public int Cantidad {get;set;}
 [Column(TypeName="decimal(18,2)")] public decimal CostoUnitario {get;set;}
}
public class PerfilTecnico {
 public int Id {get;set;}
 public int UsuarioId {get;set;}
 public Usuario Usuario {get;set;} = null!;
 [StringLength(100)] public string Especialidad {get;set;} = "General";
 public bool Activo {get;set;} = true;
 public bool Disponible {get;set;} = true;
}
public class AlertaPreventiva {
 public int Id {get;set;}
 public int EquipoId {get;set;}
 public Equipo Equipo {get;set;} = null!;
 public DateTime FechaProgramada {get;set;}
 public bool Atendida {get;set;}
}
