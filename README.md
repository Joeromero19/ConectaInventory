# ConectaInventory – Sprint 03
Universidad Distrital Francisco José de Caldas · Análisis de Sistemas · Grupo 301.
ASP.NET Core MVC (.NET 10), EF Core 10, SQL Server LocalDB, Razor y Bootstrap.
Base: feature/sprint02-prestamos, ZIP commit 2a5901fa62716ebcdbf717502835d6d8026ad116.
Estado: integración candidata, revisión de código estática; sin compilación ni pruebas ejecutadas. Se conservan las dos migraciones anteriores y el README anterior en Documentacion/README-Sprint02.md.

## Instalación y migración
1. Extraer ZIP completo en una carpeta nueva. Abrir ConectaInventory.csproj en Visual Studio, restaurar NuGet y compilar Ctrl+Shift+B. No iniciar todavía.
2. Herramientas > Administrador de paquetes NuGet > Consola del Administrador de paquetes. Seleccionar ConectaInventory como proyecto predeterminado y de inicio.
3. Ejecutar `Add-Migration Sprint03Mantenimiento -Context ConectaInventoryContext`. Revisar que solo agregue las cinco tablas del módulo, índices y relaciones; detenerse si elimina tablas/columnas anteriores. No borrar Migrations.
4. Ejecutar `Update-Database -Context ConectaInventoryContext -Connection "Server=(localdb)\mssqllocaldb;Database=ConectaInventoryDB_Sprint03_Pruebas;Trusted_Connection=True;MultipleActiveResultSets=true"`.
5. F5. Ambos appsettings apuntan a la base de pruebas nueva. Si ya existe esa base, verificar su contenido antes de migrar o elegir otro nombre en ambos archivos y en el comando. No aplicar a la BD original.
6. Seguir Documentacion/Pruebas-Sprint03.md y Pruebas-Sprint02.md. Conservar la migración nueva y snapshot para Git.

## Cuentas demo (Development)
Iguales a Sprint 02 (ver README anterior): admin, soporte, bodeguero, beneficiario, beneficiario2. Los perfiles técnicos existentes se inicializan como General, activos y disponibles. El administrador puede registrar técnicos y editar perfiles; no se implementa el módulo general de usuarios.

## Arquitectura e historias
Razor → MantenimientoController → MantenimientoService → ConectaInventoryContext. La gestión básica de perfiles y consultas reside en el controlador.
- HU-0301: reporte manual, remisiones pendientes y vínculo DetalleDevolucionId, bloqueo de duplicados abiertos.
- HU-0302: asignación/reasignación administrativa, especialidad visible, técnico activo y disponible.
- HU-0303: Asignado → EnRevision → Reparado/Irreparable. Técnico responsable o administrador; historial. Reparado devuelve a Disponible; Irreparable deja EnDiagnostico y genera diagnóstico NoApto para el flujo de baja del Sprint 01.
- HU-0304: actividades, repuestos, cantidades, costos e historial.
- HU-0305: consulta de alertas persistidas, filtro vencidas. No hay generación automática porque no se definió periodicidad; usar datos de prueba indicados.

## Decisiones y límites
No se crea reporte automáticamente al devolver: soporte lo registra. No hay baja automática. Se bloquea diagnóstico y baja paralelos durante mantenimiento abierto. La selección de técnico es manual. La cuenta inactiva conserva historial; se bloquea acceso al módulo de mantenimiento, no al resto del sistema (M04 pendiente). El administrador puede intervenir y queda registrado como responsable; no hay autorización de urgencia independiente. No se implementan M04/M05 completos. No se certifica concurrencia, regresión, compilación ni migración hasta probar en SQL Server real. Revisar diccionario de datos con la migración definitiva. Las pruebas deben usar datos ficticios.

## GitHub
Partir de feature/sprint02-prestamos, crear feature/sprint03-mantenimiento, copiar el ZIP de cambios sobre la carpeta del csproj, conservar .git y migraciones previas. Copiar también los archivos de la migración nueva generados al probar (incluido snapshot). Revisar git status y git diff, compilar y probar antes de commit/push. No subir bin, obj, .vs, bases de datos ni credenciales reales.
