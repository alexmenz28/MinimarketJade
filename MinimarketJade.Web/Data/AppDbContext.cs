using Microsoft.EntityFrameworkCore;

namespace MinimarketJade.Web.Data;

/// <summary>
/// Contexto de base de datos para Minimarket Jade (SQL Server).
/// Conexión configurada en appsettings; las entidades las añadirán los módulos correspondientes.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
