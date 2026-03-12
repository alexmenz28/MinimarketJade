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

    //Traemos los datos de otras tablas, para la vista general
    public async Task<List<Ventum>> GetAllAsync()
    {
        return await _db.Venta 
            .Include(v => v.IdVendedorNavigation) 
            .Include(v => v.IdClienteNavigation)  
            .OrderByDescending(v => v.FechaHora)  
            .ToListAsync();
    }
    //cargar los detalles de una venta en especifico
    public async Task<Ventum?> GetByIdAsync(int id)
    {
        return await _db.Venta
            .Include(v => v.IdVendedorNavigation)
            .Include(v => v.IdClienteNavigation)
            .Include(v => v.DetalleVenta) 
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