using System;
using System.Collections.Generic;

namespace MinimarketJade.Web.Data.Entities;

public partial class Cliente
{
    public int IdCliente { get; set; }

    public string DocumentoIdentidad { get; set; } = null!;

    public string NombreCompleto { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string? Direccion { get; set; }

    public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();
}
