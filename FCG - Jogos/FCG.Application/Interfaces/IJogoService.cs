using FCG.API.Models;
using FCG.Application.DTOs;
using FCG.Domain.Notifications;

namespace FCG.Application.Interfaces
{
    public interface IJogoService
    {
        Task<DomainNotificationsResult<JogoViewModel>> Incluir(JogoDTO jogoDTO);
        Task<DomainNotificationsResult<JogoViewModel>> Alterar(JogoDTO jogoDTO);
        Task<DomainNotificationsResult<JogoViewModel>> Excluir(int id);
        Task<DomainNotificationsResult<JogoViewModel>> Selecionar(int id);
        Task<DomainNotificationsResult<JogoViewModel>> SelecionarPorNome(string nome);
        Task<DomainNotificationsResult<IEnumerable<JogoViewModel>>> SelecionarTodos();
    }
}
