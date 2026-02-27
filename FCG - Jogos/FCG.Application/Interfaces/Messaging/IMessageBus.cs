namespace FCG.Application.Interfaces.Messaging
{
    public interface IMessageBus
    {
        Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class;
    }
}
