namespace MinimarketJade.Web.Services.Reportes;

public interface IReporteService
{
    Task<List<IngresosDiarios>> GetIngresosDiariosAsync(int dias = 7);
    Task<List<VendedorProductividad>> GetProductividadVendedoresAsync();


}

public class IngresosDiarios
{
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
}

public class VendedorProductividad
{
    public string NombreVendedor { get; set; } = "";
    public decimal TotalVentas { get; set; }
    public decimal Meta { get; set; }
    public decimal Porcentaje => Meta > 0 ? (TotalVentas / Meta * 100) : 0;
}