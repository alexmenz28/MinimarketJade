using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
                .OrderBy(p => p.RazonSocial)
                .ToListAsync();
        }

        public async Task<Proveedor?> ObtenerPorIdAsync(int id)
        {
            return await _context.Proveedors.FindAsync(id);
        }

        public async Task CrearAsync(Proveedor proveedor)
        {
            // Validar modelo (DataAnnotations)
            ValidateProveedor(proveedor);

            // Validar unicidad NIT/RUC
            if (await ExisteNitAsync(proveedor.NitRuc))
            {
                throw new InvalidOperationException("El NIT/RUC ya está registrado.");
            }

            _context.Proveedors.Add(proveedor);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Proveedor proveedor)
        {
            // Validar modelo (DataAnnotations)
            ValidateProveedor(proveedor);

            // Validar unicidad NIT/RUC (excluyendo el propio registro)
            if (await ExisteNitAsync(proveedor.NitRuc, proveedor.IdProveedor))
            {
                throw new InvalidOperationException("El NIT/RUC ya está registrado en otro proveedor.");
            }

            // Buscar la entidad existente y actualizar campos individualmente
            var existente = await _context.Proveedors
                .FirstOrDefaultAsync(p => p.IdProveedor == proveedor.IdProveedor);

            if (existente != null)
            {
                existente.RazonSocial = proveedor.RazonSocial;
                existente.NitRuc = proveedor.NitRuc;
                existente.Telefono = proveedor.Telefono;
                existente.Email = proveedor.Email;
                existente.Direccion = proveedor.Direccion;
                existente.Contacto = proveedor.Contacto;
                // Si la propiedad Activo está mapeada en BD y se usa, también se puede actualizar:
                existente.Activo = proveedor.Activo;

                await _context.SaveChangesAsync();
            }
        }

        private void ValidateProveedor(Proveedor proveedor)
        {
            var context = new ValidationContext(proveedor);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(proveedor, context, results, validateAllProperties: true);
            if (!isValid)
            {
                var messages = results.Select(r => r.ErrorMessage).Where(m => !string.IsNullOrWhiteSpace(m));
                throw new ValidationException(string.Join("; ", messages));
            }
        }

        public async Task EliminarAsync(int id)
        {
            var p = await _context.Proveedors.FindAsync(id);
            if (p != null)
            {
                // Antes de eliminar físicamente: tratar como inhabilitar por defecto
                p.Activo = false;
                _context.Proveedors.Update(p);
                await _context.SaveChangesAsync();
            }
        }

        public async Task InhabilitarAsync(int id)
        {
            var p = await _context.Proveedors.FindAsync(id);
            if (p != null)
            {
                p.Activo = false;
                _context.Proveedors.Update(p);
                await _context.SaveChangesAsync();
            }
        }

        public async Task HabilitarAsync(int id)
        {
            var p = await _context.Proveedors.FindAsync(id);
            if (p != null)
            {
                p.Activo = true;
                _context.Proveedors.Update(p);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExisteNitAsync(string nitRuc, int? excluirId = null)
        {
            if (excluirId.HasValue)
            {
                return await _context.Proveedors.AnyAsync(p => p.NitRuc == nitRuc && p.IdProveedor != excluirId.Value);
            }

            return await _context.Proveedors.AnyAsync(p => p.NitRuc == nitRuc); 
        }
    }
}
