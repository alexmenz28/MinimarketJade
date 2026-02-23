using MinimarketJade.Web.Data;
using MinimarketJade.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MinimarketJade.Web.Services;

/// <summary>
/// Implementación del servicio de productos.
/// </summary>
public class ProductoService : IProductoService
{
    private readonly AppDbContext _db;

    public ProductoService(AppDbContext db)
    {
        _db = db;
    }

    public Task<int> CountAsync(CancellationToken ct = default)
    {
        // Cuando exista DbSet<Producto>: return _db.Productos.CountAsync(ct);
        return _db.Productos.CountAsync(ct);
    }

    public async Task<List<Producto>> GetAllAsync(CancellationToken ct = default)
    {
        // Traer la lista de productos
        return await _db.Productos
        .Include(p => p.Categoria) //Llamar a categorias para que se reconozca en la tabla principal de productos/index.razor
        .ToListAsync(ct);
    }

    //Añadir nuevos productos (CREATE)
    public async Task AddAsync(Producto producto)
    {
        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();

    }
    //Actualizar productos existentes (UPDATE)
    public async Task UpdateAsync(Producto producto)
    {
        //Ubicamos el producto dentro de la tabla
        var local = _db.Productos.Local.FirstOrDefault(p => p.IdProducto == producto.IdProducto);

        if (local != null)
        {
            _db.Entry(local).State = EntityState.Detached;
        }

        // Actualizar el producto
        _db.Productos.Update(producto);
        await _db.SaveChangesAsync();
    }
    public async Task<bool> ExisteNombreAsync(string nombre)
    {
        // Busca si hay productos con el mismo nombre (ignorando mayúsculas/minúsculas), para evitar duplicados al registrar
        return await _db.Productos.AnyAsync(p => p.Nombre.ToLower() == nombre.ToLower());
    }
}
