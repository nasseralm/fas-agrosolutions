using System.Threading.Tasks;

namespace FAS.Domain.EventSourcing
{
    public interface IEventPublisher
    {
        Task PublishAsync(Event @event);
    }
}
