# Pruebas Sprint 03 (pendientes de ejecución)
Base exclusiva ConectaInventoryDB_Sprint03_Pruebas. Registrar resultado real, fecha, captura y observaciones para cada caso.

| ID | Procedimiento | Esperado |
|---|---|---|
| P-0301 | Crear equipo mediante recepción/alta/diagnóstico; entrar como soporte y reportar serial y falla | Reporte pendiente; equipo EnMantenimiento |
| P-0302 | Repetir reporte para el mismo equipo | Rechazo sin duplicado |
| P-0303 | Completar préstamo y devolución dañada según Pruebas-Sprint02.md; entrar como soporte | Remisión visible; registrar reporte y verificar vínculo al detalle |
| P-0304 | Entrar como admin, asignar técnico activo | Estado Asignado e historial |
| P-0305 | Crear segundo técnico desde Técnicos; marcar no disponible; intentar asignarlo | No aparece en selección; servidor rechaza asignación inválida |
| P-0306 | Entrar como técnico asignado y actualizar a EnRevision | Estado e historial actualizados |
| P-0307 | Registrar actividad, repuesto cantidad 2 costo 10000 | Total 20000, actividad persistida |
| P-0308 | Marcar Reparado | Reporte cerrado, equipo Disponible |
| P-0309 | Repetir con otro equipo y marcar Irreparable | EnDiagnostico, no baja automática; diagnóstico NoApto |
| P-0310 | Solicitar baja desde soporte y evaluar como admin | Solo admin decide baja definitiva |
| P-0311 | Entrar como otro técnico, bodeguero y beneficiario; intentar actualizar reporte ajeno | Técnico ajeno no puede modificar; otros roles sin acceso al módulo |
| P-0312 | Insertar alerta ficticia con SQL de abajo; consultar todas y vencidas | Alerta visible según fecha |

SQL exclusivamente en la base de pruebas, tras crear al menos un equipo no dado de baja:
```sql
USE ConectaInventoryDB_Sprint03_Pruebas;
INSERT INTO AlertasPreventivas (EquipoId, FechaProgramada, Atendida)
SELECT TOP (1) Id, DATEADD(day,-1,GETDATE()), 0 FROM Equipos WHERE Estado <> 6 ORDER BY Id;
```
Verificar nombres de tabla con migración generada antes de ejecutar. No es regla de generación automática. Repetir pruebas del Sprint 02 y verificar que no se alteró la BD original. Probar concurrencia de reportes desde dos sesiones y confirmar un único abierto.
