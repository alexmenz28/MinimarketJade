namespace MinimarketJade.Web.Models;

/// <summary>
/// DTO (objeto de transferencia) para exponer una categoría al frontend.
/// Se usa en listas planas y formularios; no incluye la jerarquía de hijos.
/// </summary>
public class CategoriaDto
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = string.Empty;
    /// <summary>Id de la categoría padre; null si es categoría raíz.</summary>
    public int? IdCategoriaPadre { get; set; }
}
