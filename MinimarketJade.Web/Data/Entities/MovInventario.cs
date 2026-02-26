using System;
using System.Collections.Generic;

namespace MinimarketJade.Web.Data.Entities;

public partial class MovInventario
{
    public int IdMovimiento { get; set; }

    public int IdProducto { get; set; }

    public string TipoMovimiento { get; set; } = null!;

    public int Cantidad { get; set; }

    public DateTime FechaHora { get; set; }

    public int IdUsuario { get; set; }

    public string? Motivo { get; set; }

    public virtual Producto IdProductoNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
