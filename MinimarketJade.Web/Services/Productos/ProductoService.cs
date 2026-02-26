using MinimarketJade.Web.Data;
using MinimarketJade.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MinimarketJade.Web.Services.Productos;

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
        return _db.Productos.CountAsync(ct);
    }

    public async Task<List<Producto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Productos
            .Include(p => p.Categoria)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Producto producto)
    {
        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Producto producto)
    {
        var local = _db.Productos.Local.FirstOrDefault(p => p.IdProducto == producto.IdProducto);
        if (local != null)
            _db.Entry(local).State = EntityState.Detached;

        _db.Productos.Update(producto);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ExisteNombreAsync(string nombre, int idActual = 0)
    {
        return await _db.Productos.AnyAsync(p =>
            p.Nombre.ToLower() == nombre.ToLower() &&
            p.IdProducto != idActual);
    }

    public async Task InhabilitarAsync(int id)
    {
        var producto = await _db.Productos.FindAsync(id);
        if (producto != null)
        {
            producto.Activo = false;
            _db.Productos.Update(producto);
            await _db.SaveChangesAsync();
        }
    }
}
