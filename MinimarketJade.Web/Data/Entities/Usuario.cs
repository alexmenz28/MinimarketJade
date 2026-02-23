using System;
using System.Collections.Generic;

namespace MinimarketJade.Web.Data.Entities;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string ContraseñaHash { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<ArqueoCaja> ArqueoCajas { get; set; } = new List<ArqueoCaja>();

    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();

    public virtual ICollection<MovInventario> MovInventarios { get; set; } = new List<MovInventario>();

    public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();
}
