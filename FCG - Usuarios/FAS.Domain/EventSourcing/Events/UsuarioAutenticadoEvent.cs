using FAS.Domain.EventSourcing;

namespace FAS.Domain.EventSourcing.Events
{
    public class UsuarioAutenticadoEvent : Event
    {
        public string Email { get; }

        public UsuarioAutenticadoEvent(int usuarioId, string email)
            : base(usuarioId)
        {
            Email = email;
        }
    }
}
