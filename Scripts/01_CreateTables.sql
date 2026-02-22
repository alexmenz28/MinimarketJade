-- ============================================================
-- Minimarket Jade - Script de creación de tablas (SQL Server)
-- Basado en Analisis_Entregable3_y_DiagramaER.md
-- Ejecutar sobre la base de datos deseada (ej: MinimarketJade)
-- ============================================================

-- Usar la base de datos (crear primero si no existe: CREATE DATABASE MinimarketJade;)
-- USE MinimarketJade;
-- GO

-- ------------------------------------------------------------
-- Tablas independientes (sin FK o solo auto-referencia)
-- ------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categoria')
BEGIN
    CREATE TABLE dbo.Categoria (
        id_categoria    INT             NOT NULL IDENTITY(1,1),
        nombre          NVARCHAR(100)   NOT NULL,
        id_categoria_padre INT          NULL,
        CONSTRAINT PK_Categoria PRIMARY KEY (id_categoria),
        CONSTRAINT FK_Categoria_Padre   FOREIGN KEY (id_categoria_padre) REFERENCES dbo.Categoria (id_categoria)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Usuario')
BEGIN
    CREATE TABLE dbo.Usuario (
        id_usuario      INT             NOT NULL IDENTITY(1,1),
        nombre_usuario  NVARCHAR(50)    NOT NULL,
        contraseña_hash NVARCHAR(256)   NOT NULL,
        rol             NVARCHAR(20)    NOT NULL,  -- 'Administrador' | 'Vendedor'
        activo          BIT             NOT NULL DEFAULT 1,
        CONSTRAINT PK_Usuario PRIMARY KEY (id_usuario),
        CONSTRAINT UQ_Usuario_nombre_usuario UNIQUE (nombre_usuario)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Cliente')
BEGIN
    CREATE TABLE dbo.Cliente (
        id_cliente          INT             NOT NULL IDENTITY(1,1),
        documento_identidad  NVARCHAR(30)    NOT NULL,
        nombre_completo      NVARCHAR(200)   NOT NULL,
        telefono             NVARCHAR(20)   NULL,
        email                NVARCHAR(100)  NULL,
        direccion            NVARCHAR(300)  NULL,
        CONSTRAINT PK_Cliente PRIMARY KEY (id_cliente),
        CONSTRAINT UQ_Cliente_documento UNIQUE (documento_identidad)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Proveedor')
BEGIN
    CREATE TABLE dbo.Proveedor (
        id_proveedor    INT             NOT NULL IDENTITY(1,1),
        razon_social    NVARCHAR(200)   NOT NULL,
        nit_ruc         NVARCHAR(30)    NOT NULL,
        telefono        NVARCHAR(20)    NULL,
        email           NVARCHAR(100)   NULL,
        direccion       NVARCHAR(300)   NULL,
        contacto        NVARCHAR(100)   NULL,
        CONSTRAINT PK_Proveedor PRIMARY KEY (id_proveedor),
        CONSTRAINT UQ_Proveedor_nit_ruc UNIQUE (nit_ruc)
    );
END
GO

-- ------------------------------------------------------------
-- Producto (depende de Categoria)
-- ------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Producto')
BEGIN
    CREATE TABLE dbo.Producto (
        id_producto     INT             NOT NULL IDENTITY(1,1),
        codigo_barras   NVARCHAR(50)    NULL,
        nombre          NVARCHAR(200)   NOT NULL,
        descripcion     NVARCHAR(MAX)   NULL,
        id_categoria    INT             NOT NULL,
        precio_compra   DECIMAL(18,2)   NOT NULL DEFAULT 0,
        precio_venta    DECIMAL(18,2)   NOT NULL DEFAULT 0,
        stock_actual    INT             NOT NULL DEFAULT 0,
        stock_minimo    INT             NOT NULL DEFAULT 0,
        unidad_medida   NVARCHAR(20)    NOT NULL DEFAULT 'UND',
        activo          BIT             NOT NULL DEFAULT 1,
        CONSTRAINT PK_Producto PRIMARY KEY (id_producto),
        CONSTRAINT FK_Producto_Categoria FOREIGN KEY (id_categoria) REFERENCES dbo.Categoria (id_categoria),
        CONSTRAINT UQ_Producto_codigo_barras UNIQUE (codigo_barras)
    );
END
GO

-- ------------------------------------------------------------
-- Venta y NotaVenta (dependen de Usuario, Cliente, Producto ya existen)
-- ------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Venta')
BEGIN
    CREATE TABLE dbo.Venta (
        id_venta        INT             NOT NULL IDENTITY(1,1),
        fecha_hora      DATETIME2(0)    NOT NULL DEFAULT SYSDATETIME(),
        subtotal       DECIMAL(18,2)   NOT NULL DEFAULT 0,
        impuestos      DECIMAL(18,2)   NOT NULL DEFAULT 0,
        descuentos     DECIMAL(18,2)   NOT NULL DEFAULT 0,
        total          DECIMAL(18,2)   NOT NULL DEFAULT 0,
        metodo_pago    NVARCHAR(30)    NOT NULL,
        monto_recibido DECIMAL(18,2)   NULL,
        cambio         DECIMAL(18,2)   NULL,
        id_vendedor    INT             NOT NULL,
        id_cliente     INT             NULL,
        anulada        BIT             NOT NULL DEFAULT 0,
        CONSTRAINT PK_Venta PRIMARY KEY (id_venta),
        CONSTRAINT FK_Venta_Usuario  FOREIGN KEY (id_vendedor) REFERENCES dbo.Usuario (id_usuario),
        CONSTRAINT FK_Venta_Cliente FOREIGN KEY (id_cliente)  REFERENCES dbo.Cliente (id_cliente)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NotaVenta')
BEGIN
    CREATE TABLE dbo.NotaVenta (
        id_nota         INT             NOT NULL IDENTITY(1,1),
        id_venta        INT             NOT NULL,
        numero_ticket   NVARCHAR(30)    NOT NULL,
        CONSTRAINT PK_NotaVenta PRIMARY KEY (id_nota),
        CONSTRAINT FK_NotaVenta_Venta FOREIGN KEY (id_venta) REFERENCES dbo.Venta (id_venta),
        CONSTRAINT UQ_NotaVenta_id_venta UNIQUE (id_venta)
    );
END
GO

-- ------------------------------------------------------------
-- Compra (depende de Proveedor, Usuario)
-- ------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Compra')
BEGIN
    CREATE TABLE dbo.Compra (
        id_compra      INT             NOT NULL IDENTITY(1,1),
        fecha          DATE            NOT NULL,
        numero_factura NVARCHAR(50)     NULL,
        subtotal       DECIMAL(18,2)   NOT NULL DEFAULT 0,
        total          DECIMAL(18,2)   NOT NULL DEFAULT 0,
        estado         NVARCHAR(20)    NOT NULL DEFAULT 'pendiente',  -- pendiente | recibida | anulada
        id_proveedor   INT             NOT NULL,
        id_usuario     INT             NOT NULL,
        CONSTRAINT PK_Compra PRIMARY KEY (id_compra),
        CONSTRAINT FK_Compra_Proveedor FOREIGN KEY (id_proveedor) REFERENCES dbo.Proveedor (id_proveedor),
        CONSTRAINT FK_Compra_Usuario   FOREIGN KEY (id_usuario)   REFERENCES dbo.Usuario (id_usuario)
    );
END
GO

-- ------------------------------------------------------------
-- DetalleVenta (depende de Venta, Producto)
-- ------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DetalleVenta')
BEGIN
    CREATE TABLE dbo.DetalleVenta (
        id_detalle      INT             NOT NULL IDENTITY(1,1),
        id_venta        INT             NOT NULL,
        id_producto     INT             NOT NULL,
        cantidad        INT             NOT NULL,
        precio_unitario DECIMAL(18,2)   NOT NULL,
        subtotal        DECIMAL(18,2)   NOT NULL,
        CONSTRAINT PK_DetalleVenta PRIMARY KEY (id_detalle),
        CONSTRAINT FK_DetalleVenta_Venta    FOREIGN KEY (id_venta)    REFERENCES dbo.Venta (id_venta),
        CONSTRAINT FK_DetalleVenta_Producto FOREIGN KEY (id_producto) REFERENCES dbo.Producto (id_producto)
    );
END
GO

-- ------------------------------------------------------------
-- DetalleCompra (depende de Compra, Producto)
-- ------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DetalleCompra')
BEGIN
    CREATE TABLE dbo.DetalleCompra (
        id_detalle      INT             NOT NULL IDENTITY(1,1),
        id_compra       INT             NOT NULL,
        id_producto     INT             NOT NULL,
        cantidad        INT             NOT NULL,
        precio_unitario DECIMAL(18,2)   NOT NULL,
        subtotal        DECIMAL(18,2)   NOT NULL,
        CONSTRAINT PK_DetalleCompra PRIMARY KEY (id_detalle),
        CONSTRAINT FK_DetalleCompra_Compra   FOREIGN KEY (id_compra)   REFERENCES dbo.Compra (id_compra),
        CONSTRAINT FK_DetalleCompra_Producto  FOREIGN KEY (id_producto) REFERENCES dbo.Producto (id_producto)
    );
END
GO

-- ------------------------------------------------------------
-- MovInventario (depende de Producto, Usuario)
-- Sin id_compra según diagrama aprobado.
-- tipo_movimiento: venta | compra | ingreso | ajuste | anulacion
-- ------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MovInventario')
BEGIN
    CREATE TABLE dbo.MovInventario (
        id_movimiento   INT             NOT NULL IDENTITY(1,1),
        id_producto     INT             NOT NULL,
        tipo_movimiento NVARCHAR(20)    NOT NULL,
        cantidad        INT             NOT NULL,
        fecha_hora      DATETIME2(0)    NOT NULL DEFAULT SYSDATETIME(),
        id_usuario      INT             NOT NULL,
        motivo          NVARCHAR(MAX)   NULL,
        CONSTRAINT PK_MovInventario PRIMARY KEY (id_movimiento),
        CONSTRAINT FK_MovInventario_Producto FOREIGN KEY (id_producto) REFERENCES dbo.Producto (id_producto),
        CONSTRAINT FK_MovInventario_Usuario  FOREIGN KEY (id_usuario)  REFERENCES dbo.Usuario (id_usuario)
    );
END
GO

-- ------------------------------------------------------------
-- ArqueoCaja (depende de Usuario)
-- ------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ArqueoCaja')
BEGIN
    CREATE TABLE dbo.ArqueoCaja (
        id_arqueo               INT             NOT NULL IDENTITY(1,1),
        fecha                   DATE            NOT NULL,
        total_ventas_registrado DECIMAL(18,2)   NOT NULL DEFAULT 0,
        total_efectivo_fisico   DECIMAL(18,2)   NOT NULL DEFAULT 0,
        diferencia              DECIMAL(18,2)   NOT NULL DEFAULT 0,
        id_usuario              INT             NOT NULL,
        CONSTRAINT PK_ArqueoCaja PRIMARY KEY (id_arqueo),
        CONSTRAINT FK_ArqueoCaja_Usuario FOREIGN KEY (id_usuario) REFERENCES dbo.Usuario (id_usuario)
    );
END
GO

-- ------------------------------------------------------------
-- Índices recomendados para consultas frecuentes
-- ------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Producto_id_categoria' AND object_id = OBJECT_ID('dbo.Producto'))
    CREATE NONCLUSTERED INDEX IX_Producto_id_categoria ON dbo.Producto (id_categoria);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Producto_codigo_barras' AND object_id = OBJECT_ID('dbo.Producto'))
    CREATE NONCLUSTERED INDEX IX_Producto_codigo_barras ON dbo.Producto (codigo_barras) WHERE codigo_barras IS NOT NULL;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Venta_fecha_hora' AND object_id = OBJECT_ID('dbo.Venta'))
    CREATE NONCLUSTERED INDEX IX_Venta_fecha_hora ON dbo.Venta (fecha_hora);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Venta_id_vendedor' AND object_id = OBJECT_ID('dbo.Venta'))
    CREATE NONCLUSTERED INDEX IX_Venta_id_vendedor ON dbo.Venta (id_vendedor);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DetalleVenta_id_venta' AND object_id = OBJECT_ID('dbo.DetalleVenta'))
    CREATE NONCLUSTERED INDEX IX_DetalleVenta_id_venta ON dbo.DetalleVenta (id_venta);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MovInventario_id_producto' AND object_id = OBJECT_ID('dbo.MovInventario'))
    CREATE NONCLUSTERED INDEX IX_MovInventario_id_producto ON dbo.MovInventario (id_producto);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MovInventario_fecha_hora' AND object_id = OBJECT_ID('dbo.MovInventario'))
    CREATE NONCLUSTERED INDEX IX_MovInventario_fecha_hora ON dbo.MovInventario (fecha_hora);

GO
