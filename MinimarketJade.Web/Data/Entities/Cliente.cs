using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MinimarketJade.Web.Data.Entities;

public partial class Cliente
{
    public int IdCliente { get; set; }

    [Required]
    [StringLength(30)]
    public string DocumentoIdentidad { get; set; } = null!;

    [Required]
    [StringLength(200)]
    public string NombreCompleto { get; set; } = null!;

    [StringLength(20)]
    public string? Telefono { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(300)]
    public string? Direccion { get; set; }

    public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();
}
