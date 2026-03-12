using MinimarketJade.Web.Data.Entities;

namespace MinimarketJade.Web.Services;

public interface IMovInventarioService
{
    Task RegistrarAsync(int idProducto, string tipoMovimiento, int cantidad, int idUsuario, string? motivo = null);
    Task<List<MovInventario>> GetByProductoAsync(int idProducto);
    Task<List<MovInventario>> GetAllAsync();
}