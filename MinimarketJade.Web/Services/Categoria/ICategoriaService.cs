using MinimarketJade.Web.Data.Entities;

namespace MinimarketJade.Web.Services.Categorias;

public interface ICategoriaService
{
    // Listar 
    Task<List<Categoria>> GetAllAsync(CancellationToken ct = default);
}