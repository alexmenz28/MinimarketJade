using MinimarketJade.Web.Data.Entities;

namespace MinimarketJade.Web.Services.Proveedores
{
    public interface IProveedorService
    {
        Task<List<Proveedor>> ObtenerTodosAsync();
        Task<Proveedor?> ObtenerPorIdAsync(int id);
        Task CrearAsync(Proveedor proveedor);
        Task ActualizarAsync(Proveedor proveedor);
        Task EliminarAsync(int id);
    }
}
