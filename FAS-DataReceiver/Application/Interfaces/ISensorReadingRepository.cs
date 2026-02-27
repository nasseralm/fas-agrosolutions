using Agro.DataReceiver.Domain.Entities;

namespace Agro.DataReceiver.Application.Interfaces;

public interface ISensorReadingRepository
{
    Task InsertAsync(SensorReading reading, CancellationToken cancellationToken = default);

    /// <summary>
    /// Última leitura por talhão (para dashboard exibir umidade/status).
    /// </summary>
    Task<IReadOnlyList<LatestReadingByTalhao>> GetLatestByTalhaoIdsAsync(
        IEnumerable<string> talhaoIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Média de umidade por hora nas últimas 24h para os talhões informados (gráfico histórico).
    /// </summary>
    Task<IReadOnlyList<HourlyUmidade>> GetHourlyAverageUmidadeLast24hAsync(
        IEnumerable<string> talhaoIds,
        CancellationToken cancellationToken = default);
}

public sealed class LatestReadingByTalhao
{
    public string TalhaoId { get; init; } = string.Empty;
    public double? UmidadeSoloPct { get; init; }
    public DateTime Timestamp { get; init; }
}

/// <summary>
/// Média de umidade em uma hora (0-23) para o gráfico de histórico 24h.
/// </summary>
public sealed class HourlyUmidade
{
    /// <summary>Hora no dia (0-23), formatada como "00" a "23".</summary>
    public string Hour { get; init; } = string.Empty;
    /// <summary>Média da umidade do solo (%) naquela hora.</summary>
    public double UmidadePct { get; init; }
}
