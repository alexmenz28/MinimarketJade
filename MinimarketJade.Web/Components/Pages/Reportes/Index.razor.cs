using Microsoft.AspNetCore.Components;
using MinimarketJade.Web.Services.Auth;
using MinimarketJade.Web.Services.Reportes;
using ApexCharts;


namespace MinimarketJade.Web.Components.Pages.Reportes;

public partial class Index : ComponentBase
{
    [Inject] private IReporteService ReporteService { get; set; } = default!;
    [Inject] private AuthService Auth { get; set; } = default!;

    private List<IngresosDiarios> ingresosDiarios = new();
    private decimal metaDiaria = 2000;
    private decimal totalHoy;
    private decimal totalAyer;
    private decimal cumplimientoHoy;
    private decimal variacion;

    private List<VendedorProductividad> vendedores = new();
    private bool mostrarTodosVendedores = false;

    private ApexChartOptions<IngresosDiarios>? opcionesGrafica;

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

        totalHoy = ingresosDiarios
            .FirstOrDefault(i => i.Fecha.Date == DateTime.Today)?.Total ?? 0;

        totalAyer = ingresosDiarios
            .FirstOrDefault(i => i.Fecha.Date == DateTime.Today.AddDays(-1))?.Total ?? 0;

        cumplimientoHoy = metaDiaria > 0 ? (totalHoy / metaDiaria * 100) : 0;

        variacion = totalAyer > 0 ? ((totalHoy - totalAyer) / totalAyer * 100) : 0;
        vendedores = await ReporteService.GetProductividadVendedoresAsync(); 

    }
}