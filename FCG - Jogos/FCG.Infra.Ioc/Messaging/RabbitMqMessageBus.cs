using FCG.Application.Interfaces.Messaging;
using MassTransit;

namespace FCG.Infra.IoC.Messaging;

public class RabbitMqMessageBus : IMessageBus
{
    private readonly IPublishEndpoint _publishEndpoint;

    public RabbitMqMessageBus(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class
        => _publishEndpoint.Publish(message, ct);
}
