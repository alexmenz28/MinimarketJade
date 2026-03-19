using MinimarketJade.Web.Services.Reportes;
using Microsoft.EntityFrameworkCore;
using MinimarketJade.Web.Data;

namespace MinimarketJade.Web.Services.Reportes;

public class ReporteService : IReporteService
{
    private readonly AppDbContext _db;
    public ReporteService(AppDbContext db) => _db = db;

    public async Task<List<IngresosDiarios>> GetIngresosDiariosAsync(int dias = 7)
    {
        var desde = DateTime.Now.AddDays(-dias);
        var ventas = await _db.Venta
        .AsNoTracking()
        .Where(v => !v.Anulada && v.FechaHora >= desde && v.Total > 0)
        .ToListAsync();

        return ventas
            .GroupBy(v => v.FechaHora.Date)
            .Select(g => new IngresosDiarios
            {
                Fecha = g.Key,
                Total = g.Sum(v => v.Total)
            })
            .OrderBy(x => x.Fecha)
            .ToList();
    }


    public async Task<List<VendedorProductividad>> GetProductividadVendedoresAsync()
    {
        var inicioSemana = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);

        var ventas = await _db.Venta
            .AsNoTracking()
            .Where(v => !v.Anulada && v.FechaHora >= inicioSemana && v.Total > 0)
            .Include(v => v.IdVendedorNavigation)
            .ToListAsync();

        return ventas
            .GroupBy(v => v.IdVendedorNavigation.NombreUsuario)
            .Select(g => new VendedorProductividad
            {
                NombreVendedor = g.Key,
                TotalVentas = g.Sum(v => v.Total),
                Meta = 3000
            })
            .OrderByDescending(v => v.TotalVentas)
            .ToList();
    }

}