using System;
using System.Collections.Generic;

namespace MinimarketJade.Web.Data.Entities;

public partial class ArqueoCaja
{
    public int IdArqueo { get; set; }

    public DateOnly Fecha { get; set; }

    public decimal TotalVentasRegistrado { get; set; }

    public decimal TotalEfectivoFisico { get; set; }

    public decimal Diferencia { get; set; }

    public int IdUsuario { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
