
using FAS.API.Models;
using FAS.Domain.Notifications;

namespace FAS.Domain.Account
{
    public interface IAuthenticate
    {
        Task<DomainNotificationsResult<UserTokenViewModel>> Login(LoginDTO loginDTO);
        public Task<DomainNotificationsResult<string>> RecuperarSenha(string email);
        Task<bool> Autenticar(string email, string senha);
        Task<string> GerarToken(int id, string email);
    }
}
