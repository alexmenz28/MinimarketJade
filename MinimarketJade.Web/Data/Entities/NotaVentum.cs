using System;
using System.Collections.Generic;

namespace MinimarketJade.Web.Data.Entities;

public partial class NotaVentum
{
    public int IdNota { get; set; }

    public int IdVenta { get; set; }

    public string NumeroTicket { get; set; } = null!;

    public virtual Ventum IdVentaNavigation { get; set; } = null!;
}
