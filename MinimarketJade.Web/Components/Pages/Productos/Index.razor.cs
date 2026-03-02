using MinimarketJade.Web.Data.Entities;
using MinimarketJade.Web.Services;
using MinimarketJade.Web.Services.Categorias;
using MinimarketJade.Web.Helpers;
using Microsoft.AspNetCore.Components;

namespace MinimarketJade.Web.Components.Pages.Productos;

public partial class Index : ComponentBase
{
    [Inject] private IProductoService ProductoService { get; set; } = default!;
    [Inject] private ICategoriaService CategoriaService { get; set; } = default!;

    private List<Producto> productos = new();
    private List<Categoria> categorias = new();
    private Producto producto = new();
    private bool mostrarModal = false;
    private bool mostrarDetalle = false;
    private bool esEdicion = false;
    private string? error;
    private string busqueda = "";

    private IEnumerable<Producto> ProductosFiltrados =>
        string.IsNullOrWhiteSpace(busqueda)
            ? productos.OrderByDescending(p => p.Activo)
            : productos.Where(p =>
                (p.Nombre?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (p.CodigoBarras?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (p.Categoria?.Nombre?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false))
              .OrderByDescending(p => p.Activo);

    protected override async Task OnInitializedAsync() => await CargarDatos();

    private async Task CargarDatos()
    {
        productos = await ProductoService.GetAllAsync();
        categorias = await CategoriaService.GetAllAsync();
    }

    private void GenerarCodigo()
    {
        var random = new Random();
        string codigo = "777";
        for (int i = 0; i < 6; i++) codigo += random.Next(0, 10);
        producto.CodigoBarras = codigo;
    }

    private void AbrirModal()
    {
        error = null;
        esEdicion = false;
        producto = new Producto { Activo = true, UnidadMedida = "Unidad", StockMinimo = 0, CodigoBarras = "", IdCategoria = 1 };
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

    private void CerrarModal() => mostrarModal = false;

    private async Task Guardar()
    {
        error = null;
        bool existe = await ProductoService.ExisteNombreAsync(producto.Nombre, producto.IdProducto);
        if (existe)
        {
            error = $"El nombre '{producto.Nombre}' ya existe. Elige uno diferente.";
            return;
        }

        if (esEdicion) await ProductoService.UpdateAsync(producto);
        else await ProductoService.AddAsync(producto);

        mostrarModal = false;
        await CargarDatos();
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