using FAS.API.Models;
using FAS.Application.DTOs;
using FAS.Domain.Notifications;

namespace FAS.Application.Interfaces
{
    public interface IUsuarioService
    {
        Task<DomainNotificationsResult<UsuarioViewModel>> Incluir(UsuarioDTO usuarioDTO);
        Task<DomainNotificationsResult<UsuarioViewModel>> Alterar(UsuarioDTO usuarioDTO);
        Task<DomainNotificationsResult<UsuarioViewModel>> Excluir(int id);
        Task<DomainNotificationsResult<UsuarioViewModel>> Selecionar(int id);
        Task<UsuarioViewModel> SelecionarPorEmail(string email);
        Task<DomainNotificationsResult<UsuarioViewModel>> SelecionarPorNomeEmail(string email, string nome);
    }
}
