using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MinimarketJade.Web.Data.Entities;

namespace MinimarketJade.Web.Data;

/// <summary>
/// Contexto de base de datos para Minimarket Jade (SQL Server).
/// Conexión configurada en appsettings; aquí se registran las entidades (ej: Categoria).
/// </summary>
public class AppDbContext : DbContext
public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    public virtual DbSet<Categoria> Categoria { get; set; }

    public virtual DbSet<ArqueoCaja> ArqueoCajas { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Compra> Compras { get; set; }

    public virtual DbSet<DetalleCompra> DetalleCompras { get; set; }

    public virtual DbSet<DetalleVentum> DetalleVenta { get; set; }

    public virtual DbSet<MovInventario> MovInventarios { get; set; }

    public virtual DbSet<NotaVentum> NotaVenta { get; set; }

    /// <summary>Tabla Categoria: categorías de productos con jerarquía (padre/hijos).</summary>
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public virtual DbSet<Producto> Productos { get; set; }

    /// <summary>Tabla Usuario: usuarios del sistema para login (nombre_usuario, contraseña_hash, rol).</summary>
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public virtual DbSet<Proveedor> Proveedors { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Ventum> Venta { get; set; }

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
        modelBuilder.UseCollation("Latin1_General_CI_AI");

        modelBuilder.Entity<ArqueoCaja>(entity =>
        {
            entity.HasKey(e => e.IdArqueo);

            entity.ToTable("ArqueoCaja");

            entity.Property(e => e.IdArqueo).HasColumnName("id_arqueo");
            entity.Property(e => e.Diferencia)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("diferencia");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.TotalEfectivoFisico)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total_efectivo_fisico");
            entity.Property(e => e.TotalVentasRegistrado)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total_ventas_registrado");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.ArqueoCajas)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ArqueoCaja_Usuario");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente);

            entity.ToTable("Cliente");

            entity.HasIndex(e => e.DocumentoIdentidad, "UQ_Cliente_documento").IsUnique();

            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.Direccion)
                .HasMaxLength(300)
                .HasColumnName("direccion");
            entity.Property(e => e.DocumentoIdentidad)
                .HasMaxLength(30)
                .HasColumnName("documento_identidad");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(200)
                .HasColumnName("nombre_completo");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<Compra>(entity =>
        {
            entity.HasKey(e => e.IdCompra);

            entity.ToTable("Compra");

            entity.Property(e => e.IdCompra).HasColumnName("id_compra");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("pendiente")
                .HasColumnName("estado");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.NumeroFactura)
                .HasMaxLength(50)
                .HasColumnName("numero_factura");
            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("subtotal");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.Compras)
                .HasForeignKey(d => d.IdProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compra_Proveedor");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Compras)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compra_Usuario");
        });

        modelBuilder.Entity<DetalleCompra>(entity =>
        {
            entity.HasKey(e => e.IdDetalle);

            entity.ToTable("DetalleCompra");

            entity.Property(e => e.IdDetalle).HasColumnName("id_detalle");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.IdCompra).HasColumnName("id_compra");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.PrecioUnitario)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("precio_unitario");
            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("subtotal");

            entity.HasOne(d => d.IdCompraNavigation).WithMany(p => p.DetalleCompras)
                .HasForeignKey(d => d.IdCompra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleCompra_Compra");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetalleCompras)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleCompra_Producto");
        });

        modelBuilder.Entity<DetalleVentum>(entity =>
        {
            entity.HasKey(e => e.IdDetalle);

            entity.HasIndex(e => e.IdVenta, "IX_DetalleVenta_id_venta");

            entity.Property(e => e.IdDetalle).HasColumnName("id_detalle");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.IdVenta).HasColumnName("id_venta");
            entity.Property(e => e.PrecioUnitario)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("precio_unitario");
            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("subtotal");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleVenta_Producto");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleVenta_Venta");
        });

        modelBuilder.Entity<MovInventario>(entity =>
        {
            entity.HasKey(e => e.IdMovimiento);

            entity.ToTable("MovInventario");

            entity.HasIndex(e => e.FechaHora, "IX_MovInventario_fecha_hora");

            entity.HasIndex(e => e.IdProducto, "IX_MovInventario_id_producto");

            entity.Property(e => e.IdMovimiento).HasColumnName("id_movimiento");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.FechaHora)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha_hora");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Motivo).HasColumnName("motivo");
            entity.Property(e => e.TipoMovimiento)
                .HasMaxLength(20)
                .HasColumnName("tipo_movimiento");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.MovInventarios)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovInventario_Producto");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.MovInventarios)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovInventario_Usuario");
        });

        modelBuilder.Entity<NotaVentum>(entity =>
        {
            entity.HasKey(e => e.IdNota);

            entity.HasIndex(e => e.IdVenta, "UQ_NotaVenta_id_venta").IsUnique();

            entity.Property(e => e.IdNota).HasColumnName("id_nota");
            entity.Property(e => e.IdVenta).HasColumnName("id_venta");
            entity.Property(e => e.NumeroTicket)
                .HasMaxLength(30)
                .HasColumnName("numero_ticket");

            entity.HasOne(d => d.IdVentaNavigation).WithOne(p => p.NotaVentum)
                .HasForeignKey<NotaVentum>(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NotaVenta_Venta");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto);

            entity.ToTable("Producto");

            entity.HasIndex(e => e.CodigoBarras, "IX_Producto_codigo_barras").HasFilter("([codigo_barras] IS NOT NULL)");

            entity.HasIndex(e => e.IdCategoria, "IX_Producto_id_categoria");

            entity.HasIndex(e => e.CodigoBarras, "UQ_Producto_codigo_barras").IsUnique();

            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CodigoBarras)
                .HasMaxLength(50)
                .HasColumnName("codigo_barras");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.Nombre)
                .HasMaxLength(200)
                .HasColumnName("nombre");
            entity.Property(e => e.PrecioCompra)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("precio_compra");
            entity.Property(e => e.PrecioVenta)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("precio_venta");
            entity.Property(e => e.StockActual).HasColumnName("stock_actual");
            entity.Property(e => e.StockMinimo).HasColumnName("stock_minimo");
            entity.Property(e => e.UnidadMedida)
                .HasMaxLength(20)
                .HasDefaultValue("UND")
                .HasColumnName("unidad_medida");
        });

        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.IdProveedor);

            entity.ToTable("Proveedor");

            entity.HasIndex(e => e.NitRuc, "UQ_Proveedor_nit_ruc").IsUnique();

            entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor");
            entity.Property(e => e.Contacto)
                .HasMaxLength(100)
                .HasColumnName("contacto");
            entity.Property(e => e.Direccion)
                .HasMaxLength(300)
                .HasColumnName("direccion");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.NitRuc)
                .HasMaxLength(30)
                .HasColumnName("nit_ruc");
            // Mapeamos la propiedad Nombre de la entidad a la columna razon_social existente
            entity.Property(e => e.Nombre)
                .HasMaxLength(200)
                .HasColumnName("razon_social");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");
            entity.Property(e => e.TipoProducto)
                .HasMaxLength(100)
                .HasColumnName("tipo_producto");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario);

            entity.ToTable("Usuario");

            entity.HasIndex(e => e.NombreUsuario, "UQ_Usuario_nombre_usuario").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.ContraseñaHash)
                .HasMaxLength(256)
                .HasColumnName("contraseña_hash");
            entity.Property(e => e.NombreUsuario)
                .HasMaxLength(50)
                .HasColumnName("nombre_usuario");
            entity.Property(e => e.Rol)
                .HasMaxLength(20)
                .HasColumnName("rol");
        });

        modelBuilder.Entity<Ventum>(entity =>
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
            entity.HasKey(e => e.IdVenta);

            entity.HasIndex(e => e.FechaHora, "IX_Venta_fecha_hora");

            entity.HasIndex(e => e.IdVendedor, "IX_Venta_id_vendedor");

            entity.Property(e => e.IdVenta).HasColumnName("id_venta");
            entity.Property(e => e.Anulada).HasColumnName("anulada");
            entity.Property(e => e.Cambio)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("cambio");
            entity.Property(e => e.Descuentos)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("descuentos");
            entity.Property(e => e.FechaHora)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fecha_hora");
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.IdVendedor).HasColumnName("id_vendedor");
            entity.Property(e => e.Impuestos)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("impuestos");
            entity.Property(e => e.MetodoPago)
                .HasMaxLength(30)
                .HasColumnName("metodo_pago");
            entity.Property(e => e.MontoRecibido)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("monto_recibido");
            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("subtotal");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdCliente)
                .HasConstraintName("FK_Venta_Cliente");

            entity.HasOne(d => d.IdVendedorNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdVendedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Venta_Usuario");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
