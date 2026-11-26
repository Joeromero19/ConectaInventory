-- Solo lectura. Ejecutar antes de generar/aplicar la migración Sprint02Prestamos.
-- Verificar visualmente que el esquema preliminar contiene cero préstamos.
SELECT DB_NAME() AS BaseActual;
IF DB_NAME() <> N'ConectaInventoryDB_Sprint02'
    THROW 50001, 'Seleccione ConectaInventoryDB_Sprint02. No use la base original.', 1;
SELECT MigrationId, ProductVersion FROM dbo.__EFMigrationsHistory ORDER BY MigrationId;
IF COL_LENGTH('dbo.Prestamos','EquipoId') IS NOT NULL
BEGIN
    SELECT COUNT(*) AS PrestamosPreliminares FROM dbo.Prestamos;
    IF EXISTS(SELECT 1 FROM dbo.Prestamos)
        THROW 50002, 'Hay prestamos preliminares: respalde y prepare migracion de datos antes de continuar.', 1;
END;
SELECT COUNT(*) AS Equipos FROM dbo.Equipos;
SELECT COUNT(*) AS Usuarios FROM dbo.Usuarios;
SELECT COUNT(*) AS Recepciones FROM dbo.Recepciones;
