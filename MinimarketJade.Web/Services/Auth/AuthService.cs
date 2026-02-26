using MinimarketJade.Web.Data.Entities;

namespace MinimarketJade.Web.Services.Auth;

/// <summary>
/// Servicio de autenticación en memoria (singleton). Guarda el usuario actual tras el login
/// y expone si está autenticado y el rol. El frontend lo inyecta para proteger rutas y mostrar menú.
/// </summary>
public class AuthService
{
    public Usuario? CurrentUser { get; private set; }
    public string RolNombre => CurrentUser?.Rol ?? "";

    public bool IsAuthenticated => CurrentUser != null;
    public bool IsAdministrador => string.Equals(RolNombre, "Administrador", StringComparison.OrdinalIgnoreCase);
    public bool IsVendedor => string.Equals(RolNombre, "Vendedor", StringComparison.OrdinalIgnoreCase);

    public void SignIn(Usuario user)
    {
        CurrentUser = user;
    }

    public void SignOut()
    {
        CurrentUser = null;
    }
}
