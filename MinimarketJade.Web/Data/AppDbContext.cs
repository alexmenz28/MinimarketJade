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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
