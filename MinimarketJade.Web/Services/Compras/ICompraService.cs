using MinimarketJade.Web.Data.Entities;

namespace MinimarketJade.Web.Services.Compras
{
    public interface ICompraService
    {
        Task<List<Compra>> ObtenerTodosAsync();
        Task<Compra?> ObtenerPorIdAsync(int id);
        Task<string> GenerarNumeroFacturaAsync();
        Task CrearCompraAsync(Compra compra, List<DetalleCompra> detalles);
        Task<bool> ExisteNumeroFacturaAsync(string numeroFactura);
        // KPIs
        Task<List<PrecioPromedioProductoDto>> ObtenerPrecioCompraPromedioPorProductoAsync(int year, int month);
        Task<List<GastoProveedorDto>> ObtenerGastoPorProveedorAsync(DateTime? desde = null, DateTime? hasta = null);
        Task<decimal> ObtenerConcentracionTop3ProveedoresAsync(DateTime? desde = null, DateTime? hasta = null);
        // Precio estándar por categoría (por ejemplo promedio de precio_compra por categoría)
        Task<List<CategoriaPrecioDto>> ObtenerPrecioEstandarPorCategoriaAsync();
    }
}

public class PrecioPromedioProductoDto
{
    public int IdProducto { get; set; }
    public string? NombreProducto { get; set; }
    public decimal PrecioPromedio { get; set; }
    public decimal TotalGastado { get; set; }
    public int CantidadTotal { get; set; }
    // Costo estándar asociado al producto (si está disponible)
    public decimal CostoEstandar { get; set; }
}

public class GastoProveedorDto
{
    public int IdProveedor { get; set; }
    public string? RazonSocial { get; set; }
    public decimal TotalGastado { get; set; }
    public decimal Porcentaje { get; set; }
}

public class CategoriaPrecioDto
{
    public int IdCategoria { get; set; }
    public string? NombreCategoria { get; set; }
    public decimal PrecioEstandar { get; set; }
}
