using Agro.DataReceiver.Application.DTOs;

namespace Agro.DataReceiver.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync(SensorReadingReceivedEvent @event, CancellationToken cancellationToken = default);
}
