using MinimarketJade.Web.Models;

namespace MinimarketJade.Web.Services;

/// <summary>
/// Servicio de aplicación para gestionar categorías de productos.
/// Expone listas planas, solo raíces, árbol completo y CRUD para que el frontend las consuma.
/// </summary>
public interface ICategoriaService
{
    /// <summary>Obtiene todas las categorías en lista plana, ordenadas por nombre.</summary>
    Task<IReadOnlyList<CategoriaDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Obtiene solo las categorías raíz (sin padre), para desplegables de primer nivel.</summary>
    Task<IReadOnlyList<CategoriaDto>> GetRaicesAsync(CancellationToken ct = default);

    /// <summary>Obtiene la jerarquía completa en forma de árbol (cada categoría con su lista de Hijos).</summary>
    Task<IReadOnlyList<CategoriaTreeDto>> GetArbolAsync(CancellationToken ct = default);

    /// <summary>Obtiene una categoría por id; null si no existe.</summary>
    Task<CategoriaDto?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Crea una nueva categoría. IdCategoriaPadre null = raíz.</summary>
    Task<CategoriaDto> CreateAsync(CategoriaDto dto, CancellationToken ct = default);

    /// <summary>Actualiza una categoría existente por id. Devuelve false si no existe.</summary>
    Task<bool> UpdateAsync(int id, CategoriaDto dto, CancellationToken ct = default);

    /// <summary>Elimina una categoría por id. Devuelve false si no existe o tiene hijos (evitar huérfanos).</summary>
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
