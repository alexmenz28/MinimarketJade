namespace MinimarketJade.Web.Data.Entities;

/// <summary>
/// Categoría de productos (catálogo).
/// </summary>
public class Categoria
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int? IdCategoriaPadre { get; set; }

    // Navegación
    public Categoria? CategoriaPadre { get; set; }
    public ICollection<Categoria> Subcategorias { get; set; } = new List<Categoria>();
}
