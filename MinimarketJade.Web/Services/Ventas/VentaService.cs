using MinimarketJade.Web.Data;
using MinimarketJade.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MinimarketJade.Web.Services;

public class VentaService : IVentaService
{
    private readonly AppDbContext _db;

    public VentaService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Ventum>> GetAllAsync()
    {
        return await _db.Venta 
            .Include(v => v.IdVendedorNavigation) // Trae los datos del Usuario
            .Include(v => v.IdClienteNavigation)  // Trae los datos del Cliente
            .OrderByDescending(v => v.FechaHora)  // Las más recientes primero
            .ToListAsync();
    }

    public async Task<Ventum?> GetByIdAsync(int id)
    {
        return await _db.Venta
            .Include(v => v.IdVendedorNavigation)
            .Include(v => v.IdClienteNavigation)
            .Include(v => v.DetalleVenta) // Trae los productos de esa venta
            .FirstOrDefaultAsync(v => v.IdVenta == id);
    }

    public async Task<int> AddAsync(Ventum venta)
    {
        _db.Venta.Add(venta);
        await _db.SaveChangesAsync();
        return venta.IdVenta;
    }

    public async Task AnularAsync(int id)
    {
        var venta = await _db.Venta.FindAsync(id);
        if (venta != null)
        {
            venta.Anulada = true;
            _db.Venta.Update(venta);
            await _db.SaveChangesAsync();
        }
    }
}