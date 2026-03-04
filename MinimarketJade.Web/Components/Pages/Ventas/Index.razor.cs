using Microsoft.AspNetCore.Components;
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

    //Totales finales
    private decimal Total => carrito.Sum(i => i.Subtotal);
    private decimal Cambio => montoRecibido > Total ? montoRecibido - Total : 0;

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
        var prod = productos.FirstOrDefault(p => p.IdProducto == idProducto);
        if (prod == null) return;

        var item = carrito.FirstOrDefault(c => c.IdProducto == prod.IdProducto);
        if (prod.StockActual > (item?.Cantidad ?? 0))
        {
            if (item != null) { 
                item.Cantidad++; item.Subtotal = item.Cantidad * item.PrecioUnitario;
            }
            else carrito.Add(new DetalleVentum { 
                IdProducto = prod.IdProducto, IdProductoNavigation = prod, Cantidad = 1, PrecioUnitario = prod.PrecioVenta, Subtotal = prod.PrecioVenta 
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
                motivo: $"Devolución por anulación de venta"
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
}