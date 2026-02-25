using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinimarketJade.Web.Data.Entities;

public partial class Proveedor
{
    public int IdProveedor { get; set; }

    [Required]
    [MaxLength(200)]
    public string RazonSocial { get; set; } = null!; // mapea a razon_social

    [Required]
    [MaxLength(30)]
    public string NitRuc { get; set; } = null!; // mapea a nit_ruc (único)

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(300)]
    public string? Direccion { get; set; }

    [MaxLength(100)]
    public string? Contacto { get; set; }

    [NotMapped]
    public bool Activo { get; set; } = true;

    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();
}
