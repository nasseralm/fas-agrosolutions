namespace Agro.DataReceiver.Application.Interfaces;

public interface IDeviceCacheService
{
    Task<string?> GetTalhaoIdAsync(string deviceId, CancellationToken cancellationToken = default);
    Task SetTalhaoIdAsync(string deviceId, string talhaoId, CancellationToken cancellationToken = default);
}
