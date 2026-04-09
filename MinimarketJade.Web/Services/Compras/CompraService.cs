using Microsoft.EntityFrameworkCore;
using MinimarketJade.Web.Data;
using MinimarketJade.Web.Data.Entities;

namespace MinimarketJade.Web.Services.Compras
{
    public class CompraService : ICompraService
    {
        private readonly AppDbContext _context;

        public CompraService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Compra>> ObtenerTodosAsync()
        {
            return await _context.Compras
                .Include(c => c.IdProveedorNavigation)
                .OrderByDescending(c => c.IdCompra)
                .ToListAsync();
        }

        public async Task<Compra?> ObtenerPorIdAsync(int id)
        {
            return await _context.Compras
                .Include(c => c.IdProveedorNavigation)
                .Include(c => c.DetalleCompras)
                    .ThenInclude(d => d.IdProductoNavigation)
                .FirstOrDefaultAsync(c => c.IdCompra == id);
        }

        public async Task<string> GenerarNumeroFacturaAsync()
        {
            var ahora = DateTime.UtcNow;
            string prefijo = $"FAC-{ahora:yyyy}-{ahora:MM}-";
            int count = await _context.Compras
                .Where(c => c.Fecha.Year == ahora.Year && c.Fecha.Month == ahora.Month)
                .CountAsync();
            int correlativo = count + 1;
            return prefijo + correlativo.ToString("D4");
        }

        public async Task CrearCompraAsync(Compra compra, List<DetalleCompra> detalles)
        {
            if (compra == null) throw new ArgumentNullException(nameof(compra));
            if (detalles == null || detalles.Count == 0) throw new ArgumentException("La compra debe contener al menos un detalle.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (string.IsNullOrWhiteSpace(compra.NumeroFactura))
                    compra.NumeroFactura = await GenerarNumeroFacturaAsync();

                var proveedor = await _context.Proveedors.FindAsync(compra.IdProveedor);
                if (proveedor == null)
                    throw new InvalidOperationException($"Proveedor con id {compra.IdProveedor} no encontrado.");

                if (!string.IsNullOrWhiteSpace(compra.NumeroFactura))
                {
                    bool existeFactura = await ExisteNumeroFacturaAsync(compra.NumeroFactura);
                    if (existeFactura)
                        throw new InvalidOperationException($"El número de factura '{compra.NumeroFactura}' ya existe.");
                }

                if (compra.IdUsuario == 0)
                    compra.IdUsuario = 1;

                compra.Subtotal = detalles.Sum(d => d.Subtotal);
                compra.Total = compra.Subtotal;
                compra.Fecha = DateOnly.FromDateTime(DateTime.UtcNow);

                _context.Compras.Add(compra);
                await _context.SaveChangesAsync();

                foreach (var det in detalles)
                {
                    det.IdCompra = compra.IdCompra;
                    _context.DetalleCompras.Add(det);

                    var producto = await _context.Productos.FindAsync(det.IdProducto);
                    if (producto == null)
                        throw new InvalidOperationException($"Producto con id {det.IdProducto} no encontrado.");

                    if (det.Cantidad <= 0) throw new InvalidOperationException("La cantidad debe ser mayor que cero.");
                    if (det.PrecioUnitario <= 0) throw new InvalidOperationException("El precio unitario debe ser mayor que cero.");

                    const int MAX_STOCK = 1_000_000;
                    if (producto.StockActual + det.Cantidad > MAX_STOCK)
                        throw new InvalidOperationException($"El stock resultante para el producto '{producto.Nombre}' excede el límite permitido ({MAX_STOCK}).");

                    producto.StockActual += det.Cantidad;
                    _context.Productos.Update(producto);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ExisteNumeroFacturaAsync(string numeroFactura)
        {
            if (string.IsNullOrWhiteSpace(numeroFactura)) return false;
            return await _context.Compras.AnyAsync(c => c.NumeroFactura == numeroFactura);
        }

        public async Task<List<PrecioPromedioProductoDto>> ObtenerPrecioCompraPromedioPorProductoAsync(int year, int month)
        {
            var desde = new DateOnly(year, month, 1);
            var hasta = desde.AddMonths(1);

            var query = from dc in _context.DetalleCompras
                        join c in _context.Compras on dc.IdCompra equals c.IdCompra
                        join p in _context.Productos on dc.IdProducto equals p.IdProducto
                        join cat in _context.Categoria on p.IdCategoria equals cat.IdCategoria
                        where (c.Estado == "recibida" || c.Estado == "pendiente")
                              && c.Fecha >= desde && c.Fecha < hasta
                        group new { dc, p, cat } by new
                        {
                            dc.IdProducto,
                            NombreProducto = p.Nombre,
                            p.PrecioCompra,
                            NombreCategoria = cat.Nombre
                        } into g
                        select new PrecioPromedioProductoDto
                        {
                            IdProducto = g.Key.IdProducto,
                            NombreProducto = g.Key.NombreProducto,
                            CategoriaProducto = g.Key.NombreCategoria,
                            TotalGastado = g.Sum(x => x.dc.Subtotal),
                            CantidadTotal = g.Sum(x => x.dc.Cantidad),
                            PrecioPromedio = g.Sum(x => x.dc.Subtotal) / (g.Sum(x => x.dc.Cantidad) == 0 ? 1 : g.Sum(x => x.dc.Cantidad)),
                            CostoEstandar = g.Key.PrecioCompra
                        };

            return await query.OrderByDescending(x => x.TotalGastado).ToListAsync();
        }

        public async Task<List<GastoProveedorDto>> ObtenerGastoPorProveedorAsync(DateTime? desde = null, DateTime? hasta = null)
        {
            var compras = _context.Compras.AsQueryable();

            if (desde.HasValue)
            {
                var d = DateOnly.FromDateTime(desde.Value);
                compras = compras.Where(c => c.Fecha >= d);
            }
            if (hasta.HasValue)
            {
                var h = DateOnly.FromDateTime(hasta.Value);
                compras = compras.Where(c => c.Fecha <= h);
            }

            var totalGeneral = await compras.SumAsync(c => (decimal?)c.Total) ?? 0m;

            var query = from c in compras
                        join p in _context.Proveedors on c.IdProveedor equals p.IdProveedor
                        group c by new { p.IdProveedor, p.RazonSocial } into g
                        select new GastoProveedorDto
                        {
                            IdProveedor = g.Key.IdProveedor,
                            RazonSocial = g.Key.RazonSocial,
                            TotalGastado = g.Sum(x => x.Total),
                            Porcentaje = totalGeneral == 0 ? 0 : (g.Sum(x => x.Total) / totalGeneral * 100)
                        };

            return await query.OrderByDescending(x => x.TotalGastado).ToListAsync();
        }

        public async Task<decimal> ObtenerConcentracionTop3ProveedoresAsync(DateTime? desde = null, DateTime? hasta = null)
        {
            var listado = await ObtenerGastoPorProveedorAsync(desde, hasta);
            var top3 = listado.Take(3).Sum(x => x.TotalGastado);
            var total = listado.Sum(x => x.TotalGastado);
            if (total == 0) return 0m;
            return top3 / total * 100m;
        }

        public async Task<List<CategoriaPrecioDto>> ObtenerPrecioEstandarPorCategoriaAsync()
        {
            var query = from p in _context.Productos
                        join c in _context.Categoria on p.IdCategoria equals c.IdCategoria
                        where p.Activo
                        group p by new { c.IdCategoria, c.Nombre } into g
                        select new CategoriaPrecioDto
                        {
                            IdCategoria = g.Key.IdCategoria,
                            NombreCategoria = g.Key.Nombre,
                            PrecioEstandar = g.Average(x => x.PrecioCompra)
                        };

            return await query.OrderBy(x => x.NombreCategoria).ToListAsync();
        }
    }
}