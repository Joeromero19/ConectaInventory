# ConectaInventory – Sprint 02

Universidad Distrital Francisco José de Caldas · Análisis de Sistemas · Grupo 301.
ASP.NET Core MVC (.NET 10), EF Core 10, SQL Server LocalDB, Razor y Bootstrap.
Base: ZIP `ConectaInventory-feature-sprint01-corregido`, commit de archivo 92e9b3ab2d010bce91e6dddc59d6bcff626e61f9. Requisitos: `Parcial02 editable.docx`, sección Módulo 02.

**Estado:** código fuente integrado de los cinco flujos; revisión estática, no compilado ni ejecutado aquí. La migración Sprint01Inicial creada en el equipo del estudiante no estaba en el ZIP base. Debe conservarse; la nueva migración se generará con EF Core localmente. No se certifican las pruebas hasta ejecutarlas.

## Instalación en la rama existente (recomendada)

1. Detener la aplicación. Comprobar `git branch --show-current` (debe ser `feature/sprint02-prestamos`) y `git status`. Guardar una copia de los archivos y de la base del Sprint 02, especialmente la carpeta `Migrations/` con `20261126035106_Sprint01Inicial` y su snapshot. Guardar los cambios previos mediante un commit local si procede. No borrar la carpeta original.
2. Usar el ZIP de archivos modificados, extrayendo su contenido directamente en la carpeta que contiene `ConectaInventory.csproj` y aceptando reemplazar los archivos del mismo nombre. No borrar archivos que no estén en el ZIP. Alternativamente, copiar el contenido del ZIP completo a la carpeta del proyecto sin reemplazar ni eliminar `Migrations/` ni `.git/`.
3. Si ya se creó manualmente `Models/Beneficiario.cs`, comparar con `Models/Prestamos/Entidades.cs` y retirar únicamente la definición duplicada, previa copia de respaldo. Mantener cualquier migración de beneficiarios que ya se haya aplicado; EF calculará el cambio restante.
4. Abrir `ConectaInventory.csproj` como proyecto en Visual Studio. Restaurar y compilar (`dotnet restore`, `dotnet build` o Ctrl+Shift+B).
5. En el Explorador de objetos de SQL Server, abrir una consulta **en ConectaInventoryDB_Sprint02**, ejecutar el archivo `Documentacion/VerificarAntesDeMigrar.sql`. Confirmar que el historial contiene `Sprint01Inicial` y que la tabla preliminar `Prestamos` no contiene filas. Si contiene filas, detenerse: se necesita una migración de datos para preservar esos préstamos. No aplicar la nueva migración en `ConectaInventoryDB`.
6. En la Consola del Administrador de paquetes NuGet:

```powershell
Get-Migration -Context ConectaInventoryContext -Connection "Server=(localdb)\mssqllocaldb;Database=ConectaInventoryDB_Sprint02;Trusted_Connection=True;MultipleActiveResultSets=true"
Add-Migration Sprint02Prestamos -Context ConectaInventoryContext
```

7. Revisar la migración generada: se esperan columnas nuevas en `Prestamos`, retirar la relación preliminar `EquipoId` y `FechaInicio`, conversión de estado a texto, rowversion en Equipos y nuevas tablas del módulo. Es normal que EF avise de posible pérdida de datos al retirar columnas; solo proceder si `Prestamos` preliminar estaba vacía y se cuenta con respaldo. No debe eliminar tablas o datos del inventario. Generar SQL para revisión si se desea:

```powershell
Script-Migration Sprint01Inicial Sprint02Prestamos -Context ConectaInventoryContext
```

Si hubiera una migración intermedia de beneficiarios, usar el nombre de esa migración como origen del script.

8. Aplicar únicamente a la base nueva:

```powershell
Update-Database -Context ConectaInventoryContext -Connection "Server=(localdb)\mssqllocaldb;Database=ConectaInventoryDB_Sprint02;Trusted_Connection=True;MultipleActiveResultSets=true"
Get-Migration -Context ConectaInventoryContext -Connection "Server=(localdb)\mssqllocaldb;Database=ConectaInventoryDB_Sprint02;Trusted_Connection=True;MultipleActiveResultSets=true"
```

9. Ejecutar con F5 en Development. El inicializador crea los beneficiarios demo si no existen. Probar con `Documentacion/Pruebas-Sprint02.md`. Guardar la carpeta `Migrations/` completa en Git cuando las pruebas lo permitan. No subir bases de datos, archivos de usuarios, actas reales ni credenciales reales.

## Instalación desde cero en otra carpeta

El ZIP completo contiene el código y los recursos web, pero no la carpeta Migrations local. Para una instalación limpia, cambiar **ambos** appsettings a un nombre nuevo de BD (ej. `ConectaInventoryDB_Sprint02_Limpia`) que no exista. Abrir proyecto, compilar, ejecutar `Add-Migration Sprint02Inicial -Context ConectaInventoryContext` y `Update-Database` con la conexión explícita a esa BD limpia. No aplicar Sprint02Inicial a una base que ya tenga las tablas del Sprint 01. En la rama existente se debe seguir la sección anterior, no esta.

## Cuentas de prueba (solo Development)

| Login | Clave demo | Rol |
|---|---|---|
| bodeguero | Bodeguero123* | Bodeguero |
| admin | Admin123* | Administrador |
| soporte | Soporte123* | SoporteTecnico |
| beneficiario | Beneficiario123* | Beneficiario |
| beneficiario2 | Beneficiario234* | Beneficiario (concurrencia y aislamiento) |

El código no cambia claves de cuentas existentes. Las cuentas demo son para la base local académica, no producción.

## Arquitectura e historias

Razor → `PrestamosController` → `PrestamoService` → EF Core `ConectaInventoryContext`. Se conserva la arquitectura del Sprint 01; no se incorporan repositorios separados porque DbContext ya cumple el acceso a datos. Se conserva el estilo Id/UsuarioId de las tablas existentes.

| HU | Implementación |
|---|---|
| HU-0201 / CU2.1, CU2.2 | Solicitud de varias categorías, periodo y propósito, representación institucional, disponibilidad orientativa, radicación, modificación, cancelación y duplicidad por periodo idéntico. |
| HU-0202 / CU2.3 | Evaluación, propuesta de ajuste, rechazo motivado, aprobación con reserva de cantidades, liberación al cancelar. |
| HU-0203 / CU2.4 | Entrega completa a titular verificado, selección de equipos identificados con QR, acta imprimible y PDF firmado custodiado, creación de préstamo y actualización de inventario atómicas. |
| HU-0204 / CU2.5, CU2.6 | Programación independiente, recepción total, registro de faltantes sin cierre, inspección por equipo, daños a mantenimiento, atraso. |
| HU-0205 / CU2.7 | Historial derivado, filtros de beneficiario, estado, categoría y periodo, detalle, actas y eventos. Beneficiarios solo consultan sus propios registros. |

## Decisiones y límites

- Duplicidad: periodo exactamente igual (el parcial deja pendiente decidir si también se bloquean solapamientos). Reservas de disponibilidad sí consideran intervalos solapados, inclusivos.
- Disponibilidad conservadora: equipos físicamente disponibles hoy menos reservas aprobadas que se solapan. No anticipa devoluciones futuras sin inspección. Puede subestimar cantidades cuando distintas reservas se solapan con partes disjuntas del intervalo. En entrega se vuelve a verificar. El equipo no cambia a Prestado al aprobar.
- El QR debe existir en la entrega; el operador comprueba los códigos físicos e inspección. La disponibilidad orientativa cuenta el estado físico y puede incluir unidades aún sin QR.
- La firma se verifica por el bodeguero: se registra identidad/aceptación, referencia, PDF de hasta 5 MB y hash SHA-256. No es validación criptográfica de firma electrónica ni sistema de certificación. Se valida encabezado PDF, no contenido ni antivirus; no cargar actas reales en entorno demo.
- Los eventos persistidos visibles en el detalle constituyen los avisos internos, no se envían emails ni SMS. La tabla EventosPrestamo conserva eventos; el historial sigue derivándose de solicitudes/préstamos/entregas/devoluciones y no existe una tabla HistorialPrestamo.
- Vencido se calcula al consultar según la fecha contractual, aunque el estado persistido puede continuar Activo hasta una operación. La programación no altera la fecha contractual. La devolución completa deja el préstamo Devuelto y la devolución Cerrada; no se añade cierre administrativo posterior sin regla definida.
- La entrega requiere fecha actual dentro del periodo aprobado. El tipo de préstamo se determina por la institución indicada en la solicitud. El perfil del beneficiario es de tipo Individual por defecto; la representación se conserva por operación.
- Se agregaron `CantidadPropuesta`, representación en solicitud, datos de custodia del acta, identidad verificada, marca de atraso, eventos y rowversion del equipo para implementar reglas no materializadas en el diccionario propuesto. Los nombres de columnas siguen convenciones existentes del repositorio. El diccionario final de datos debe conciliarse con la migración generada.
- Integración Sprint 01: bloqueos de reubicación y baja con préstamo activo/vencido, stock físico separado de cantidades reservadas, rowversion de Equipo. No se corrigen aquí todos los pendientes históricos del Sprint 01 (capacidad de bodega, auditoría de lectura QR, etc.).
- P-0212 depende de HU-0301 del Sprint 03: este módulo deja el equipo dañado EnMantenimiento y conserva el vínculo devolución-equipo; la creación de un reporte de mantenimiento vinculado pertenece a ese sprint.
- Los procesos de negocio se ejecutan en transacciones serializables; se debe comprobar concurrencia, rollback, migración y regresión sobre SQL Server real. No se afirma que la compilación ni las pruebas hayan sido realizadas en este entorno.
