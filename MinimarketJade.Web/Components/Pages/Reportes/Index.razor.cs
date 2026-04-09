using Microsoft.AspNetCore.Components;
using MinimarketJade.Web.Data.Entities;
using MinimarketJade.Web.Services.Compras;
using ApexCharts;

namespace MinimarketJade.Web.Components.Pages.Reportes;

public partial class Index : ComponentBase
{
    [Inject] private ICompraService CompraService { get; set; } = default!;

    private List<GastoProveedorDto>? GastosPorProveedor;
    private List<PrecioCategoriaMesDto>? PreciosCategoria;
    private List<PrecioPromedioProductoDto>? ProductosDetalle;
    private PrecioCategoriaMesDto? categoriaDetalle;
    private bool mostrarDetalle = false;
    private string? totalSeleccionado;
    private ApexChartOptions<GastoProveedorDto>? opcionesTorta;

    protected override async Task OnInitializedAsync()
    {
        var hasta = DateTime.UtcNow;
        var desde = hasta.AddMonths(-3);
        GastosPorProveedor = await CompraService.ObtenerGastoPorProveedorAsync(desde, hasta);

        opcionesTorta = new ApexChartOptions<GastoProveedorDto>
        {
            Chart = new Chart
            {
                Toolbar = new Toolbar { Show = false }
            },
            Legend = new Legend { Position = LegendPosition.Right },
            Colors = GastosPorProveedor?.Select(g => g.Porcentaje > 40 ? "#dc3545" : "#1D9E75").ToList(),
            Tooltip = new Tooltip
            {
                Y = new TooltipY
                {
                    Formatter = "function(val) { return val.toFixed(2) + '%'; }"
                }
            }
        };

        var mesActual = DateTime.UtcNow;
        var actual = await CompraService.ObtenerPrecioCompraPromedioPorProductoAsync(mesActual.Year, mesActual.Month);

        var categoriaActual = actual
            .GroupBy(x => x.CategoriaProducto)
            .Select(g => new
            {
                Nombre = g.Key,
                Promedio = g.Sum(x => x.TotalGastado) / (g.Sum(x => x.CantidadTotal) == 0 ? 1 : g.Sum(x => x.CantidadTotal)),
                Estandar = g.Sum(x => x.CostoEstandar * x.CantidadTotal) / (g.Sum(x => x.CantidadTotal) == 0 ? 1 : g.Sum(x => x.CantidadTotal))
            }).ToList();

        PreciosCategoria = categoriaActual.Select(a => new PrecioCategoriaMesDto
        {
            NombreCategoria = a.Nombre,
            PrecioPromedio = a.Promedio,
            PrecioEstandar = a.Estandar
        }).ToList();
    }

    private async Task AbrirDetalle(PrecioCategoriaMesDto categoria)
    {
        categoriaDetalle = categoria;
        mostrarDetalle = true;
        var mesActual = DateTime.UtcNow;
        var todos = await CompraService.ObtenerPrecioCompraPromedioPorProductoAsync(mesActual.Year, mesActual.Month);
        ProductosDetalle = todos.Where(p => p.CategoriaProducto == categoria.NombreCategoria).ToList();
    }

    private void MostrarTotal(SelectedData<GastoProveedorDto> data)
    {
        var proveedor = data.DataPoint.Items.FirstOrDefault();
        if (proveedor != null)
            totalSeleccionado = $"{proveedor.RazonSocial}: Bs. {proveedor.TotalGastado:N2}";
    }
}

public class PrecioCategoriaMesDto
{
    public string? NombreCategoria { get; set; }
    public decimal PrecioPromedio { get; set; }
    public decimal PrecioEstandar { get; set; }
}