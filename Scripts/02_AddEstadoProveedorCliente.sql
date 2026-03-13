-- ============================================================
-- Minimarket Jade - Agregar columna estado (activo) a
-- Proveedor y Cliente (SQL Server)
-- Ejecutar sobre la base de datos ya creada (ej: MinimarketJade)
-- ============================================================

-- USE MinimarketJade;
-- GO

-- ------------------------------------------------------------
-- Proveedor: agregar columna activo si no existe
-- ------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Proveedor') AND name = 'activo'
)
BEGIN
    ALTER TABLE dbo.Proveedor
    ADD activo BIT NOT NULL DEFAULT 1;

    -- Opcional: actualizar filas existentes por si el DEFAULT no se aplicó
    UPDATE dbo.Proveedor SET activo = 1 WHERE activo IS NULL;
END
GO

-- ------------------------------------------------------------
-- Cliente: agregar columna activo si no existe
-- ------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Cliente') AND name = 'activo'
)
BEGIN
    ALTER TABLE dbo.Cliente
    ADD activo BIT NOT NULL DEFAULT 1;

    UPDATE dbo.Cliente SET activo = 1 WHERE activo IS NULL;
END
GO
