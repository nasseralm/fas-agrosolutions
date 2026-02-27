using System.Text.Json.Serialization;

namespace Agro.DataReceiver.Application.DTOs;

public sealed class SensorReadingReceivedEvent
{
    [JsonPropertyName("eventId")]
    public string EventId { get; set; } = string.Empty;

    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("talhaoId")]
    public string TalhaoId { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("resolvedBy")]
    public string ResolvedBy { get; set; } = string.Empty;

    [JsonPropertyName("summary")]
    public ReadingSummary Summary { get; set; } = new();
}

public sealed class ReadingSummary
{
    [JsonPropertyName("umidadeSoloPct")]
    public double? UmidadeSoloPct { get; set; }

    [JsonPropertyName("temperaturaSoloC")]
    public double? TemperaturaSoloC { get; set; }

    [JsonPropertyName("precipitacaoMm")]
    public double? PrecipitacaoMm { get; set; }
}
