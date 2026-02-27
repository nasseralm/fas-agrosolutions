using FAS.Domain.EventSourcing;

namespace FAS.Domain.EventSourcing.Events
{
    public class UsuarioExcluidoEvent : Event
    {
        public UsuarioExcluidoEvent(int usuarioId)
            : base(usuarioId)
        {
        }
    }
}
