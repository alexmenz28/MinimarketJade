using Microsoft.AspNetCore.Components;
using MinimarketJade.Web.Data.Entities;
using MinimarketJade.Web.Services;
using MinimarketJade.Web.Services.Auth;
using MinimarketJade.Web.Services.Productos;

namespace MinimarketJade.Web.Components.Pages.Inventario;

public partial class Index : ComponentBase
{
    [Inject] private IMovInventarioService MovInventarioService { get; set; } = default!;
    [Inject] private IProductoService ProductoService { get; set; } = default!;
    [Inject] private AuthService AuthService { get; set; } = default!;


    private List<MovInventario> movimientos = new();
    private List<Producto> productos = new();

    // Filtros
    private string busqueda = "";
    private string filtroTipo = "";
    private DateTime? fechaDesde;
    private DateTime? fechaHasta;

    // Ajuste
    private bool mostrarAjuste = false;
    private string? errorAjuste;
    private int idProductoAjuste = 0;
    private string operacion = "sumar"; 
    private int cantidadAjuste = 1;
    private string motivoAjuste = "";

    private IEnumerable<MovInventario> MovimientosFiltrados => movimientos
        .Where(m =>
            (string.IsNullOrWhiteSpace(busqueda) ||
             m.IdProductoNavigation.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(filtroTipo) || m.TipoMovimiento == filtroTipo) &&
            (fechaDesde == null || m.FechaHora.Date >= fechaDesde.Value.Date) &&
            (fechaHasta == null || m.FechaHora.Date <= fechaHasta.Value.Date));

    protected override async Task OnInitializedAsync() => await CargarDatos();

    private async Task CargarDatos()
    {
        movimientos = await MovInventarioService.GetAllAsync();
        productos = await ProductoService.GetAllAsync();
    }

    private void LimpiarFiltros()
    {
        busqueda = "";
        filtroTipo = "";
        fechaDesde = null;
        fechaHasta = null;
    }

    private void AbrirAjuste()
    {
        errorAjuste = null;
        idProductoAjuste = 0;
        operacion = "sumar";
        cantidadAjuste = 1;
        motivoAjuste = "";
        mostrarAjuste = true;
    }

    private async Task GuardarAjuste()
    {
        errorAjuste = null;

        if (idProductoAjuste == 0) { errorAjuste = "⚠ Selecciona un producto."; return; }
        if (cantidadAjuste <= 0) { errorAjuste = "⚠ La cantidad debe ser mayor a 0."; return; }
        if (string.IsNullOrWhiteSpace(motivoAjuste)) { errorAjuste = "⚠ El motivo es obligatorio."; return; }

        var producto = productos.FirstOrDefault(p => p.IdProducto == idProductoAjuste);
        if (producto == null) { errorAjuste = "⚠ Producto no encontrado."; return; }

        if (operacion == "restar" && producto.StockActual < cantidadAjuste)
        {
            errorAjuste = $"⚠ Stock insuficiente. Stock actual: {producto.StockActual}";
            return;
        }

        await ProductoService.ActualizarStockAsync(idProductoAjuste, operacion == "sumar" ? -cantidadAjuste : cantidadAjuste);


        await MovInventarioService.RegistrarAsync(
            idProducto: idProductoAjuste,
            tipoMovimiento: "Ajuste",
            cantidad: cantidadAjuste,
            idUsuario: AuthService.CurrentUser!.IdUsuario,
            motivo: $"{(operacion == "sumar" ? "Ajuste +" : "Ajuste -")}{cantidadAjuste} | {motivoAjuste}"
        );

        mostrarAjuste = false;
        await CargarDatos();
    }
}