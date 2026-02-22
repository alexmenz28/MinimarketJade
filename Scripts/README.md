# Scripts de base de datos – Minimarket Jade

Scripts SQL Server para crear la base de datos del sistema, según el diagrama ER definido en `Analisis_Entregable3_y_DiagramaER.md`.

## Orden de ejecución

1. **00_CreateDatabase.sql** – Crea la base de datos `MinimarketJade` (opcional si ya existe la BD).
2. **01_CreateTables.sql** – Crea todas las tablas, claves primarias, foráneas e índices. Es idempotente (comprueba `IF NOT EXISTS`).
3. **02_SeedData.sql** – Inserta datos iniciales (categorías de ejemplo y usuario admin). Opcional.

## Tablas creadas

| Tabla           | Descripción                          |
|-----------------|--------------------------------------|
| Categoria       | Categorías de productos (jerárquico) |
| Usuario         | Usuarios del sistema (Admin/Vendedor) |
| Cliente         | Clientes para historial de compras   |
| Proveedor       | Proveedores                          |
| Producto        | Catálogo y stock                     |
| Venta           | Cabecera de venta                    |
| DetalleVenta    | Líneas de venta                      |
| NotaVenta       | Comprobante por venta (1:1)          |
| Compra          | Cabecera de compra a proveedor       |
| DetalleCompra   | Líneas de compra                     |
| MovInventario   | Movimientos de stock (venta/compra/ingreso/ajuste/anulacion) |
| ArqueoCaja      | Auditoría de caja                    |

## Notas

- **MovInventario** no incluye `id_compra` (según diagrama aprobado).
- La cadena de conexión del proyecto debe apuntar a la base `MinimarketJade` (o al nombre que uses).
- En `02_SeedData.sql` el usuario `admin` tiene un hash de contraseña placeholder; reemplazar por el generado por la aplicación al implementar login.
