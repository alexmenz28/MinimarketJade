using MinimarketJade.Web.Data;

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
        return Task.FromResult(0);
    }
}
