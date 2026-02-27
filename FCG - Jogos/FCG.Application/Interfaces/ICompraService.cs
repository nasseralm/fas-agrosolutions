using FCG.Application.DTOs;
using FCG.Domain.Notifications;

namespace FCG.Application.Interfaces
{
    public interface ICompraService
    {
        Task<DomainNotificationsResult<bool>> EfetuarCompra(CompraDTO compraDTO);
    }
}