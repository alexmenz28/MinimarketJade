namespace MinimarketJade.Web.Models;

/// <summary>
/// DTO que representa una categoría con sus subcategorías anidadas (árbol).
/// Se usa cuando el frontend necesita mostrar la jerarquía completa (ej: menú o selector en cascada).
/// </summary>
public class CategoriaTreeDto
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int? IdCategoriaPadre { get; set; }
    /// <summary>Subcategorías hijas de esta categoría (pueden tener a su vez hijos).</summary>
    public List<CategoriaTreeDto> Hijos { get; set; } = new();
}
