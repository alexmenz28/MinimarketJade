-- ============================================================
-- Minimarket Jade - Reset + Seed visible para KPIs de Productos
-- Objetivo:
--   1) Limpiar datos seed KPI anteriores
--   2) Insertar dataset nuevo para que los 3 gráficos se aprecien
--      claramente (más vendidos, stock crítico, menos vendidos)
--
-- Ejecutar después de:
--   - 01_CreateTables.sql
--   - (opcional) 02_SeedData.sql
--
-- Soporta ambas variantes de tablas de ventas:
--   - Ventum / DetalleVentum
--   - Venta  / DetalleVenta
-- ============================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRAN;

-- ------------------------------------------------------------
-- 0) Usuario vendedor/admin para FK de ventas
-- ------------------------------------------------------------
DECLARE @vendedorId INT =
(
    SELECT TOP 1 id_usuario
    FROM dbo.Usuario
    WHERE activo = 1
      AND (rol = N'Vendedor' OR rol = N'Administrador')
    ORDER BY CASE WHEN rol = N'Vendedor' THEN 0 ELSE 1 END, id_usuario
);

IF @vendedorId IS NULL
BEGIN
    INSERT INTO dbo.Usuario (nombre_usuario, contraseña_hash, rol, activo)
    VALUES (N'admin_kpi', N'SEED_HASH', N'Administrador', 1);

    SET @vendedorId = SCOPE_IDENTITY();
END

-- ------------------------------------------------------------
-- 1) Categorías base (si faltan)
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Categoria WHERE nombre = N'General')
    INSERT INTO dbo.Categoria (nombre, id_categoria_padre) VALUES (N'General', NULL);
IF NOT EXISTS (SELECT 1 FROM dbo.Categoria WHERE nombre = N'Abarrotes')
    INSERT INTO dbo.Categoria (nombre, id_categoria_padre) VALUES (N'Abarrotes', NULL);
IF NOT EXISTS (SELECT 1 FROM dbo.Categoria WHERE nombre = N'Lácteos')
    INSERT INTO dbo.Categoria (nombre, id_categoria_padre) VALUES (N'Lácteos', NULL);
IF NOT EXISTS (SELECT 1 FROM dbo.Categoria WHERE nombre = N'Bebidas')
    INSERT INTO dbo.Categoria (nombre, id_categoria_padre) VALUES (N'Bebidas', NULL);

DECLARE @catGeneral   INT = (SELECT TOP 1 id_categoria FROM dbo.Categoria WHERE nombre = N'General');
DECLARE @catAbarrotes INT = (SELECT TOP 1 id_categoria FROM dbo.Categoria WHERE nombre = N'Abarrotes');
DECLARE @catLacteos   INT = (SELECT TOP 1 id_categoria FROM dbo.Categoria WHERE nombre = N'Lácteos');
DECLARE @catBebidas   INT = (SELECT TOP 1 id_categoria FROM dbo.Categoria WHERE nombre = N'Bebidas');

-- ------------------------------------------------------------
-- 2) Limpiar seed anterior de ventas KPI
-- ------------------------------------------------------------
DECLARE @prefix NVARCHAR(30) = N'KPI_SEED_%';

DECLARE @hasVentum BIT = CASE WHEN OBJECT_ID(N'dbo.Ventum') IS NOT NULL THEN 1 ELSE 0 END;
DECLARE @hasDetalleVentum BIT = CASE WHEN OBJECT_ID(N'dbo.DetalleVentum') IS NOT NULL THEN 1 ELSE 0 END;
DECLARE @hasVenta BIT = CASE WHEN OBJECT_ID(N'dbo.Venta') IS NOT NULL THEN 1 ELSE 0 END;
DECLARE @hasDetalleVenta BIT = CASE WHEN OBJECT_ID(N'dbo.DetalleVenta') IS NOT NULL THEN 1 ELSE 0 END;

IF @hasVentum = 1 AND @hasDetalleVentum = 1
BEGIN
    DELETE FROM dbo.DetalleVentum
    WHERE id_venta IN (SELECT id_venta FROM dbo.Ventum WHERE metodo_pago LIKE @prefix);

    DELETE FROM dbo.Ventum
    WHERE metodo_pago LIKE @prefix;
END
ELSE IF @hasVenta = 1 AND @hasDetalleVenta = 1
BEGIN
    DELETE FROM dbo.DetalleVenta
    WHERE id_venta IN (SELECT id_venta FROM dbo.Venta WHERE metodo_pago LIKE @prefix);

    DELETE FROM dbo.Venta
    WHERE metodo_pago LIKE @prefix;
END
ELSE
BEGIN
    THROW 50000, 'No se encontraron tablas de ventas (Ventum/DetalleVentum o Venta/DetalleVenta).', 1;
END

-- ------------------------------------------------------------
-- 3) Limpiar productos KPI anteriores (código KPI-PRD-xxx)
-- ------------------------------------------------------------
-- Importante: primero borramos detalles/compra relacionados para evitar FK.
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DetalleCompra')
BEGIN
    DELETE dc
    FROM dbo.DetalleCompra dc
    JOIN dbo.Producto p ON p.id_producto = dc.id_producto
    WHERE p.codigo_barras LIKE N'KPI-PRD-%';
END

IF @hasDetalleVentum = 1
BEGIN
    DELETE dv
    FROM dbo.DetalleVentum dv
    JOIN dbo.Producto p ON p.id_producto = dv.id_producto
    WHERE p.codigo_barras LIKE N'KPI-PRD-%';
END
ELSE IF @hasDetalleVenta = 1
BEGIN
    DELETE dv
    FROM dbo.DetalleVenta dv
    JOIN dbo.Producto p ON p.id_producto = dv.id_producto
    WHERE p.codigo_barras LIKE N'KPI-PRD-%';
END

DELETE FROM dbo.Producto
WHERE codigo_barras LIKE N'KPI-PRD-%';

-- ------------------------------------------------------------
-- 4) Insertar productos KPI (todos activos para que KPI5 tenga barras visibles)
--    Además dejamos varios en stock crítico para KPI2.
-- ------------------------------------------------------------
DECLARE @und NVARCHAR(20) = N'UND';

INSERT INTO dbo.Producto
(
    codigo_barras, nombre, descripcion, id_categoria,
    precio_compra, precio_venta, stock_actual, stock_minimo,
    unidad_medida, activo
)
VALUES
-- Top ventas esperado
(N'KPI-PRD-001', N'Azúcar 1kg',   N'Seed KPI visible', @catGeneral,   1.00, 2.00,  8, 4, @und, 1),
(N'KPI-PRD-002', N'Arroz 1kg',    N'Seed KPI visible', @catAbarrotes, 1.50, 2.50,  6, 3, @und, 1),
(N'KPI-PRD-003', N'Leche 1L',     N'Seed KPI visible', @catLacteos,   2.00, 3.00,  3, 5, @und, 1), -- crítico
(N'KPI-PRD-004', N'Pan 500g',     N'Seed KPI visible', @catGeneral,   0.80, 1.50,  5, 5, @und, 1), -- crítico
(N'KPI-PRD-005', N'Galletas',     N'Seed KPI visible', @catAbarrotes, 0.90, 1.80,  1, 3, @und, 1), -- crítico
(N'KPI-PRD-006', N'Refresco',     N'Seed KPI visible', @catBebidas,   1.10, 2.20,  9, 4, @und, 1),
(N'KPI-PRD-007', N'Chocolate',    N'Seed KPI visible', @catAbarrotes, 1.20, 2.40,  2, 4, @und, 1), -- crítico
(N'KPI-PRD-008', N'Cafe',         N'Seed KPI visible', @catAbarrotes, 1.30, 2.60,  2, 2, @und, 1), -- crítico
(N'KPI-PRD-011', N'Queso',        N'Seed KPI visible', @catLacteos,   2.50, 4.00, 12, 3, @und, 1),
(N'KPI-PRD-012', N'Yogurt',       N'Seed KPI visible', @catLacteos,   1.60, 2.80,  4, 4, @und, 1); -- crítico

-- ------------------------------------------------------------
-- 5) Insertar ventas seed y detalle (todas no anuladas + 1 anulada)
--    Diseñado para que KPI1 y KPI5 muestren barras claramente.
-- ------------------------------------------------------------
IF @hasVentum = 1 AND @hasDetalleVentum = 1
BEGIN
    INSERT INTO dbo.Ventum (metodo_pago, id_vendedor, id_cliente, anulada, total, subtotal, impuestos, descuentos, monto_recibido, cambio)
    VALUES
      (N'KPI_SEED_V001', @vendedorId, NULL, 0, 0, 0, 0, 0, NULL, NULL),
      (N'KPI_SEED_V002', @vendedorId, NULL, 0, 0, 0, 0, 0, NULL, NULL),
      (N'KPI_SEED_V003', @vendedorId, NULL, 0, 0, 0, 0, 0, NULL, NULL),
      (N'KPI_SEED_V004', @vendedorId, NULL, 0, 0, 0, 0, 0, NULL, NULL),
      (N'KPI_SEED_V005', @vendedorId, NULL, 0, 0, 0, 0, 0, NULL, NULL),
      (N'KPI_SEED_V006', @vendedorId, NULL, 0, 0, 0, 0, 0, NULL, NULL),
      (N'KPI_SEED_V007', @vendedorId, NULL, 0, 0, 0, 0, 0, NULL, NULL),
      (N'KPI_SEED_V008', @vendedorId, NULL, 0, 0, 0, 0, 0, NULL, NULL),
      (N'KPI_SEED_V009', @vendedorId, NULL, 0, 0, 0, 0, 0, NULL, NULL),
      (N'KPI_SEED_V010', @vendedorId, NULL, 0, 0, 0, 0, 0, NULL, NULL),
      (N'KPI_SEED_V011', @vendedorId, NULL, 0, 0, 0, 0, 0, NULL, NULL),
      (N'KPI_SEED_V012', @vendedorId, NULL, 1, 0, 0, 0, 0, NULL, NULL); -- anulada

    INSERT INTO dbo.DetalleVentum (id_venta, id_producto, cantidad, precio_unitario, subtotal)
    SELECT
        v.id_venta,
        p.id_producto,
        d.cantidad,
        p.precio_venta,
        d.cantidad * p.precio_venta
    FROM (VALUES
        -- fuerte para KPI1
        (N'KPI_SEED_V001', N'KPI-PRD-001', 8),
        (N'KPI_SEED_V002', N'KPI-PRD-001', 7),
        (N'KPI_SEED_V003', N'KPI-PRD-001', 6),
        -- medio
        (N'KPI_SEED_V004', N'KPI-PRD-002', 5),
        (N'KPI_SEED_V005', N'KPI-PRD-002', 4),
        (N'KPI_SEED_V006', N'KPI-PRD-006', 4),
        (N'KPI_SEED_V007', N'KPI-PRD-006', 3),
        -- bajo (pero > 0 para que KPI5 tenga barras)
        (N'KPI_SEED_V008', N'KPI-PRD-003', 2),
        (N'KPI_SEED_V009', N'KPI-PRD-004', 1),
        (N'KPI_SEED_V010', N'KPI-PRD-005', 1),
        (N'KPI_SEED_V011', N'KPI-PRD-007', 1),
        (N'KPI_SEED_V011', N'KPI-PRD-008', 1),
        (N'KPI_SEED_V011', N'KPI-PRD-011', 2),
        (N'KPI_SEED_V011', N'KPI-PRD-012', 1),
        -- anulada (debe ignorarse)
        (N'KPI_SEED_V012', N'KPI-PRD-001', 20),
        (N'KPI_SEED_V012', N'KPI-PRD-002', 20)
    ) AS d(metodo_pago, codigo_barras, cantidad)
    JOIN dbo.Ventum v ON v.metodo_pago = d.metodo_pago
    JOIN dbo.Producto p ON p.codigo_barras = d.codigo_barras;
END
ELSE
BEGIN
    INSERT INTO dbo.Venta (metodo_pago, id_vendedor, id_cliente, anulada, total, subtotal, impuestos, descuentos, monto_recibido, cambio)
    VALUES
      (N'KPI_SEED_V001', @vendedorId, NULL, 0, 0, 0, 0, 0, NULL, NULL),
      (N'KPI_SEED_V002', @vendedorId, NULL, 0, 0, 0, 0, 0, NULL, NULL),
      (N'KPI_SEED_V003', @vendedorId, NULL, 0, 0, 0, 0, 0, NULL, NULL),
      (N'KPI_SEED_V004', @vendedorId, NULL, 0, 0, 0, 0, 0, NULL, NULL),
      (N'KPI_SEED_V005', @vendedorId, NULL, 0, 0, 0, 0, 0, NULL, NULL),
      (N'KPI_SEED_V006', @vendedorId, NULL, 0, 0, 0, 0, 0, NULL, NULL),
      (N'KPI_SEED_V007', @vendedorId, NULL, 0, 0, 0, 0, 0, NULL, NULL),
      (N'KPI_SEED_V008', @vendedorId, NULL, 0, 0, 0, 0, 0, NULL, NULL),
      (N'KPI_SEED_V009', @vendedorId, NULL, 0, 0, 0, 0, 0, NULL, NULL),
      (N'KPI_SEED_V010', @vendedorId, NULL, 0, 0, 0, 0, 0, NULL, NULL),
      (N'KPI_SEED_V011', @vendedorId, NULL, 0, 0, 0, 0, 0, NULL, NULL),
      (N'KPI_SEED_V012', @vendedorId, NULL, 1, 0, 0, 0, 0, NULL, NULL); -- anulada

    INSERT INTO dbo.DetalleVenta (id_venta, id_producto, cantidad, precio_unitario, subtotal)
    SELECT
        v.id_venta,
        p.id_producto,
        d.cantidad,
        p.precio_venta,
        d.cantidad * p.precio_venta
    FROM (VALUES
        -- fuerte para KPI1
        (N'KPI_SEED_V001', N'KPI-PRD-001', 8),
        (N'KPI_SEED_V002', N'KPI-PRD-001', 7),
        (N'KPI_SEED_V003', N'KPI-PRD-001', 6),
        -- medio
        (N'KPI_SEED_V004', N'KPI-PRD-002', 5),
        (N'KPI_SEED_V005', N'KPI-PRD-002', 4),
        (N'KPI_SEED_V006', N'KPI-PRD-006', 4),
        (N'KPI_SEED_V007', N'KPI-PRD-006', 3),
        -- bajo (pero > 0 para que KPI5 tenga barras)
        (N'KPI_SEED_V008', N'KPI-PRD-003', 2),
        (N'KPI_SEED_V009', N'KPI-PRD-004', 1),
        (N'KPI_SEED_V010', N'KPI-PRD-005', 1),
        (N'KPI_SEED_V011', N'KPI-PRD-007', 1),
        (N'KPI_SEED_V011', N'KPI-PRD-008', 1),
        (N'KPI_SEED_V011', N'KPI-PRD-011', 2),
        (N'KPI_SEED_V011', N'KPI-PRD-012', 1),
        -- anulada (debe ignorarse)
        (N'KPI_SEED_V012', N'KPI-PRD-001', 20),
        (N'KPI_SEED_V012', N'KPI-PRD-002', 20)
    ) AS d(metodo_pago, codigo_barras, cantidad)
    JOIN dbo.Venta v ON v.metodo_pago = d.metodo_pago
    JOIN dbo.Producto p ON p.codigo_barras = d.codigo_barras;
END

COMMIT TRAN;

-- ============================================================
-- Fin: dataset KPI visible
-- ============================================================

