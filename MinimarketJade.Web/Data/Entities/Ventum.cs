using System;
using System.Collections.Generic;

namespace MinimarketJade.Web.Data.Entities;

public partial class Ventum
{
    public int IdVenta { get; set; }

    public DateTime FechaHora { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Impuestos { get; set; }

    public decimal Descuentos { get; set; }

    public decimal Total { get; set; }

    public string MetodoPago { get; set; } = null!;

    public decimal? MontoRecibido { get; set; }

    public decimal? Cambio { get; set; }

    public int IdVendedor { get; set; }

    public int? IdCliente { get; set; }

    public bool Anulada { get; set; }

    public virtual ICollection<DetalleVentum> DetalleVenta { get; set; } = new List<DetalleVentum>();

    public virtual Cliente? IdClienteNavigation { get; set; }

    public virtual Usuario IdVendedorNavigation { get; set; } = null!;

    public virtual NotaVentum? NotaVentum { get; set; }
}
