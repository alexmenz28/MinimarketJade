using MinimarketJade.Web.Data.Entities;

namespace MinimarketJade.Web.Services;

public interface IVentaService
{
    Task<List<Ventum>> GetAllAsync();

    Task<Ventum?> GetByIdAsync(int id);

    Task<int> AddAsync(Ventum venta);

    Task AnularAsync(int id);

    Task<List<Ventum>> GetByClienteAsync(int idCliente);
}