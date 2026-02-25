namespace MinimarketJade.Web.Data.Entities;

/// <summary>
/// Representa una categoría de productos en el catálogo.
/// La estructura es jerárquica (árbol): una categoría puede tener una categoría padre
/// y varias subcategorías. Si IdCategoriaPadre es null, la categoría es raíz (nivel superior).
/// </summary>
public class Categoria
{
    /// <summary>Identificador único de la categoría (PK en la tabla Categoria).</summary>
    public int IdCategoria { get; set; }

    /// <summary>Nombre de la categoría (ej: "Abarrotes", "Lácteos").</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Id de la categoría padre. Si es null, esta categoría es raíz (no tiene padre).
    /// Si tiene valor, esta categoría es hija de otra (ej: "Leche" es hija de "Lácteos").
    /// </summary>
    public int? IdCategoriaPadre { get; set; }

    // Navegación para Entity Framework (relación recursiva)
    /// <summary>Categoría padre (solo tiene valor si IdCategoriaPadre no es null).</summary>
    public Categoria? CategoriaPadre { get; set; }

    /// <summary>Lista de subcategorías hijas de esta categoría.</summary>
    public ICollection<Categoria> Subcategorias { get; set; } = new List<Categoria>();
}
