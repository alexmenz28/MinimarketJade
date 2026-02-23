using Microsoft.EntityFrameworkCore;
using MinimarketJade.Web.Data;
using MinimarketJade.Web.Data.Entities;

namespace MinimarketJade.Web.Services.Categorias;

public class CategoriaService : ICategoriaService
{
    private readonly AppDbContext _db;

    public CategoriaService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Categoria>> GetAllAsync(CancellationToken ct = default)
    {
        //Enlistar las categorias
        return await _db.Categoria.ToListAsync(ct);
    }
}