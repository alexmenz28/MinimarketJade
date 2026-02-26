# Guía del módulo de Categorías (backend)

Este documento explica cómo está implementado el módulo de **categorías** en el backend y cómo usarlo desde el frontend (Blazor). Las categorías son **jerárquicas** (árbol: padre → hijos) y se implementan con una relación **recursiva** en la base de datos.

---

## 1. ¿Qué es la jerarquía de categorías?

- **Categoría raíz**: no tiene padre (`IdCategoriaPadre = null`). Ejemplo: "Alimentos", "Limpieza".
- **Subcategoría**: tiene un padre (`IdCategoriaPadre` apunta a otra categoría). Ejemplo: "Leche" es hija de "Lácteos", y "Lácteos" es hija de "Alimentos".

La tabla `Categoria` en SQL Server tiene: `id_categoria`, `nombre`, `id_categoria_padre`. La columna `id_categoria_padre` referencia a `id_categoria` de la misma tabla (relación recursiva).

---

## 2. Dónde está el código (backend)

| Ubicación | Descripción |
|-----------|-------------|
| `MinimarketJade.Web/Data/Entities/Categoria.cs` | Entidad EF Core (IdCategoria, Nombre, IdCategoriaPadre, navegación Padre/Hijos). |
| `MinimarketJade.Web/Data/AppDbContext.cs` | DbSet `Categorias` y mapeo a la tabla con nombres de columna `id_categoria`, `nombre`, `id_categoria_padre`. |
| `MinimarketJade.Web/Models/CategoriaDto.cs` | DTO plano para listas y formularios (IdCategoria, Nombre, IdCategoriaPadre). |
| `MinimarketJade.Web/Models/CategoriaTreeDto.cs` | DTO con lista `Hijos` para representar el árbol completo. |
| `MinimarketJade.Web/Services/Categoria/ICategoriaService.cs` | Interfaz del servicio (contrato que usa el frontend). |
| `MinimarketJade.Web/Services/Categoria/CategoriaService.cs` | Implementación: consultas y CRUD. |

Los comentarios en español en el código explican cada parte.

---

## 3. Servicio: ICategoriaService

El frontend debe inyectar `ICategoriaService` en los componentes que necesiten categorías. Métodos disponibles:

| Método | Uso típico |
|--------|------------|
| `GetAllAsync()` | Lista plana de todas las categorías (para un desplegable simple o tabla). |
| `GetRaicesAsync()` | Solo categorías raíz (sin padre), para el primer nivel de un selector. |
| `GetArbolAsync()` | Jerarquía completa: cada categoría con su lista `Hijos` (para menú o selector en cascada). |
| `GetByIdAsync(id)` | Obtener una categoría por id. |
| `CreateAsync(dto)` | Crear categoría (dto.IdCategoriaPadre = null para raíz). |
| `UpdateAsync(id, dto)` | Actualizar nombre o padre. |
| `DeleteAsync(id)` | Eliminar; devuelve `false` si la categoría tiene hijos (no se permite borrar para no dejar huérfanos). |

---

## 4. Cómo usar desde el frontend (Blazor)

### Inyectar el servicio

En el `.razor` (o en un `@code`):

```csharp
@inject ICategoriaService CategoriaService
```

### Ejemplo: listar todas las categorías (lista plana)

```csharp
var categorias = await CategoriaService.GetAllAsync();
// categorias es IReadOnlyList<CategoriaDto> con IdCategoria, Nombre, IdCategoriaPadre
```

### Ejemplo: solo categorías raíz (primer nivel)

```csharp
var raices = await CategoriaService.GetRaicesAsync();
```

### Ejemplo: árbol completo (para menú o cascada)

```csharp
var arbol = await CategoriaService.GetArbolAsync();
// Cada elemento tiene .Hijos (lista de CategoriaTreeDto), que a su vez pueden tener .Hijos
```

### Ejemplo: crear una categoría raíz

```csharp
var dto = new CategoriaDto { Nombre = "Abarrotes", IdCategoriaPadre = null };
var creada = await CategoriaService.CreateAsync(dto);
```

### Ejemplo: crear una subcategoría

```csharp
var dto = new CategoriaDto { Nombre = "Arroz", IdCategoriaPadre = 2 }; // 2 = id del padre
var creada = await CategoriaService.CreateAsync(dto);
```

### Ejemplo: actualizar y eliminar

```csharp
await CategoriaService.UpdateAsync(id, dto);
var ok = await CategoriaService.DeleteAsync(id); // false si tiene hijos o no existe
```

---

## 5. Base de datos

La tabla `Categoria` se crea con el script `Scripts/01_CreateTables.sql`. Los datos iniciales (ej. categorías de ejemplo) pueden ir en `Scripts/02_SeedData.sql`. Asegúrense de haber ejecutado esos scripts sobre la base `MinimarketJade` antes de usar el servicio.

---

## 6. Resumen rápido

- **Jerárquica** = categorías en niveles (padre/hijos).
- **Recursiva** = la misma tabla se referencia con `id_categoria_padre`.
- El frontend usa **ICategoriaService** inyectado; los DTOs son **CategoriaDto** (plano) y **CategoriaTreeDto** (con **Hijos** para el árbol).
- Los comentarios en el código están en español para que todo el equipo pueda seguir la lógica.

Si dudas sobre algún método o DTO, revisa los comentarios en `Services/Categoria/ICategoriaService.cs` y `Services/Categoria/CategoriaService.cs`.
