using Microsoft.AspNetCore.Components;
using MinimarketJade.Web.Data.Entities;
using MinimarketJade.Web.Services.Compras;
using MinimarketJade.Web.Services.Auth;
using MinimarketJade.Web.Services.Reportes;
using ApexCharts;

namespace MinimarketJade.Web.Components.Pages.Reportes;

public partial class Index : ComponentBase
{
    [Inject] private ICompraService CompraService { get; set; } = default!;
    [Inject] private IReporteService ReporteService { get; set; } = default!;
    [Inject] private AuthService Auth { get; set; } = default!;

    private List<GastoProveedorDto>? GastosPorProveedor;
    private List<PrecioCategoriaMesDto>? PreciosCategoria;
    private List<PrecioPromedioProductoDto>? ProductosDetalle;
    private PrecioCategoriaMesDto? categoriaDetalle;
    private bool mostrarDetalle = false;
    private string? totalSeleccionado;
    private ApexChartOptions<GastoProveedorDto>? opcionesTorta;

    private List<IngresosDiarios> ingresosDiarios = new();
    private decimal metaDiaria = 2000;
    private decimal totalHoy;
    private decimal totalAyer;
    private decimal cumplimientoHoy;
    private decimal variacion;
    private List<VendedorProductividad> vendedores = new();
    private bool mostrarTodosVendedores = false;
    private ApexChartOptions<IngresosDiarios>? opcionesGrafica;
    private ClientesRecurrentesDto? clientesRecurrentes;
    private List<ClientesMesDto> clientesPorMes = new();

    protected override async Task OnInitializedAsync()
    {
        if (!Auth.IsAdministrador) return;

        opcionesGrafica = new ApexChartOptions<IngresosDiarios>
        {
            Chart = new Chart
            {
                Toolbar = new Toolbar { Show = false },
                Zoom = new Zoom { Enabled = false }
            },
            Stroke = new Stroke { Curve = Curve.Smooth, Width = new List<int> { 4 } },
            Annotations = new Annotations
            {
                Yaxis = new List<AnnotationsYAxis>
                {
                    new AnnotationsYAxis
                    {
                        Y = (double)metaDiaria,
                        BorderColor = "#FF0000",
                        BorderWidth = 3,
                        Label = new Label
                        {
                            Text = "Meta",
                            Style = new Style { Color = "#FF0000" }
                        }
                    }
                }
            }
        };

        ingresosDiarios = await ReporteService.GetIngresosDiariosAsync(7);
        totalHoy = ingresosDiarios.FirstOrDefault(i => i.Fecha.Date == DateTime.Today)?.Total ?? 0;
        totalAyer = ingresosDiarios.FirstOrDefault(i => i.Fecha.Date == DateTime.Today.AddDays(-1))?.Total ?? 0;
        cumplimientoHoy = metaDiaria > 0 ? (totalHoy / metaDiaria * 100) : 0;
        variacion = totalAyer > 0 ? ((totalHoy - totalAyer) / totalAyer * 100) : 0;

        vendedores = await ReporteService.GetProductividadVendedoresAsync();
        clientesRecurrentes = await ReporteService.GetClientesRecurrentesAsync(5);
        clientesPorMes = await ReporteService.GetClientesRecurrentesPorMesAsync(DateTime.Today.Year, 5);

        var hasta = DateTime.UtcNow;
        var desde = hasta.AddMonths(-3);
        GastosPorProveedor = await CompraService.ObtenerGastoPorProveedorAsync(desde, hasta);

        opcionesTorta = new ApexChartOptions<GastoProveedorDto>
        {
            Chart = new Chart { Toolbar = new Toolbar { Show = false } },
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        await InvokeAsync(StateHasChanged);
    }
}

public class PrecioCategoriaMesDto
{
    public string? NombreCategoria { get; set; }
    public decimal PrecioPromedio { get; set; }
    public decimal PrecioEstandar { get; set; }
}