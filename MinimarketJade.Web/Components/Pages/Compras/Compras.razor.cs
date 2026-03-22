using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MinimarketJade.Web.Data.Entities;
using MinimarketJade.Web.Services.Compras;
using MinimarketJade.Web.Services.Proveedores;
using MinimarketJade.Web.Services;

namespace MinimarketJade.Web.Components.Pages.Compras;

public partial class Compras : ComponentBase
{
    // Services injected in the .razor via @inject: CompraService, ProveedorService, ProductoService, JS

    private List<Compra>? compras;
    private List<Proveedor> proveedores = new();
    private List<Producto> productos = new();

    private Compra compraActual = new();
    private List<LineaCompra> lineas = new();

    private bool mostrarModal = false;
    private bool mostrarDetalleModal = false;
    private Compra? compraDetalle;

    private string textoBusquedaProducto = string.Empty;

    private decimal Subtotal => lineas.Sum(l => l.Cantidad * l.PrecioUnitario);
    private decimal Total => Subtotal; // sin impuestos

    private IEnumerable<Producto> productosFiltrados =>
        // Solo filtrar y mostrar productos cuando el usuario haya escrito algo en el buscador
        string.IsNullOrWhiteSpace(textoBusquedaProducto)
            ? Enumerable.Empty<Producto>()
            : productos.Where(p => (p.Nombre != null && p.Nombre.Contains(textoBusquedaProducto, StringComparison.OrdinalIgnoreCase))
              || (p.CodigoBarras != null && p.CodigoBarras.Contains(textoBusquedaProducto, StringComparison.OrdinalIgnoreCase)));

    protected override async Task OnInitializedAsync()
    {
        await CargarDatos();
    }

    private async Task CargarDatos()
    {
        compras = await CompraService.ObtenerTodosAsync();
        proveedores = await ProveedorService.ObtenerTodosAsync();
        productos = await ProductoService.GetAllAsync();
    }

    private void AbrirModal()
    {
        compraActual = new Compra();
        lineas = new List<LineaCompra>();
        mostrarModal = true;
    }

    private void CerrarModal()
    {
        mostrarModal = false;
    }

    private void AgregarLinea(Producto p)
    {
        // Evitar duplicados en líneas
        if (lineas.Any(l => l.Producto.IdProducto == p.IdProducto)) return;

        lineas.Add(new LineaCompra
        {
            Producto = p,
            IdProducto = p.IdProducto,
            Cantidad = 1,
            PrecioUnitario = p.PrecioCompra
        });
    }

    private void RemoverLinea(LineaCompra ln)
    {
        lineas.Remove(ln);
    }

    private void LimpiarBusqueda()
    {
        textoBusquedaProducto = string.Empty;
    }

    private async Task GuardarCompra()
    {
        // Validaciones
        if (compraActual.IdProveedor == 0)
        {
            await JS.InvokeVoidAsync("alert", "Seleccione un proveedor.");
            return;
        }

        if (!lineas.Any())
        {
            await JS.InvokeVoidAsync("alert", "Agregue al menos un producto a la compra.");
            return;
        }

        // Construir detalles
        var detalles = new List<DetalleCompra>();
        foreach (var ln in lineas)
        {
            if (ln.Cantidad <= 0)
            {
                await JS.InvokeVoidAsync("alert", "La cantidad debe ser mayor que cero para todos los productos.");
                return;
            }

            if (ln.PrecioUnitario <= 0)
            {
                await JS.InvokeVoidAsync("alert", "El precio unitario debe ser mayor que cero para todos los productos.");
                return;
            }

            var det = new DetalleCompra
            {
                IdProducto = ln.IdProducto,
                Cantidad = ln.Cantidad,
                PrecioUnitario = ln.PrecioUnitario,
                Subtotal = ln.Cantidad * ln.PrecioUnitario
            };
            detalles.Add(det);
        }

        // Validar número de factura único (si se ingresó)
        if (!string.IsNullOrWhiteSpace(compraActual.NumeroFactura))
        {
            bool existeFactura = await CompraService.ExisteNumeroFacturaAsync(compraActual.NumeroFactura);
            if (existeFactura)
            {
                await JS.InvokeVoidAsync("alert", $"El número de factura '{compraActual.NumeroFactura}' ya existe. Verifique antes de registrar.");
                return;
            }
        }

        try
        {
            await CompraService.CrearCompraAsync(compraActual, detalles);
            mostrarModal = false;
            await CargarDatos();
        }
        catch (Exception ex)
        {
            // Mostrar el mensaje de la excepción raíz para facilitar diagnóstico
            var msg = ex.GetBaseException()?.Message ?? ex.Message;
            await JS.InvokeVoidAsync("alert", "Error al registrar compra: " + msg);
        }
    }

    private async Task VerDetalle(int id)
    {
        compraDetalle = await CompraService.ObtenerPorIdAsync(id);
        mostrarDetalleModal = true;
    }

    private class LineaCompra
    {
        public Producto Producto { get; set; } = null!;
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
