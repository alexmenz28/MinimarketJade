using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MinimarketJade.Web.Data.Entities;

public partial class Proveedor
{
    public int IdProveedor { get; set; }

    [Required]
    [MaxLength(200)]
    public string Nombre { get; set; } = null!; // Mapeado a columna razon_social en DbContext

    // Mantener NitRuc por compatibilidad con la base existente (opcional)
    [MaxLength(30)]
    public string? NitRuc { get; set; }

    [Required]
    [MaxLength(20)]
    public string Telefono { get; set; } = null!;

    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(300)]
    public string? Direccion { get; set; }

    [MaxLength(100)]
    public string? Contacto { get; set; }

    [MaxLength(100)]
    public string? TipoProducto { get; set; }

    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();
}
