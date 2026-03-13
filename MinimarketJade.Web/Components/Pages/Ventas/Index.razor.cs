using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MinimarketJade.Web.Data.Entities;
using MinimarketJade.Web.Services;
using MinimarketJade.Web.Services.Auth;
using MinimarketJade.Web.Services.Clientes;
using MinimarketJade.Web.Services.Productos;

namespace MinimarketJade.Web.Components.Pages.Ventas;

public partial class Index : ComponentBase
{
    [Inject] private IVentaService VentaService { get; set; } = default!;
    [Inject] private IClienteService ClienteService { get; set; } = default!;
    [Inject] private IProductoService ProductoService { get; set; } = default!;
    [Inject] private INotaVentaService NotaVentaService { get; set; } = default!;
    [Inject] private IMovInventarioService MovInventarioService { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    private IJSObjectReference? _jsModule;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            _jsModule = await JS.InvokeAsync<IJSObjectReference>(
                "import", "./Components/Pages/Ventas/Index.razor.js");
    }
    [Inject] private AuthService AuthService { get; set; } = default!;

    private List<Ventum> ventas = new();
    private List<Cliente> clientes = new();
    private List<Producto> productos = new();
    private List<DetalleVentum> carrito = new();
    private List<NotaVentum> notas = new();

    private Ventum venta = new();
    private Ventum? ventaDetalle;
    private NotaVentum? notaDetalle;
    private Cliente? clienteSeleccionado;

    private int idProducto;
    private decimal montoRecibido;
    private string filtroCliente = "";
    private string busquedaVenta = "";

    private DateTime? fechaDesde;
    private DateTime? fechaHasta;

    private bool mostrarVenta = false;
    private bool mostrarDetalle = false;
    private bool mostrarClientes = false;
    private bool mostrarFormCliente = false;
    private string? alertaStock;
    private string? errorCliente;
    private string nuevoCI = "";
    private string nuevoNombre = "";

    private IEnumerable<Ventum> VentasFiltradas =>
        ventas.Where(v =>
            (fechaDesde == null || v.FechaHora.Date >= fechaDesde.Value.Date) &&
            (fechaHasta == null || v.FechaHora.Date <= fechaHasta.Value.Date) &&
            (string.IsNullOrWhiteSpace(busquedaVenta) ||
             (v.IdClienteNavigation?.NombreCompleto.Contains(busquedaVenta, StringComparison.OrdinalIgnoreCase) ?? false) ||
             notas.Any(n => n.IdVenta == v.IdVenta && n.NumeroTicket.Contains(busquedaVenta, StringComparison.OrdinalIgnoreCase))));

    private IEnumerable<Cliente> ClientesFiltrados =>
        clientes.Where(c => string.IsNullOrWhiteSpace(filtroCliente) ||
        c.NombreCompleto.Contains(filtroCliente, StringComparison.OrdinalIgnoreCase) ||
        c.DocumentoIdentidad.Contains(filtroCliente));



    protected override async Task OnInitializedAsync() => await CargarDatos();

    private async Task CargarDatos()
    {
        ventas = await VentaService.GetAllAsync();
        clientes = await ClienteService.ObtenerTodosAsync();
        productos = await ProductoService.GetAllAsync();
        notas = await NotaVentaService.GetAllAsync();
    }

    private void LimpiarFiltro()
    {
        fechaDesde = null;
        fechaHasta = null;
        busquedaVenta = "";
    }

    //Inicializar Venta
    private void AbrirModal()
    {
        venta = new Ventum { FechaHora = DateTime.Now, IdVendedor = AuthService.CurrentUser!.IdUsuario, MetodoPago = "Efectivo", Anulada = false };
        clienteSeleccionado = null;
        carrito.Clear();
        montoRecibido = 0;
        alertaStock = null;
        filtroCliente = "";
        mostrarVenta = true;
    }

    private void SeleccionarCliente(Cliente? cli)
    {
        clienteSeleccionado = cli;
        venta.IdCliente = cli?.IdCliente;
        mostrarClientes = false;
        mostrarFormCliente = false;
        filtroCliente = "";
    }

    private async Task RegistrarCliente()
    {
        errorCliente = null;
        var existe = clientes.Any(c => c.DocumentoIdentidad == nuevoCI.Trim());
        if (existe)
        {
            errorCliente = "Ya existe un cliente con ese CI.";
            return;
        }
        var nuevo = new Cliente
        {
            DocumentoIdentidad = nuevoCI.Trim(),
            NombreCompleto = nuevoNombre.Trim()
        };
        await ClienteService.CrearAsync(nuevo);
        await CargarDatos();

        var registrado = clientes.FirstOrDefault(c => c.DocumentoIdentidad == nuevo.DocumentoIdentidad);
        if (registrado != null) SeleccionarCliente(registrado);

        nuevoCI = "";
        nuevoNombre = "";
        mostrarFormCliente = false;
    }

    // Agregar producto al carrito
    private void Agregar()
    {
        alertaStock = null;
        if (idProducto == 0) return;
        // Busca el producto en memoria
        var prod = productos.FirstOrDefault(p => p.IdProducto == idProducto);
        if (prod == null) return;

        // Verifica si ya está en el carrito
        var item = carrito.FirstOrDefault(c => c.IdProducto == prod.IdProducto);
        if (prod.StockActual > (item?.Cantidad ?? 0))
        {
            if (item != null)
            {
                item.Cantidad++; item.Subtotal = item.Cantidad * item.PrecioUnitario;
            }
            else carrito.Add(new DetalleVentum
            {
                IdProducto = prod.IdProducto,
                IdProductoNavigation = prod,
                Cantidad = 1,
                PrecioUnitario = prod.PrecioVenta,
                Subtotal = prod.PrecioVenta
            });
            idProducto = 0;
        }
        else alertaStock = $"⚠ Sin stock disponible para \"{prod.Nombre}\".";
    }

    //botones + y - para modificar cantidad en el carrito
    private void ModificarCantidad(DetalleVentum item, int cambio)
    {
        alertaStock = null;
        var prod = productos.FirstOrDefault(p => p.IdProducto == item.IdProducto);
        if (prod == null) return;
        int nueva = item.Cantidad + cambio;
        if (nueva <= 0) carrito.Remove(item);
        else if (nueva <= prod.StockActual) { item.Cantidad = nueva; item.Subtotal = nueva * item.PrecioUnitario; }
        else alertaStock = $"⚠ Stock máximo: {prod.StockActual} unid.";
    }

    private async Task Guardar(bool esAnulada)
    {
        venta.Total = Total;
        venta.Subtotal = Total;
        venta.MontoRecibido = montoRecibido;
        venta.Cambio = Cambio;
        venta.Anulada = esAnulada;
        venta.DetalleVenta = carrito.Select(c => new DetalleVentum
        {
            IdProducto = c.IdProducto,
            Cantidad = c.Cantidad,
            PrecioUnitario = c.PrecioUnitario,
            Subtotal = c.Subtotal
        }).ToList();
        await VentaService.AddAsync(venta);

        //Movimiento de inventario si no es anulada
        if (!esAnulada)
        {
            foreach (var item in carrito)
            {
                await ProductoService.ActualizarStockAsync(item.IdProducto, item.Cantidad);
                await MovInventarioService.RegistrarAsync(
                    idProducto: item.IdProducto,
                    tipoMovimiento: "Salida",
                    cantidad: item.Cantidad,
                    idUsuario: venta.IdVendedor,
                    motivo: $"Venta registrada - TKT-{DateTime.Now:yyyyMMdd}-{venta.IdVenta:D4}"
                );
            }

            var nota = new NotaVentum
            {
                IdVenta = venta.IdVenta,
                NumeroTicket = $"TKT-{DateTime.Now:yyyyMMdd}-{venta.IdVenta:D4}"
            };
            await NotaVentaService.CrearAsync(nota);
        }

        mostrarVenta = false;
        await CargarDatos();
    }

    //Totales finales
    private decimal Total => carrito.Sum(i => i.Subtotal);
    private decimal Cambio => montoRecibido > Total ? montoRecibido - Total : 0;

    private async Task Cancelar()
    {
        if (carrito.Any())
            await Guardar(true);
        else
            mostrarVenta = false;
    }

    private async Task Anular(Ventum v)
    {
        await VentaService.AnularAsync(v.IdVenta);
        foreach (var d in v.DetalleVenta)
        {
            await ProductoService.ActualizarStockAsync(d.IdProducto, -d.Cantidad);
            await MovInventarioService.RegistrarAsync(
                idProducto: d.IdProducto,
                tipoMovimiento: "Entrada",
                cantidad: d.Cantidad,
                idUsuario: AuthService.CurrentUser!.IdUsuario,
                motivo: $"Devolución por anulación de venta: "
            );
        }
        await CargarDatos();
    }

    private async Task VerDetalle(int id)
    {
        ventaDetalle = await VentaService.GetByIdAsync(id);
        notaDetalle = await NotaVentaService.GetByVentaAsync(id);
        if (ventaDetalle != null) mostrarDetalle = true;
    }

    private void CerrarDetalle()
    {
        mostrarDetalle = false;
        ventaDetalle = null;
        notaDetalle = null;
    }

    private async Task DescargarPDF()
    {
        if (ventaDetalle == null) return;

        var estado = ventaDetalle.Anulada
            ? "<span class='badge danger'>ANULADA</span>"
            : "<span class='badge success'>COMPLETADA</span>";

        var ticket = notaDetalle != null ? $"<p class='sub'>🎫 {notaDetalle.NumeroTicket}</p>" : "";

        var filas = string.Join("", ventaDetalle.DetalleVenta.Select(d => $"""
        <tr>
            <td>{d.IdProductoNavigation?.Nombre}</td>
            <td style='text-align:center'>{d.Cantidad}</td>
            <td style='text-align:right;color:#888'>{d.PrecioUnitario:N2}</td>
            <td style='text-align:right;font-weight:bold'>{d.Subtotal:N2}</td>
        </tr>
    """));

        var cambio = (ventaDetalle.Cambio ?? 0) > 0
            ? $"<div style='display:flex;justify-content:space-between'><span style='color:#888;font-size:12px'>Cambio</span><span style='color:green;font-weight:bold'>{ventaDetalle.Cambio:N2} Bs.</span></div>"
            : "";

        var html = $"""
        <h4>MINIMARKET JADE</h4>
        <p class='sub'>{ventaDetalle.FechaHora:dd MMMM yyyy, HH:mm}</p>
        {ticket}
        <div style='text-align:center;margin:6px 0'>{estado}</div>

        <div class='info-grid'>
            <div class='info-box'><div class='lbl'>Cliente</div><div class='val'>{ventaDetalle.IdClienteNavigation?.NombreCompleto ?? "Público General"}</div></div>
            <div class='info-box'><div class='lbl'>Vendedor</div><div class='val'>{ventaDetalle.IdVendedorNavigation?.NombreUsuario}</div></div>
            <div class='info-box'><div class='lbl'>Método de Pago</div><div class='val'>{ventaDetalle.MetodoPago}</div></div>
            <div class='info-box'><div class='lbl'>Monto Recibido</div><div class='val'>{ventaDetalle.MontoRecibido:N2} Bs.</div></div>
        </div>

        <table>
            <thead><tr><th>DESCRIPCIÓN</th><th style='text-align:center'>CANT.</th><th style='text-align:right'>P.UNIT.</th><th style='text-align:right'>SUBTOTAL</th></tr></thead>
            <tbody>{filas}</tbody>
        </table>

        <div style='background:#f8f9fa;padding:12px;border-radius:8px;margin-top:12px'>
            <div style='display:flex;justify-content:space-between;margin-bottom:4px'><span style='color:#888;font-size:12px'>Subtotal</span><span>{ventaDetalle.Subtotal:N2} Bs.</span></div>
            <div style='display:flex;justify-content:space-between;border-top:1px solid #ddd;padding-top:8px' class='total'><span>TOTAL</span><span>{ventaDetalle.Total:N2} Bs.</span></div>
            {cambio}
        </div>
    """;

        await _jsModule!.InvokeVoidAsync("imprimirComprobante", html);
    }

}