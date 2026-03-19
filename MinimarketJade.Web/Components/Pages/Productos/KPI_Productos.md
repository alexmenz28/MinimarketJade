# KPIs - Módulo Productos (implementación actual)

## Resumen
Este documento refleja el estado real implementado en el software para el módulo de **Reportes/KPI**.

KPIs incluidos:
- **Más vendidos**
- **Stock crítico**
- **Menos vendidos**

Condición global:
- Se consideran **solo productos activos** (`Producto.activo = 1`).
- Se ignoran ventas anuladas (`anulada = 0`).

---

## Implementación en la UI

Vista:
- `Proyecto/MinimarketJade.Web/Components/Pages/Reportes/Index.razor`
- `Proyecto/MinimarketJade.Web/Components/Pages/Productos/KpisProductos.razor`
- `Proyecto/MinimarketJade.Web/Components/Pages/Productos/KpisProductos.razor.js`

Tecnología de visualización:
- **Chart.js** para:
  - Más vendidos (gráfico de barras).
  - Stock crítico (gráfico donut: críticos vs no críticos).
- **Tabla** para:
  - Menos vendidos (Top 5 con datos adicionales).

---

## KPI 1 - Más vendidos

Objetivo:
- Mostrar los productos con mayor demanda.

Lógica aplicada:
- Suma de `cantidad` por producto.
- Filtra ventas no anuladas.
- Filtra productos activos.
- En la UI se muestra **Top 3**.

Consulta de referencia (SQL):
```sql
SELECT
    p.id_producto,
    p.nombre,
    SUM(dv.cantidad) AS total_vendido
FROM Producto p
JOIN DetalleVenta dv ON dv.id_producto = p.id_producto
JOIN Venta v ON v.id_venta = dv.id_venta
WHERE p.activo = 1
  AND v.anulada = 0
GROUP BY p.id_producto, p.nombre
ORDER BY total_vendido DESC;
```

---

## KPI 2 - Stock crítico

Objetivo:
- Identificar riesgo de quiebre de stock.

Lógica aplicada:
- Producto crítico si `stock_actual <= stock_minimo`.
- Solo productos activos.
- En la UI:
  - donut `Críticos vs No críticos`,
  - resumen textual de productos críticos.

Consulta de referencia (SQL):
```sql
SELECT
    p.id_producto,
    p.nombre,
    p.stock_actual,
    p.stock_minimo
FROM Producto p
WHERE p.activo = 1
  AND p.stock_actual <= p.stock_minimo
ORDER BY p.stock_actual ASC, p.nombre ASC;
```

---

## KPI 5 - Menos vendidos

Objetivo:
- Detectar productos de baja rotación.

Lógica aplicada:
- Suma de ventas por producto activo.
- Considera solo ventas no anuladas.
- En la UI se muestra **Top 5 menos vendidos con ventas > 0** (para evitar filas no informativas).
- Se agregan columnas:
  - unidades vendidas,
  - % participación sobre total vendido,
  - estado de stock (`Crítico` / `OK`).

Consulta de referencia (SQL):
```sql
SELECT
    p.id_producto,
    p.nombre,
    SUM(dv.cantidad) AS total_vendido
FROM Producto p
JOIN DetalleVenta dv ON dv.id_producto = p.id_producto
JOIN Venta v ON v.id_venta = dv.id_venta
WHERE p.activo = 1
  AND v.anulada = 0
GROUP BY p.id_producto, p.nombre
HAVING SUM(dv.cantidad) > 0
ORDER BY total_vendido ASC, p.nombre ASC;
```

---

## Seed de datos para pruebas

Script actual para poblar datos visibles en los 3 KPI:
- `Proyecto/Scripts/03_ResetSeedKpisProductos.sql`

Este script:
- limpia seeds previos de KPI,
- crea dataset controlado de productos/ventas,
- deja datos visibles para los tres KPI.

---

## Nota de compatibilidad de tablas

En el código EF Core se usa:
- `Db.Venta` (entidad `Ventum`)
- `Db.DetalleVenta` (entidad `DetalleVentum`)

En SQL físico de BD pueden verse como:
- `Venta` / `DetalleVenta`
o
- `Ventum` / `DetalleVentum`

Los scripts seed recientes contemplan ambas variantes.

