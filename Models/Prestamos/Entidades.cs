using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ConectaInventory.Models;
public enum EstadoSolicitudPrestamo { Borrador, Pendiente, EnRevision, PendienteAjuste, Aprobada, Rechazada, Cancelada, Materializada }
public class Beneficiario {
 public int Id {get;set;}
 public int UsuarioId {get;set;}
 public Usuario Usuario {get;set;}=null!;
 [MaxLength(50)] public string TipoBeneficiario {get;set;}="Individual";
}
public class SolicitudPrestamo {
 public int Id {get;set;}
 [NotMapped] public string Radicado => $"SOL-{Id:D6}";
 public int BeneficiarioId {get;set;}
 public Beneficiario Beneficiario {get;set;}=null!;
 public DateOnly FechaInicioPrevista {get;set;}
 public DateOnly FechaFinPrevista {get;set;}
 public DateTime FechaSolicitud {get;set;}
 public DateTime FechaActualizacion {get;set;}
 public EstadoSolicitudPrestamo Estado {get;set;}
 public int? UsuarioDecisionId {get;set;}
 public Usuario? UsuarioDecision {get;set;}
 [MaxLength(500)] public string? MotivoDecision {get;set;}
 public DateTime? FechaDecision {get;set;}
 [MaxLength(350)] public string Proposito {get;set;}="";
 [MaxLength(160)] public string? InstitucionRepresentada {get;set;}
 public List<DetalleSolicitud> Detalles {get;set;}=new();
 public Prestamo? Prestamo {get;set;}
}
public class DetalleSolicitud {
 public int Id {get;set;}
 public int SolicitudPrestamoId {get;set;}
 public SolicitudPrestamo Solicitud {get;set;}=null!;
 public int CategoriaEquipoId {get;set;}
 public CategoriaEquipo Categoria {get;set;}=null!;
 public int CantidadSolicitada {get;set;}
 public int? CantidadAprobada {get;set;}
 public int CantidadReservada {get;set;}
 public int? CantidadPropuesta {get;set;}
}
public class Prestamo {
 public int Id {get;set;}
 public int SolicitudPrestamoId {get;set;}
 public SolicitudPrestamo Solicitud {get;set;}=null!;
 public int BeneficiarioId {get;set;}
 public Beneficiario Beneficiario {get;set;}=null!;
 [MaxLength(20)] public string TipoPrestamo {get;set;}="Individual";
 [MaxLength(160)] public string? InstitucionRepresentada {get;set;}
 public DateTime FechaEntrega {get;set;}
 public DateOnly FechaDevolucionPrevista {get;set;}
 public DateOnly? FechaProgramadaDevolucion {get;set;}
 public EstadoPrestamo Estado {get;set;}=EstadoPrestamo.Activo;
 public string? ObservacionesFaltantes {get;set;}
 public List<DetallePrestamo> Detalles {get;set;}=new();
 public Entrega? Entrega {get;set;}
 public Devolucion? Devolucion {get;set;}
 [NotMapped] public EstadoPrestamo EstadoActual => Estado==EstadoPrestamo.Activo && FechaDevolucionPrevista < DateOnly.FromDateTime(DateTime.Today) ? EstadoPrestamo.Vencido : Estado;
}
public class DetallePrestamo {
 public int Id {get;set;}
 public int PrestamoId {get;set;}
 public Prestamo Prestamo {get;set;}=null!;
 public int EquipoId {get;set;}
 public Equipo Equipo {get;set;}=null!;
}
public class Entrega {
 public int Id {get;set;}
 public int PrestamoId {get;set;}
 public Prestamo Prestamo {get;set;}=null!;
 public int ResponsableId {get;set;}
 public Usuario Responsable {get;set;}=null!;
 public int BeneficiarioReceptorId {get;set;}
 public Beneficiario BeneficiarioReceptor {get;set;}=null!;
 public DateTime FechaEntrega {get;set;}
 [MaxLength(100)] public string IdentidadVerificada {get;set;}="";
 public ActaEntrega? Acta {get;set;}
}
public class ActaEntrega {
 public int Id {get;set;}
 public int EntregaId {get;set;}
 public Entrega Entrega {get;set;}=null!;
 public DateTime FechaFirma {get;set;}
 [MaxLength(15)] public string EstadoFirma {get;set;}="Firmada";
 [MaxLength(500)] public string ReferenciaDocumento {get;set;}="";
 [MaxLength(200)] public string NombreDocumento {get;set;}="acta.pdf";
 [MaxLength(64)] public string HuellaSha256 {get;set;}="";
 public byte[] DocumentoFirmado {get;set;}=Array.Empty<byte>();
}
public class Devolucion {
 public int Id {get;set;}
 public int PrestamoId {get;set;}
 public Prestamo Prestamo {get;set;}=null!;
 public int ResponsableId {get;set;}
 public Usuario Responsable {get;set;}=null!;
 public DateTime FechaRealRecepcion {get;set;}
 [MaxLength(20)] public string Estado {get;set;}="Cerrada";
 public bool Tardia {get;set;}
 public List<DetalleDevolucion> Detalles {get;set;}=new();
}
public class DetalleDevolucion {
 public int Id {get;set;}
 public int DevolucionId {get;set;}
 public Devolucion Devolucion {get;set;}=null!;
 public int PrestamoId {get;set;}
 public int EquipoId {get;set;}
 public DetallePrestamo DetallePrestamo {get;set;}=null!;
 public Equipo Equipo {get;set;}=null!;
 [MaxLength(40)] public string Condicion {get;set;}="";
 [MaxLength(500)] public string? Novedades {get;set;}
 public bool InspeccionFavorable {get;set;}
 [MaxLength(30)] public string DestinoEquipo {get;set;}="";
}
public class EventoPrestamo {
 public int Id {get;set;}
 public int SolicitudPrestamoId {get;set;}
 public SolicitudPrestamo Solicitud {get;set;}=null!;
 public int UsuarioId {get;set;}
 public Usuario Usuario {get;set;}=null!;
 public DateTime Fecha {get;set;}
 [MaxLength(600)] public string Mensaje {get;set;}="";
}
