# ConectaInventory - Sprint 01 (Modulo 01: Gestion de Inventario y Activos)

Aplicacion web ASP.NET Core MVC (net10.0) con EF Core y SQL Server LocalDB.
Proyecto de Analisis de Sistemas, Grupo 301, Universidad Distrital Francisco Jose de Caldas.

## Ejecutar

1. Requisitos: SDK de .NET 10 y SQL Server LocalDB (viene con Visual Studio).
2. `dotnet run` (o F5 en Visual Studio). Al arrancar en desarrollo se crea la base
   `ConectaInventoryDB`, se siembran categorias, ubicaciones y tres usuarios de prueba.

| Usuario     | Clave          | Rol                |
|-------------|----------------|--------------------|
| bodeguero   | Bodeguero123*  | Bodeguero          |
| admin       | Admin123*      | Administrador      |
| soporte     | Soporte123*    | Soporte Tecnico    |

Las claves son solo de desarrollo: no se siembran fuera del entorno Development.

## Historias de usuario

| HU      | Que hace                                   | Donde                                              | Rol            |
|---------|--------------------------------------------|----------------------------------------------------|----------------|
| HU-0101 | Recepcion de donacion y alta de equipo     | `InventarioController` (Recepcion, Alta)           | Bodeguero / Administrador |
| HU-0102 | Reubicacion con historial                  | `InventarioController.Reubicar`                    | Bodeguero      |
| HU-0103 | Generar, reimprimir y validar QR           | `IdentificacionController`                         | Administrador / Bodeguero |
| HU-0104 | Consultar stock y disponibilidad efectiva  | `InventarioController.Stock`                       | Todos          |
| HU-0105 | Diagnostico, solicitud y decision de baja  | `DiagnosticoBajaController`                        | Soporte / Administrador |

## Arquitectura

Vistas Razor -> Controladores (delgados) -> Servicios (reglas de negocio) -> `ConectaInventoryContext` (EF Core).

- `Services/InventarioService`, `IdentificacionService`, `BajasService`: validaciones, estados y persistencia.
- `Models/`: entidades del dominio (Equipo, RecepcionDonacion, CodigoQR, MovimientoInventario, DiagnosticoTecnico, SolicitudBaja, ActaBaja, ...).
- `Prestamo` es una entidad minima del Modulo 02: el Modulo 01 solo la lee para bloquear traslados, bajas y excluir equipos de la disponibilidad.

## Estados del equipo (DE-S01-01)

Registrado -> EnDiagnostico -> Disponible / EnMantenimiento / BajaDefinitiva. Prestado lo gestiona el Modulo 02.

## Pendiente (siguientes sprints)

Pruebas automatizadas (casos del documento, seccion 9.6), migraciones de EF (hoy `EnsureCreated`),
Modulos 02 a 05.
