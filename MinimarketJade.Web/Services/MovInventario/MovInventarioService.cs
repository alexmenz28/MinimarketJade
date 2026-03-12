using MinimarketJade.Web.Data;
using MinimarketJade.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MinimarketJade.Web.Services;

public class MovInventarioService : IMovInventarioService
{
    private readonly AppDbContext _context;

    public MovInventarioService(AppDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarAsync(int idProducto, string tipoMovimiento, int cantidad, int idUsuario, string? motivo = null)
    {
        var mov = new MovInventario
        {
            IdProducto = idProducto,
            TipoMovimiento = tipoMovimiento,
            Cantidad = cantidad,
            FechaHora = DateTime.Now,
            IdUsuario = idUsuario,
            Motivo = motivo
        };
        _context.MovInventarios.Add(mov);
        await _context.SaveChangesAsync();
    }

    public async Task<List<MovInventario>> GetByProductoAsync(int idProducto)
    {
        return await _context.MovInventarios
            .Include(m => m.IdUsuarioNavigation)
            .Include(m => m.IdProductoNavigation)
            .Where(m => m.IdProducto == idProducto)
            .OrderByDescending(m => m.FechaHora)
            .ToListAsync();
    }

    public async Task<List<MovInventario>> GetAllAsync()
    {
        return await _context.MovInventarios
            .Include(m => m.IdUsuarioNavigation)
            .Include(m => m.IdProductoNavigation)
            .OrderByDescending(m => m.FechaHora)
            .ToListAsync();
    }
}