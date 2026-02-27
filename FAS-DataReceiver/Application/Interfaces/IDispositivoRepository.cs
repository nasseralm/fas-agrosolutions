namespace Agro.DataReceiver.Application.Interfaces;

public interface IDispositivoRepository
{
    Task<string?> GetTalhaoIdByDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mapeamento talhão → sensor (para exibir na tela de propriedades/talhões).
    /// </summary>
    Task<IReadOnlyList<DeviceMappingEntry>> GetMappingAsync(CancellationToken cancellationToken = default);
}

public sealed class DeviceMappingEntry
{
    public string TalhaoId { get; init; } = string.Empty;
    public string DeviceId { get; init; } = string.Empty;
}
