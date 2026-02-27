namespace Agro.DataReceiver.Application.Interfaces;

public interface IDeduplicationService
{
    Task<bool> IsDuplicateAsync(string eventId, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(string eventId, CancellationToken cancellationToken = default);
}
