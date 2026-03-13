using MinimarketJade.Web.Data.Entities;
using MinimarketJade.Web.Models;
using MinimarketJade.Web.Services;
using MinimarketJade.Web.Services.Categorias;
using MinimarketJade.Web.Services.Productos;
using MinimarketJade.Web.Helpers;
using Microsoft.AspNetCore.Components;

namespace MinimarketJade.Web.Components.Pages.Productos;

public partial class Index : ComponentBase
{
    [Inject] private IProductoService ProductoService { get; set; } = default!;
    [Inject] private ICategoriaService CategoriaService { get; set; } = default!;

    private List<Producto> productos = new();
    private IReadOnlyList<CategoriaDto> categorias = Array.Empty<CategoriaDto>();
    private Producto producto = new();

    // Interfaces
    private bool mostrarModal = false;
    private bool mostrarDetalle = false;
    private bool esEdicion = false;
    private string? error;
    private bool autorizarVentaConPerdida = false;

    // Tabla Principal
    private string busqueda = "";
    private int idCategoriaFiltro = 0;  
    private bool? estadoFiltro = null;
    private bool mostrarDropCategoria = false;
    private bool mostrarDropEstado = false;

    private string busquedaCategoria = "";

    private IEnumerable<Producto> ProductosFiltrados => productos
        .Where(p => idCategoriaFiltro == 0 || p.IdCategoria == idCategoriaFiltro)
        .Where(p => !estadoFiltro.HasValue || p.Activo == estadoFiltro.Value)
        .Where(p => string.IsNullOrWhiteSpace(busqueda) ||
            (p.Nombre?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (p.CodigoBarras?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false))
        .OrderByDescending(p => p.Activo);

    private IEnumerable<CategoriaDto> CategoriasFiltradasMenu => categorias
        .Where(c => string.IsNullOrWhiteSpace(busquedaCategoria) ||
               (c.Nombre?.Contains(busquedaCategoria, StringComparison.OrdinalIgnoreCase) ?? false));

    protected override async Task OnInitializedAsync() => await CargarDatos();

    private async Task CargarDatos()
    {
        productos = await ProductoService.GetAllAsync();
        categorias = await CategoriaService.GetAllAsync();
    }

    private void LimpiarFiltros()
    {
        busqueda = "";
        idCategoriaFiltro = 0;
        estadoFiltro = null;
        busquedaCategoria = "";
        mostrarDropCategoria = false;
        mostrarDropEstado = false;
    }

    private void GenerarCodigo()
    {
        error = null;
        var random = new Random();
        string nuevoCodigo;
        bool existe;
        do
        {
            nuevoCodigo = "777";
            for (int i = 0; i < 6; i++) nuevoCodigo += random.Next(0, 10);
            existe = productos.Any(p => p.CodigoBarras == nuevoCodigo);
        } while (existe);
        producto.CodigoBarras = nuevoCodigo;
    }

    private void AbrirModal()
    {
        error = null;
        autorizarVentaConPerdida = false;
        esEdicion = false;
        producto = new Producto { Activo = true, UnidadMedida = "Unidad", StockMinimo = 5, CodigoBarras = "", IdCategoria = 0 };
        mostrarModal = true;
    }

    private void VerDetalle(Producto p)
    {
        producto = p;
        mostrarDetalle = true;
    }

    private void EditarProducto(Producto p)
    {
        error = null;
        autorizarVentaConPerdida = false;
        esEdicion = true;
        producto = new Producto
        {
            IdProducto = p.IdProducto,
            Nombre = p.Nombre,
            CodigoBarras = p.CodigoBarras,
            PrecioCompra = p.PrecioCompra,
            PrecioVenta = p.PrecioVenta,
            StockActual = p.StockActual,
            StockMinimo = p.StockMinimo,
            UnidadMedida = p.UnidadMedida,
            Descripcion = p.Descripcion,
            IdCategoria = p.IdCategoria,
            Activo = p.Activo,
        };
        mostrarModal = true;
    }

    private void CerrarModal()
    {
        error = null;
        mostrarModal = false;
    }

    private async Task Guardar()
    {
        error = null;

        if (string.IsNullOrWhiteSpace(producto.Nombre)) { error = "⚠ El nombre es obligatorio."; return; }
        if (string.IsNullOrWhiteSpace(producto.CodigoBarras)) { error = "⚠ El código de barras es obligatorio. Usa 'Generar'."; return; }
        if (producto.PrecioCompra <= 0) { error = "⚠ El precio de compra debe ser mayor a 0."; return; }
        if (producto.PrecioVenta <= 0) { error = "⚠ El precio de venta debe ser mayor a 0."; return; }
        if (producto.PrecioVenta < producto.PrecioCompra && !autorizarVentaConPerdida)
        {
            error = "⚠ El precio de venta no puede ser menor al de compra. Si es una liquidación o promoción, marque 'Autorizar venta con pérdida'.";
            return;
        }
        if (producto.StockActual < 0) { error = "⚠ El stock no puede ser negativo."; return; }
        if (producto.StockActual == 0 && !esEdicion) { error = "⚠ El stock inicial debe ser mayor a 0."; return; }
        if (producto.IdCategoria <= 0) { error = "⚠ Debes seleccionar una categoría."; return; }

        try
        {
            bool existe = await ProductoService.ExisteNombreAsync(producto.Nombre, producto.IdProducto);
            if (existe) { error = $"⚠ El producto '{producto.Nombre}' ya existe."; return; }

            if (esEdicion) await ProductoService.UpdateAsync(producto);
            else await ProductoService.AddAsync(producto);

            mostrarModal = false;
            await CargarDatos();
        }
        catch (Exception ex)
        {
            error = "❌ Error al conectar con la base de datos: " + ex.Message;
        }
    }

    private async Task InhabilitarProducto(Producto p)
    {
        await ProductoService.InhabilitarAsync(p.IdProducto);
        await CargarDatos();
    }

    private async Task HabilitarProducto(Producto p)
    {
        p.Activo = true;
        await ProductoService.UpdateAsync(p);
        await CargarDatos();
    }
}