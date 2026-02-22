namespace MinimarketJade.Web.Services;

/// <summary>
/// Servicio de aplicación para el módulo de productos/catálogo.
/// </summary>
public interface IProductoService
{
    // Métodos a implementar según RF: listar, buscar por nombre/código/categoría, etc.
    Task<int> CountAsync(CancellationToken ct = default);
}
