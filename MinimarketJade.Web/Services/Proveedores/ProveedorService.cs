using Microsoft.EntityFrameworkCore;
using MinimarketJade.Web.Data;
using MinimarketJade.Web.Data.Entities;

namespace MinimarketJade.Web.Services.Proveedores
{
    public class ProveedorService : IProveedorService
    {
        private readonly AppDbContext _context;

        public ProveedorService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Proveedor>> ObtenerTodosAsync()
        {
            return await _context.Proveedors
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<Proveedor?> ObtenerPorIdAsync(int id)
        {
            return await _context.Proveedors.FindAsync(id);
        }

        public async Task CrearAsync(Proveedor proveedor)
        {
            _context.Proveedors.Add(proveedor);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Proveedor proveedor)
        {
            _context.Proveedors.Update(proveedor);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var p = await _context.Proveedors.FindAsync(id);
            if (p != null)
            {
                _context.Proveedors.Remove(p);
                await _context.SaveChangesAsync();
            }
        }
    }
}
