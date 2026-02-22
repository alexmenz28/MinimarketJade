using Microsoft.EntityFrameworkCore;
using MinimarketJade.Web.Data.Entities;

namespace MinimarketJade.Web.Data;

/// <summary>
/// Contexto de base de datos para Minimarket Jade (SQL Server).
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Categoria> Categorias => Set<Categoria>();

    // DbSets según el diagrama ER (se irán añadiendo entidades)
    // public DbSet<Producto> Productos => Set<Producto>();
    // public DbSet<Usuario> Usuarios => Set<Usuario>();
    // public DbSet<Cliente> Clientes => Set<Cliente>();
    // public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    // public DbSet<Venta> Ventas => Set<Venta>();
    // public DbSet<DetalleVenta> DetalleVentas => Set<DetalleVenta>();
    // public DbSet<NotaVenta> NotaVentas => Set<NotaVenta>();
    // public DbSet<Compra> Compras => Set<Compra>();
    // public DbSet<DetalleCompra> DetalleCompras => Set<DetalleCompra>();
    // public DbSet<MovInventario> MovimientosInventario => Set<MovInventario>();
    // public DbSet<ArqueoCaja> ArqueosCaja => Set<ArqueoCaja>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Categoria>(e =>
        {
            e.ToTable("Categoria");
            e.HasKey(x => x.IdCategoria);
            e.Property(x => x.Nombre).HasMaxLength(100);
            e.HasOne(x => x.CategoriaPadre)
                .WithMany(x => x.Subcategorias)
                .HasForeignKey(x => x.IdCategoriaPadre)
                .IsRequired(false);
        });
    }
}
