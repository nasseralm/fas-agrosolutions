namespace Agro.DataReceiver.Application.Interfaces;

public interface IGeoFenceService
{
    Task<string?> FindTalhaoByLocationAsync(double lat, double lon, CancellationToken cancellationToken = default);
    Task RefreshCacheAsync(CancellationToken cancellationToken = default);
}
