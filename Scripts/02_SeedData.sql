-- ============================================================
-- Minimarket Jade - Datos iniciales (opcional)
-- Ejecutar después de 01_CreateTables.sql
-- ============================================================

-- Categorías de ejemplo
IF NOT EXISTS (SELECT 1 FROM dbo.Categoria WHERE nombre = N'General')
    INSERT INTO dbo.Categoria (nombre, id_categoria_padre) VALUES (N'General', NULL);

IF NOT EXISTS (SELECT 1 FROM dbo.Categoria WHERE nombre = N'Abarrotes')
    INSERT INTO dbo.Categoria (nombre, id_categoria_padre) VALUES (N'Abarrotes', NULL);

IF NOT EXISTS (SELECT 1 FROM dbo.Categoria WHERE nombre = N'Lácteos')
    INSERT INTO dbo.Categoria (nombre, id_categoria_padre) VALUES (N'Lácteos', NULL);

IF NOT EXISTS (SELECT 1 FROM dbo.Categoria WHERE nombre = N'Bebidas')
    INSERT INTO dbo.Categoria (nombre, id_categoria_padre) VALUES (N'Bebidas', NULL);

-- Usuario administrador inicial (contraseña debe cambiarse desde la aplicación; hash de ejemplo)
-- Ejemplo: hash para 'admin123' (usar ASP.NET Core Identity o mismo algoritmo que la app)
IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE nombre_usuario = N'admin')
    INSERT INTO dbo.Usuario (nombre_usuario, contraseña_hash, rol, activo)
    VALUES (N'admin', N'CAMBIAR_POR_HASH_REAL', N'Administrador', 1);

GO
