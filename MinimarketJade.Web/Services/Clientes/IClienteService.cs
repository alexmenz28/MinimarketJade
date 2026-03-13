using MinimarketJade.Web.Data.Entities;

namespace MinimarketJade.Web.Services.Clientes
{
    public interface IClienteService
    {
        Task<List<Cliente>> ObtenerTodosAsync();
        Task<Cliente?> ObtenerPorIdAsync(int id);
        Task<bool> CrearAsync(Cliente cliente);
        Task<bool> ActualizarAsync(Cliente cliente);
        Task EliminarAsync(int id);
    }
}
