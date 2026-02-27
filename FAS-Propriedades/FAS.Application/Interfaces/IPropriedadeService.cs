using FAS.API.Models;
using FAS.Application.DTOs;
using FAS.Domain.Pagination;
using FAS.Domain.Notifications;

namespace FAS.Application.Interfaces
{
    public interface IPropriedadeService
    {
        Task<DomainNotificationsResult<PropriedadeViewModel>> Incluir(int producerId, PropriedadeDTO dto);
        Task<DomainNotificationsResult<PropriedadeViewModel>> Alterar(int producerId, bool isAdmin, PropriedadeDTO dto);
        Task<DomainNotificationsResult<PropriedadeViewModel>> Excluir(int producerId, bool isAdmin, int id);
        Task<DomainNotificationsResult<PropriedadeViewModel>> Selecionar(int id);
        Task<DomainNotificationsResult<PagedList<PropriedadeViewModel>>> Listar(int producerId, int pageNumber, int pageSize);
    }
}
