using FAS.API.Models;
using FAS.Application.DTOs;
using FAS.Domain.Pagination;
using FAS.Domain.Notifications;

namespace FAS.Application.Interfaces
{
    public interface ITalhaoService
    {
        Task<DomainNotificationsResult<TalhaoViewModel>> Incluir(int producerId, bool isAdmin, TalhaoDTO dto);
        Task<DomainNotificationsResult<TalhaoViewModel>> Alterar(int producerId, bool isAdmin, TalhaoDTO dto);
        Task<DomainNotificationsResult<TalhaoViewModel>> Excluir(int producerId, bool isAdmin, int id);
        Task<DomainNotificationsResult<TalhaoViewModel>> Selecionar(int id);
        Task<DomainNotificationsResult<PagedList<TalhaoViewModel>>> ListarPorPropriedade(int producerId, bool isAdmin, int propriedadeId, int pageNumber, int pageSize);
    }
}
