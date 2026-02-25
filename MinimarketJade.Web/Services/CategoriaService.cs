using Microsoft.EntityFrameworkCore;
using MinimarketJade.Web.Data;
using MinimarketJade.Web.Data.Entities;
using MinimarketJade.Web.Models;

namespace MinimarketJade.Web.Services;

/// <summary>
/// Implementación del servicio de categorías.
/// Gestiona la lectura y escritura de la tabla Categoria (jerarquía padre/hijos).
/// </summary>
public class CategoriaService : ICategoriaService
{
    private readonly AppDbContext _db;

    public CategoriaService(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoriaDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _db.Categorias
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaDto
            {
                IdCategoria = c.IdCategoria,
                Nombre = c.Nombre,
                IdCategoriaPadre = c.IdCategoriaPadre
            })
            .ToListAsync(ct);
        return list;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoriaDto>> GetRaicesAsync(CancellationToken ct = default)
    {
        var list = await _db.Categorias
            .Where(c => c.IdCategoriaPadre == null)
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaDto
            {
                IdCategoria = c.IdCategoria,
                Nombre = c.Nombre,
                IdCategoriaPadre = null
            })
            .ToListAsync(ct);
        return list;
    }

    /// <inheritdoc />
    /// <remarks>Construye el árbol en memoria: carga todas las categorías y agrupa por IdCategoriaPadre para asignar Hijos.</remarks>
    public async Task<IReadOnlyList<CategoriaTreeDto>> GetArbolAsync(CancellationToken ct = default)
    {
        var todas = await _db.Categorias.OrderBy(c => c.Nombre).ToListAsync(ct);
        var dict = todas.Select(c => new CategoriaTreeDto
        {
            IdCategoria = c.IdCategoria,
            Nombre = c.Nombre,
            IdCategoriaPadre = c.IdCategoriaPadre
        }).ToDictionary(x => x.IdCategoria);

        foreach (var n in dict.Values)
        {
            if (n.IdCategoriaPadre.HasValue && dict.TryGetValue(n.IdCategoriaPadre.Value, out var padre))
                padre.Hijos.Add(n);
        }

        // Devolver solo raíces (las que no tienen padre); sus Hijos ya están rellenados
        return dict.Values.Where(n => !n.IdCategoriaPadre.HasValue).OrderBy(n => n.Nombre).ToList();
    }

    /// <inheritdoc />
    public async Task<CategoriaDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var c = await _db.Categorias.AsNoTracking().FirstOrDefaultAsync(x => x.IdCategoria == id, ct);
        return c == null ? null : new CategoriaDto { IdCategoria = c.IdCategoria, Nombre = c.Nombre, IdCategoriaPadre = c.IdCategoriaPadre };
    }

    /// <inheritdoc />
    public async Task<CategoriaDto> CreateAsync(CategoriaDto dto, CancellationToken ct = default)
    {
        var entidad = new Categoria
        {
            Nombre = dto.Nombre.Trim(),
            IdCategoriaPadre = dto.IdCategoriaPadre
        };
        _db.Categorias.Add(entidad);
        await _db.SaveChangesAsync(ct);
        return new CategoriaDto { IdCategoria = entidad.IdCategoria, Nombre = entidad.Nombre, IdCategoriaPadre = entidad.IdCategoriaPadre };
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(int id, CategoriaDto dto, CancellationToken ct = default)
    {
        var c = await _db.Categorias.FirstOrDefaultAsync(x => x.IdCategoria == id, ct);
        if (c == null) return false;
        c.Nombre = dto.Nombre.Trim();
        c.IdCategoriaPadre = dto.IdCategoriaPadre;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    /// <inheritdoc />
    /// <remarks>No permite borrar una categoría que tenga subcategorías (hijos), para mantener integridad.</remarks>
    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var c = await _db.Categorias.Include(x => x.Subcategorias).FirstOrDefaultAsync(x => x.IdCategoria == id, ct);
        if (c == null) return false;
        if (c.Subcategorias.Count > 0) return false; // Tiene hijos; no borrar
        _db.Categorias.Remove(c);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
