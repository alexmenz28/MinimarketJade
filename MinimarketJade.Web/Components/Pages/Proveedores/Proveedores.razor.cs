using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MinimarketJade.Web.Data.Entities;
using MinimarketJade.Web.Services.Proveedores;

namespace MinimarketJade.Web.Components.Pages.Proveedores;

public partial class Proveedores : ComponentBase
{
    // Inyectados via @inject en el .razor

    private List<Proveedor>? proveedores;
    private List<Proveedor>? proveedoresFrecuentes;
    private List<Proveedor>? proveedoresOcasionales;
    private Proveedor proveedorActual = new();
    private bool mostrarModal = false;
    private bool esEdicion = false;
    private string? mensajeError;
    private string textoBusqueda = string.Empty;
    private string selectedView = string.Empty; // "Frecuentes" | "Ocasionales" | "" (todos)

    private IEnumerable<Proveedor> proveedoresFiltrados =>
        (proveedores ?? Enumerable.Empty<Proveedor>())
            .Where(p => string.IsNullOrWhiteSpace(textoBusqueda)
                ? true
                : (p.RazonSocial != null && p.RazonSocial.Contains(textoBusqueda, StringComparison.OrdinalIgnoreCase))
                  || (p.NitRuc != null && p.NitRuc.Contains(textoBusqueda, StringComparison.OrdinalIgnoreCase))
                  || (p.Contacto != null && p.Contacto.Contains(textoBusqueda, StringComparison.OrdinalIgnoreCase)));

    protected override async Task OnInitializedAsync()
    {
        await CargarDatos();
    }

    private async Task CargarDatos()
    {
        proveedores = await ProveedorService.ObtenerTodosAsync();
        // Cargar clasificación por frecuencia (ultimos 6 meses, umbral 3)
        var clas = await ProveedorService.ObtenerClasificacionFrecuenciaAsync();
        proveedoresFrecuentes = clas.Frecuentes;
        proveedoresOcasionales = clas.Ocasionales;
    }

    private void ToggleFrecuentes()
    {
        selectedView = selectedView == "Frecuentes" ? string.Empty : "Frecuentes";
    }

    private void ToggleOcasionales()
    {
        selectedView = selectedView == "Ocasionales" ? string.Empty : "Ocasionales";
    }

    private void AbrirModal()
    {
        mensajeError = null;
        esEdicion = false;
        proveedorActual = new Proveedor();
        mostrarModal = true;
    }

    private void PrepararEdicion(Proveedor p)
    {
        mensajeError = null;
        esEdicion = true;
        proveedorActual = new Proveedor
        {
            IdProveedor = p.IdProveedor,
            RazonSocial = p.RazonSocial,
            NitRuc = p.NitRuc,
            Telefono = p.Telefono,
            Email = p.Email,
            Direccion = p.Direccion,
            Contacto = p.Contacto
        };
        mostrarModal = true;
    }

    private void CerrarModal() => mostrarModal = false;

    private async Task GuardarProveedor()
    {
        mensajeError = null;

        try
        {
            // Validar NIT/RUC único
            bool existe;
            if (esEdicion)
            {
                existe = await ProveedorService.ExisteNitAsync(proveedorActual.NitRuc, proveedorActual.IdProveedor);
            }
            else
            {
                existe = await ProveedorService.ExisteNitAsync(proveedorActual.NitRuc);
            }

            if (existe)
            {
                mensajeError = "El NIT/RUC ya está registrado.";
                return;
            }

            if (esEdicion)
            {
                await ProveedorService.ActualizarAsync(proveedorActual);
            }
            else
            {
                await ProveedorService.CrearAsync(proveedorActual);
            }

            mostrarModal = false;
            await CargarDatos();
        }
        catch (Exception ex)
        {
            // Mostrar mensaje amigable y log básico
            mensajeError = "Ocurrió un error al guardar el proveedor. " + ex.Message;
        }
    }

    private async Task ConfirmarEliminar(int id)
    {
        // Mantener compatibilidad: el método ahora realiza inhabilitación lógica
        bool ok = await JS.InvokeAsync<bool>("confirm", "¿Desea inhabilitar este proveedor?");
        if (!ok) return;

        try
        {
            await ProveedorService.InhabilitarAsync(id);
            await CargarDatos();
        }
        catch (DbUpdateException)
        {
            await JS.InvokeVoidAsync("alert", "No se puede inhabilitar el proveedor porque tiene registros relacionados.");
        }
        catch (Exception ex)
        {
            await JS.InvokeVoidAsync("alert", "Ocurrió un error: " + ex.Message);
        }
    }

    private async Task ConfirmarInhabilitar(int id)
    {
        bool ok = await JS.InvokeAsync<bool>("confirm", "¿Desea inhabilitar este proveedor?");
        if (!ok) return;

        try
        {
            await ProveedorService.InhabilitarAsync(id);
            await CargarDatos();
        }
        catch (DbUpdateException)
        {
            await JS.InvokeVoidAsync("alert", "No se puede inhabilitar el proveedor porque tiene registros relacionados.");
        }
        catch (Exception ex)
        {
            await JS.InvokeVoidAsync("alert", "Ocurrió un error: " + ex.Message);
        }
    }

    private async Task ConfirmarHabilitar(int id)
    {
        bool ok = await JS.InvokeAsync<bool>("confirm", "¿Desea habilitar este proveedor?");
        if (!ok) return;

        try
        {
            await ProveedorService.HabilitarAsync(id);
            await CargarDatos();
        }
        catch (Exception ex)
        {
            await JS.InvokeVoidAsync("alert", "Ocurrió un error: " + ex.Message);
        }
    }
}
