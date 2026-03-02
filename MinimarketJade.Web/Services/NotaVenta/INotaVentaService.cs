using Microsoft.EntityFrameworkCore;
using MinimarketJade.Web.Data.Entities;

namespace MinimarketJade.Web.Services;

public interface INotaVentaService
{
    Task CrearAsync(NotaVentum nota);
    Task<NotaVentum?> GetByVentaAsync(int idVenta);

    Task<List<NotaVentum>> GetAllAsync();

}