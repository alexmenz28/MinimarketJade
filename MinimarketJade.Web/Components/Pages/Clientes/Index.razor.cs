using Microsoft.AspNetCore.Components;
using MinimarketJade.Web.Data.Entities;
using MinimarketJade.Web.Services;
using MinimarketJade.Web.Services.Clientes;

namespace MinimarketJade.Web.Components.Pages.Clientes
{
    public partial class Index
    {
        [Inject] private IClienteService ClienteService { get; set; }
        [Inject] private IVentaService VentaService { get; set; }

        private List<Cliente>? clientes;
        private Cliente clienteActual = new();
        private bool mostrarFormulario = false;
        private string? mensajeError;
        private string textoBusqueda = "";
        private List<Ventum>? historialVentas;
        private int? clienteSeleccionadoId;
        private string tipoBusqueda = "Id";

        private IEnumerable<Cliente> clientesFiltrados
        {
            get
            {
                if (clientes == null)
                    return new List<Cliente>();

                IEnumerable<Cliente> resultado = clientes;

                switch (tipoBusqueda)
                {
                    case "Id":
                        resultado = clientes.Where(c =>
                            string.IsNullOrWhiteSpace(textoBusqueda) ||
                            c.IdCliente.ToString().Contains(textoBusqueda));
                        break;

                    case "Documento":
                        resultado = clientes.Where(c =>
                            string.IsNullOrWhiteSpace(textoBusqueda) ||
                            c.DocumentoIdentidad.Contains(textoBusqueda, StringComparison.OrdinalIgnoreCase));
                        break;

                    case "Nombre":
                        resultado = clientes.Where(c =>
                            string.IsNullOrWhiteSpace(textoBusqueda) ||
                            c.NombreCompleto.Contains(textoBusqueda, StringComparison.OrdinalIgnoreCase));
                        break;

                    case "Recurrentes":
                        resultado = clientes.Where(c =>
                            c.Venta.Count(v => !v.Anulada) >= 5);
                        break;

                    case "NoRecurrentes":
                        resultado = clientes.Where(c =>
                            c.Venta.Count(v => !v.Anulada) < 5);
                        break;
                }

                return resultado;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await CargarClientes();
        }

        private async Task CargarClientes()
        {
            clientes = await ClienteService.ObtenerTodosAsync();
        }

        private void NuevoCliente()
        {
            clienteActual = new Cliente();
            mostrarFormulario = true;
        }

        private void Editar(Cliente cliente)
        {
            clienteActual = new Cliente
            {
                IdCliente = cliente.IdCliente,
                DocumentoIdentidad = cliente.DocumentoIdentidad,
                NombreCompleto = cliente.NombreCompleto,
                Telefono = cliente.Telefono,
                Email = cliente.Email,
                Direccion = cliente.Direccion
            };

            mostrarFormulario = true;
        }

        private async Task Guardar()
        {
            mensajeError = null;

            if (clienteActual.IdCliente == 0)
            {
                var creado = await ClienteService.CrearAsync(clienteActual);

                if (!creado)
                {
                    mensajeError = "Este cliente ya existe con exactamente los mismos datos o Documento.";
                    return;
                }
            }
            else
            {
                var actualizado = await ClienteService.ActualizarAsync(clienteActual);

                if (!actualizado)
                {
                    mensajeError = "Ya existe un cliente con ese documento.";
                    return;
                }
            }

            mostrarFormulario = false;
            await CargarClientes();
        }

        private async Task CambiarEstado(int id)
        {
            historialVentas = null;
            clienteSeleccionadoId = null;

            await ClienteService.CambiarEstadoAsync(id);

            await Task.Delay(50);

            await CargarClientes();
        }

        private async Task VerHistorial(int idCliente)
        {
            if (clienteSeleccionadoId == idCliente)
            {
                clienteSeleccionadoId = null;
                historialVentas = null;
                return;
            }

            clienteSeleccionadoId = idCliente;
            historialVentas = await VentaService.GetByClienteAsync(idCliente);
        }

        private void Cancelar()
        {
            mostrarFormulario = false;
        }
    }
}
