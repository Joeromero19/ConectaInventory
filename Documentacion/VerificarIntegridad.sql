-- Solo lectura, después de aplicar Sprint02Prestamos. Las consultas de incidencias deben devolver 0 filas.
SELECT DB_NAME() AS BaseActual;
SELECT MigrationId,ProductVersion FROM dbo.__EFMigrationsHistory ORDER BY MigrationId;
-- Una solicitud materializada tiene un préstamo y una entrega con acta.
SELECT s.Id,s.Estado FROM dbo.SolicitudesPrestamo s
LEFT JOIN dbo.Prestamos p ON p.SolicitudPrestamoId=s.Id
LEFT JOIN dbo.Entregas e ON e.PrestamoId=p.Id
LEFT JOIN dbo.ActasEntrega a ON a.EntregaId=e.Id
WHERE (s.Estado='Materializada' AND (p.Id IS NULL OR e.Id IS NULL OR a.Id IS NULL)) OR (s.Estado<>'Materializada' AND p.Id IS NOT NULL);
-- Reservas solo en aprobadas; coincide con aprobadas.
SELECT d.Id FROM dbo.DetallesSolicitud d JOIN dbo.SolicitudesPrestamo s ON s.Id=d.SolicitudPrestamoId
WHERE (s.Estado='Aprobada' AND (d.CantidadAprobada IS NULL OR d.CantidadReservada<>d.CantidadAprobada OR d.CantidadReservada<=0)) OR (s.Estado<>'Aprobada' AND d.CantidadReservada<>0);
-- Un equipo no puede pertenecer a dos préstamos abiertos.
SELECT d.EquipoId,COUNT(*) AS PrestamosAbiertos FROM dbo.DetallesPrestamo d JOIN dbo.Prestamos p ON p.Id=d.PrestamoId WHERE p.Estado IN ('Activo','Vencido') GROUP BY d.EquipoId HAVING COUNT(*)>1;
-- Equipo de préstamo abierto debe estar Prestado (EstadoEquipo=4).
SELECT e.Id,e.Estado FROM dbo.Equipos e JOIN dbo.DetallesPrestamo d ON e.Id=d.EquipoId JOIN dbo.Prestamos p ON p.Id=d.PrestamoId WHERE p.Estado IN ('Activo','Vencido') AND e.Estado<>4;
-- Una devolución formal contiene todos los equipos de su préstamo.
SELECT v.Id FROM dbo.Devoluciones v WHERE (SELECT COUNT(*) FROM dbo.DetallesDevolucion d WHERE d.DevolucionId=v.Id)<>(SELECT COUNT(*) FROM dbo.DetallesPrestamo d WHERE d.PrestamoId=v.PrestamoId);
