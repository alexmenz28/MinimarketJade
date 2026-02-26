using System.Security.Cryptography;
using System.Text;

namespace MinimarketJade.Web.Services;

/// <summary>
/// Genera y verifica el hash de contraseñas (SHA256 + Base64).
/// Para producción se recomienda BCrypt o Argon2; este formato coincide con la guía de login existente.
/// </summary>
public static class PasswordHelper
{
    public static string Hash(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    public static bool Verify(string password, string storedHash)
    {
        var computed = Hash(password);
        return string.Equals(computed, storedHash, StringComparison.Ordinal);
    }
}
