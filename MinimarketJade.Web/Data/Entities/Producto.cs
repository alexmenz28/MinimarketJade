using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinimarketJade.Web.Data.Entities;

[Table("Producto")]
public partial class Producto
{
    [Key]
    [Column("id_producto")]
    public int IdProducto { get; set; }

    [Column("codigo_barras")]
    public string? CodigoBarras { get; set; }

    [Column("nombre")]
    [Required]
    public string Nombre { get; set; } = null!;

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("id_categoria")]
    public int IdCategoria { get; set; }

    [Column("precio_compra")]
    public decimal PrecioCompra { get; set; }

    [Column("precio_venta")]
    public decimal PrecioVenta { get; set; }

    [Column("stock_actual")]
    public int StockActual { get; set; }

    [Column("stock_minimo")]
    public int StockMinimo { get; set; }

    [Column("unidad_medida")]
    public string UnidadMedida { get; set; } = "Unidad";

    [Column("activo")]
    public bool Activo { get; set; } = true;

    [ForeignKey("IdCategoria")]
    public virtual Categoria? Categoria { get; set; }
    //Navegacion
    public virtual ICollection<DetalleCompra> DetalleCompras { get; set; } = new List<DetalleCompra>();
    public virtual ICollection<DetalleVentum> DetalleVenta { get; set; } = new List<DetalleVentum>();
    public virtual ICollection<MovInventario> MovInventarios { get; set; } = new List<MovInventario>();
}