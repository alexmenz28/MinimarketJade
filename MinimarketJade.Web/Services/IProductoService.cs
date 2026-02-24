using MinimarketJade.Web.Data.Entities;

namespace MinimarketJade.Web.Services;

/// <summary>
/// Servicio de aplicación para el módulo de productos/catálogo.
/// </summary>
public interface IProductoService
{
    // Métodos a implementar según RF: listar, buscar por nombre/código/categoría, etc.
    Task<int> CountAsync(CancellationToken ct = default);

    //Listar
    Task<List<Producto>> GetAllAsync(CancellationToken ct = default);
    //Crear
    Task AddAsync(Producto producto);    
    //Editar
    Task UpdateAsync(Producto producto);
    //Evitar duplicados al crear o editar un producto
    Task<bool> ExisteNombreAsync(string nombre, int idActual = 0);
    //Inhabilitar productos
    Task InhabilitarAsync(int id);


}
