using MinimarketJade.Web.Data.Entities;

namespace MinimarketJade.Web.Services.Clientes
{
    public interface IClienteService
    {
        Task<List<Cliente>> ObtenerTodosAsync();
        Task<Cliente?> ObtenerPorIdAsync(int id);
        Task CrearAsync(Cliente cliente);
        Task ActualizarAsync(Cliente cliente);
        Task EliminarAsync(int id);
    }
}
