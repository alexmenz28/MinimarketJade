using MinimarketJade.Web.Data.Entities;

namespace MinimarketJade.Web.Services.Productos;

/// <summary>
/// Servicio de aplicación para el módulo de productos/catálogo.
/// </summary>
public interface IProductoService
{
    Task<int> CountAsync(CancellationToken ct = default);
    Task<List<Producto>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Producto producto);
    Task UpdateAsync(Producto producto);
    Task<bool> ExisteNombreAsync(string nombre, int idActual = 0);
    Task InhabilitarAsync(int id);
}
