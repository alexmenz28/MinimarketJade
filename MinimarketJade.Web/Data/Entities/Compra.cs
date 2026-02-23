using System;
using System.Collections.Generic;

namespace MinimarketJade.Web.Data.Entities;

public partial class Compra
{
    public int IdCompra { get; set; }

    public DateOnly Fecha { get; set; }

    public string? NumeroFactura { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = null!;

    public int IdProveedor { get; set; }

    public int IdUsuario { get; set; }

    public virtual ICollection<DetalleCompra> DetalleCompras { get; set; } = new List<DetalleCompra>();

    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
