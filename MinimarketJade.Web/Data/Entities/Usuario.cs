using System;
using System.Collections.Generic;

namespace MinimarketJade.Web.Data.Entities;

/// <summary>
/// Usuario del sistema (tabla Usuario). Se usa para login y control de acceso por rol.
/// Rol es texto: "Administrador" o "Vendedor" (según el script de BD).
/// Incluye colecciones de navegación para ArqueoCaja, Compra, MovInventario y Venta (EF).
/// </summary>
public class Usuario
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    /// <summary>Contraseña hasheada (nunca guardar en texto plano).</summary>
    public string ContraseñaHash { get; set; } = string.Empty;
    /// <summary>Rol del usuario: "Administrador" o "Vendedor".</summary>
    public string Rol { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public virtual ICollection<ArqueoCaja> ArqueoCajas { get; set; } = new List<ArqueoCaja>();
    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();
    public virtual ICollection<MovInventario> MovInventarios { get; set; } = new List<MovInventario>();
    public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();
}
