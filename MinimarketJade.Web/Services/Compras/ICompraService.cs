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
    }
}
