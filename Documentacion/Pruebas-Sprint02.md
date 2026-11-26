# Plan de verificación Sprint 02 (no ejecutado)

Todas las pruebas quedan **pendientes**, no se fabrican resultados ni capturas. Registrar fecha, responsable, precondiciones, datos, resultado real, evidencia y concepto conforme al formato del curso.

## Preparación

- Base ConectaInventoryDB_Sprint02 migrada y aplicación en Development.
- Con bodeguero registrar recepción de al menos cinco equipos. Con admin dar de alta en dos categorías y generar QR. Con soporte realizar diagnóstico Apto para dejarlos Disponibles. Conservar seriales y evidencia de stock antes de cada prueba.
- Usar beneficiario y beneficiario2 para solicitudes y pruebas de aislamiento/concurrencia.
- Para entregas: fechas de inicio hoy y fin posterior; imprimir plantilla, completar equipos/firmas de prueba y guardar PDF de máximo 5 MB. No usar firmas ni documentos reales.

| Caso | Procedimiento | Resultado esperado | Estado |
|---|---|---|---|
| P-0201 | Beneficiario solicita 1 equipo de dos categorías, fechas y propósito | Radicado único, estado Pendiente | Pendiente |
| P-0202 | Omitir propósito, fechas inválidas o cantidades cero/negativas | No se guarda | Pendiente |
| P-0203 | Consultar disponibilidad; aprobar reserva de otra solicitud superpuesta | Cantidades netas disminuyen, estado físico no cambia | Pendiente |
| P-0204 | Admin aprueba con disponibilidad suficiente | Reserva por categoría, sin préstamo ni entrega | Pendiente |
| P-0205 | Rechazar primero sin motivo, después con motivo | Primero bloqueado; después Rechazada sin entrega | Pendiente |
| P-0206 | Bodeguero entrega todos los códigos aprobados, identifica titular y adjunta PDF firmado | Préstamo Activo, solicitud Materializada, acta custodiada, equipos Prestados | Pendiente |
| P-0207 | Intentar entrega de solicitud Pendiente por URL/POST, sin firma o incompleta | Denegada, sin cambios | Pendiente |
| P-0208 | Programar devolución futura | Fecha programada guardada, contractual intacta | Pendiente |
| P-0209 | Devolver completo, un equipo Apto y uno Dañado con novedades | Devuelto, devolución Cerrada, Apto disponible y dañado EnMantenimiento | Pendiente |
| P-0210 | Consultar historial con beneficiario, estado, fecha y categoría; abrir detalle | Resultados autorizados y coherentes; no coincidencias informadas | Pendiente |
| P-0211 | Marcar un equipo Faltante y guardar observación | Préstamo abierto, observación, no se crea devolución, ningún equipo liberado | Pendiente |
| P-0212 | Remitir equipo dañado | EnMantenimiento, vínculo con detalle devolución; reporte de mantenimiento pendiente de Sprint 03 | Pendiente / dependencia HU-0301 |

## Pruebas complementarias

| Caso | Procedimiento | Resultado esperado |
|---|---|---|
| CP-S02-13 | Misma cuenta y periodo idéntico, categorías distintas | No se duplica pendiente |
| CP-S02-14 | Solicitud con cantidad mayor al stock | Radica pendiente con aviso |
| CP-S02-15 | Admin intenta aprobar insuficiente; luego propone ajuste | Aprobación bloqueada; PendienteAjuste con propuesta |
| CP-S02-16 | Beneficiario modifica propuesta y reenvía | Pendiente, reevaluación requerida |
| CP-S02-17 | Cancelar aprobada antes de entrega | Reservas 0, unidades físicas intactas |
| CP-S02-18 | Dos solicitudes de titulares distintos compiten por última unidad del mismo periodo, aprobar simultáneamente en dos sesiones | No se aprueban ambas; si hay conflicto, reintentar y verificar disponibilidad |
| CP-S02-19 | Entrega de categoría equivocada, códigos repetidos o QR inexistente | Sin préstamo ni cambio de inventario |
| CP-S02-20 | Intentar segunda entrega y segunda devolución | Operación bloqueada |
| CP-S02-21 | Intentar reubicación y baja de equipo prestado/vencido | Bloqueadas |
| CP-S02-22 | Solicitar fecha pasada para devolución | Bloqueada |
| CP-S02-23 | Préstamo contractual vencido no devuelto (fixture controlado o esperar fecha) | Vencido visible, equipos no disponibles |
| CP-S02-24 | Beneficiario2 intenta abrir/modificar/cancelar solicitud de beneficiario por ID | Denegado o no encontrado |
| CP-S02-25 | Beneficiario intenta URL de evaluación/entrega, soporte intenta historial | Acceso denegado |
| CP-S02-26 | Descargar PDF propio y probar ID ajeno | Propio disponible; ajeno denegado |
| CP-S02-27 | Falla de persistencia controlada en una base desechable durante entrega/devolución | Rollback completo; no confirmación de éxito |
| CP-S02-28 | Repetir pruebas del Sprint 01 con nueva estructura de préstamos | Funciones previas conservadas |
| CP-S02-29 | Consultar `VerificarIntegridad.sql` después de flujos | Cero filas de incidencias |

Las pruebas 18 y 27 requieren escenarios controlados en SQL Server, no se consideran ejecutadas por simple inspección del código. Para la 27 usar una base desechable o una prueba de integración con fallo inyectado; no interrumpir ni alterar datos reales. Una vez terminadas, registrar los resultados reales y actualizar el documento académico.
