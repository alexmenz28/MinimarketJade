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
        var hoy = DateTime.Today;
        var inicioSemana = hoy.AddDays(-(((int)hoy.DayOfWeek + 6) % 7));

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

    public async Task<ClientesRecurrentesDto> GetClientesRecurrentesAsync(int metaMensual = 40)
    {
        var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        var ventas = await _db.Venta
            .AsNoTracking()
            .Where(v => !v.Anulada && v.FechaHora >= inicioMes && v.IdCliente != null)
            .ToListAsync();

        var recurrentes = ventas
            .GroupBy(v => v.IdCliente)
            .Count(g => g.Count() > 1);

        return new ClientesRecurrentesDto
        {
            ClientesRecurrentes = recurrentes,
            Meta = metaMensual
        };
    }

    public async Task<List<ClientesMesDto>> GetClientesRecurrentesPorMesAsync(int anio, int metaMensual = 40)
    {
        var desde = new DateTime(anio, 1, 1);
        var hasta = new DateTime(anio, 12, 31, 23, 59, 59);

        var ventas = await _db.Venta
            .AsNoTracking()
            .Where(v => !v.Anulada && v.FechaHora >= desde && v.FechaHora <= hasta && v.IdCliente != null)
            .ToListAsync();

        var meses = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };

        return Enumerable.Range(1, 12).Select(mes =>
        {
            var ventasMes = ventas.Where(v => v.FechaHora.Month == mes).ToList();
            var recurrentes = ventasMes
                .GroupBy(v => v.IdCliente)
                .Count(g => g.Count() > 1);

            return new ClientesMesDto
            {
                Mes = mes,
                NombreMes = meses[mes - 1],
                ClientesRecurrentes = recurrentes,
                Meta = metaMensual
            };
        }).ToList();
    }
}