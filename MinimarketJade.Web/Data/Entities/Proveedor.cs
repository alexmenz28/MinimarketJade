using System;
using System.Collections.Generic;

namespace MinimarketJade.Web.Data.Entities;

public partial class Proveedor
{
    public int IdProveedor { get; set; }

    public string RazonSocial { get; set; } = null!;

    public string NitRuc { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string? Direccion { get; set; }

    public string? Contacto { get; set; }

    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();
}
