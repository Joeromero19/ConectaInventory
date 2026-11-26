namespace ConectaInventory.Models;

// Roles del sistema (documento, seccion 9.2.1)
public enum RolUsuario { Bodeguero, Administrador, SoporteTecnico, Beneficiario }

// Nombres de rol como constantes para usarlos en [Authorize(Roles = ...)]
public static class RolNombre
{
    public const string Bodeguero = nameof(RolUsuario.Bodeguero);
    public const string Administrador = nameof(RolUsuario.Administrador);
    public const string SoporteTecnico = nameof(RolUsuario.SoporteTecnico);
    public const string Beneficiario = nameof(RolUsuario.Beneficiario);
}

// Ciclo de vida del equipo (diagrama DE-S01-01)
// Recibido solo existe antes del alta; el equipo se persiste desde Registrado.
public enum EstadoEquipo
{
    Recibido,
    Registrado,
    EnDiagnostico,
    Disponible,
    Prestado,          // lo gestiona el Modulo 02 (prestamos)
    EnMantenimiento,
    BajaDefinitiva
}

// Estados de la recepcion (DE-S01-02). Se crea EnRecepcion (HU-0101)
// y pasa a Registrada cuando todos sus equipos tienen alta.
public enum EstadoRecepcion { EnRecepcion, Registrada, Rechazada, Cancelada }

// Resultado del diagnostico tecnico (DE-S01-03)
public enum ResultadoDiagnostico { Apto, Reparable, NoApto }

// Solicitud de baja (DE-S01-04). La aprobacion y la ejecucion ocurren en una sola transaccion.
public enum EstadoSolicitudBaja { Solicitada, Rechazada, Ejecutada }

// Prestamos: pertenecen al Modulo 02, aqui solo se consultan para validar reglas de integracion
public enum EstadoPrestamo { Reservado, Activo, Devuelto, Vencido, Cerrado }
