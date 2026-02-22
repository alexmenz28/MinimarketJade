# Minimarket Jade – Sistema de gestión integral

Sistema de información para el **Minimarket "Jade"**: gestión de ventas, inventario, productos, clientes, compras a proveedor, reportes y KPI.

**Proyecto integrador – UTEPSA | Ingeniería de Sistemas**

---

## Stack técnico

| Capa        | Tecnología              |
|------------|--------------------------|
| Frontend   | Blazor Server (.NET)     |
| Backend    | ASP.NET Core (C#)        |
| Base de datos | SQL Server           |

---

## Estructura del repositorio

```
Proyecto/
├── MinimarketJade.Web/     # Aplicación web Blazor Server
│   ├── Components/         # Páginas y componentes (Layout, Pages por módulo)
│   ├── Data/               # AppDbContext, entidades (EF Core)
│   ├── Services/           # Lógica de aplicación (servicios inyectables)
│   ├── Models/             # DTOs y view models
│   └── wwwroot/            # Estáticos (CSS, etc.)
├── Scripts/                # Scripts SQL Server (creación de BD y tablas)
│   ├── 00_CreateDatabase.sql
│   ├── 01_CreateTables.sql
│   ├── 02_SeedData.sql
│   └── README.md           # Orden de ejecución y descripción de tablas
└── README.md               # Este archivo
```

---

## Requisitos

- [.NET 8 o superior](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) (o LocalDB para desarrollo)
- Editor o IDE (Visual Studio, VS Code, Rider)

---

## Ejecutar la aplicación

1. **Clonar el repositorio** (o abrir la carpeta `Proyecto`).

2. **Crear la base de datos** (si aún no existe):
   - Ejecutar en orden: `Scripts/00_CreateDatabase.sql`, `Scripts/01_CreateTables.sql`.
   - Opcional: `Scripts/02_SeedData.sql` para datos iniciales.
   - Ver detalles en [Scripts/README.md](Scripts/README.md).

3. **Configurar la conexión** en `MinimarketJade.Web/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=TU_SERVIDOR;Database=MinimarketJade;Trusted_Connection=True;..."
   }
   ```

4. **Restaurar y ejecutar**:
   ```bash
   cd MinimarketJade.Web
   dotnet restore
   dotnet run
   ```

5. Abrir en el navegador la URL que muestre la consola (ej. `http://localhost:5xxx`).

---

## Módulos

- **Ventas** – Punto de venta (POS), totales, método de pago, nota de venta.
- **Productos** – Catálogo, categorías, precios, código de barras.
- **Inventario** – Stock, movimientos, alertas de stock crítico.
- **Clientes** – Perfiles, historial de compras.
- **Compras** – Compras a proveedor, recepción de mercancía.
- **Reportes** – KPI, ventas, rentabilidad, stock crítico.

---

## Equipo

- Sebastian Isurza Rodriguez  
- Lino Andres Jimenez Vaca  
- Alexander Mendoza Cholima  
- Joan Alexander Yavi Lima  

---

## Licencia

Proyecto académico – UTEPSA.
