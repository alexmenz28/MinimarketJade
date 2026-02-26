using Microsoft.EntityFrameworkCore;
using MinimarketJade.Web.Data.Entities;

namespace MinimarketJade.Web.Data;

/// <summary>
/// Contexto de base de datos para Minimarket Jade (SQL Server).
/// Conexión configurada en appsettings; aquí se registran las entidades (ej: Categoria).
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>Tabla Categoria: categorías de productos con jerarquía (padre/hijos).</summary>
    public DbSet<Categoria> Categorias => Set<Categoria>();

    /// <summary>Tabla Usuario: usuarios del sistema para login (nombre_usuario, contraseña_hash, rol).</summary>
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Mapeo de Usuario: columnas id_usuario, nombre_usuario, contraseña_hash, rol, activo (script 01_CreateTables.sql).
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuario");
            e.HasKey(x => x.IdUsuario);
            e.Property(x => x.IdUsuario).HasColumnName("id_usuario");
            e.Property(x => x.NombreUsuario).HasMaxLength(50).HasColumnName("nombre_usuario");
            e.Property(x => x.ContraseñaHash).HasMaxLength(256).HasColumnName("contraseña_hash");
            e.Property(x => x.Rol).HasMaxLength(20).HasColumnName("rol");
            e.Property(x => x.Activo).HasColumnName("activo");
        });

        // Mapeo de Categoria: nombres de columnas coinciden con el script SQL (id_categoria, nombre, id_categoria_padre).
        // Relación recursiva: una categoría puede tener una categoría padre y varias subcategorías (árbol de categorías).
        modelBuilder.Entity<Categoria>(e =>
        {
            e.ToTable("Categoria");
            e.HasKey(x => x.IdCategoria);
            e.Property(x => x.IdCategoria).HasColumnName("id_categoria");
            e.Property(x => x.Nombre).HasMaxLength(100).HasColumnName("nombre");
            e.Property(x => x.IdCategoriaPadre).HasColumnName("id_categoria_padre");
            e.HasOne(x => x.CategoriaPadre)
                .WithMany(x => x.Subcategorias)
                .HasForeignKey(x => x.IdCategoriaPadre)
                .IsRequired(false);
        });
    }
}
