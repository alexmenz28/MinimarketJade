using MinimarketJade.Web.Data.Entities;
using MinimarketJade.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace MinimarketJade.Web.Services;

public class NotaVentaService : INotaVentaService
{
    private readonly AppDbContext _context;

    public NotaVentaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task CrearAsync(NotaVentum nota)
    {
        _context.NotaVenta.Add(nota);
        await _context.SaveChangesAsync();
    }

    public async Task<NotaVentum?> GetByVentaAsync(int idVenta)
    {
        return await _context.NotaVenta
            .FirstOrDefaultAsync(n => n.IdVenta == idVenta);
    }

    public async Task<List<NotaVentum>> GetAllAsync()
    {
        return await _context.NotaVenta.ToListAsync();
    }
}