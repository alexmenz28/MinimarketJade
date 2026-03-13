using Microsoft.EntityFrameworkCore;
using MinimarketJade.Web.Data;
using MinimarketJade.Web.Data.Entities;

namespace MinimarketJade.Web.Services.Clientes
{
    public class ClienteService : IClienteService
    {
        private readonly AppDbContext _context;

        public ClienteService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Cliente>> ObtenerTodosAsync()
        {
            return await _context.Clientes
                .Include(c => c.Venta)
                .OrderBy(c => c.IdCliente)
                .ToListAsync();
        }

        public async Task<Cliente?> ObtenerPorIdAsync(int id)
        {
            return await _context.Clientes.FindAsync(id);
        }

        public async Task<bool> CrearAsync(Cliente cliente)
        {
            try
            {
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                // Error por restricción UNIQUE
                if (ex.InnerException != null &&
                    ex.InnerException.Message.Contains("UQ_Cliente_documento"))
                {
                    return false;
                }

                throw; // si es otro error, lo lanza normal
            }
        }

        public async Task<bool> ActualizarAsync(Cliente cliente)
        {
            try
            {
                var clienteExistente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.IdCliente == cliente.IdCliente);

                if (clienteExistente != null)
                {
                    clienteExistente.DocumentoIdentidad = cliente.DocumentoIdentidad;
                    clienteExistente.NombreCompleto = cliente.NombreCompleto;
                    clienteExistente.Telefono = cliente.Telefono;
                    clienteExistente.Email = cliente.Email;
                    clienteExistente.Direccion = cliente.Direccion;

                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException != null &&
                    ex.InnerException.Message.Contains("UQ_Cliente_documento"))
                {
                    return false;
                }

                throw;
            }
        }

        public async Task EliminarAsync(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();
            }
        }

        public async Task CambiarEstadoAsync(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente != null)
            {
                cliente.Activo = !cliente.Activo;
                await _context.SaveChangesAsync();
            }
        }
    }
}
