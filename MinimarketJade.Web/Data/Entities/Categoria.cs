using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinimarketJade.Web.Data.Entities;

/// <summary>
/// Categoría de productos (catálogo).
/// </summary>
[Table("Categoria")] 
public class Categoria
{
    
    [Key]
    [Column("id_categoria")]
    public int IdCategoria { get; set; }
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;
    [Column("id_categoria_padre")]
    public int? IdCategoriaPadre { get; set; }

    // Navegación 

    [ForeignKey("IdCategoriaPadre")]
    public virtual Categoria? CategoriaPadre { get; set; }
    public virtual ICollection<Categoria> Subcategorias { get; set; } = new List<Categoria>();
    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}