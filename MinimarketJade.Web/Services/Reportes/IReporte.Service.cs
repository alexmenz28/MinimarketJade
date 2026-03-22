namespace MinimarketJade.Web.Services.Reportes;

public interface IReporteService
{
    Task<List<IngresosDiarios>> GetIngresosDiariosAsync(int dias = 7);
    Task<List<VendedorProductividad>> GetProductividadVendedoresAsync();
    Task<ClientesRecurrentesDto> GetClientesRecurrentesAsync(int metaMensual = 40);
    Task<List<ClientesMesDto>> GetClientesRecurrentesPorMesAsync(int anio, int metaMensual = 40);
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

public class ClientesRecurrentesDto
{
    public int ClientesRecurrentes { get; set; }
    public int Meta { get; set; }
    public decimal Porcentaje => Meta > 0 ? (ClientesRecurrentes * 100m / Meta) : 0;
}

public class ClientesMesDto
{
    public int Mes { get; set; }
    public string NombreMes { get; set; } = "";
    public int ClientesRecurrentes { get; set; }
    public int Meta { get; set; }
    public decimal Porcentaje => Meta > 0 ? (ClientesRecurrentes * 100m / Meta) : 0;
    public string Color => Porcentaje >= 120 ? "#1D9E75"
        : Porcentaje >= 100 ? "#28a745"
        : Porcentaje >= 75 ? "#ffc107"
        : "#dc3545";
}