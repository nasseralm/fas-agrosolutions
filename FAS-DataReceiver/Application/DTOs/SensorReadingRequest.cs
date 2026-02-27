using System.Text.Json.Serialization;

namespace Agro.DataReceiver.Application.DTOs;

public sealed class SensorReadingRequest
{
    [JsonPropertyName("deviceId")]
    public string? DeviceId { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    [JsonPropertyName("geo")]
    public GeoRequest? Geo { get; set; }

    [JsonPropertyName("leituras")]
    public LeiturasRequest? Leituras { get; set; }

    [JsonPropertyName("bateriaPct")]
    public double? BateriaPct { get; set; }

    [JsonPropertyName("rssiDbm")]
    public int? RssiDbm { get; set; }

    [JsonPropertyName("seq")]
    public int? Seq { get; set; }
}

public sealed class GeoRequest
{
    [JsonPropertyName("lat")]
    public double? Lat { get; set; }

    [JsonPropertyName("lon")]
    public double? Lon { get; set; }
}

public sealed class LeiturasRequest
{
    [JsonPropertyName("umidadeSoloPct")]
    public double? UmidadeSoloPct { get; set; }

    [JsonPropertyName("temperaturaSoloC")]
    public double? TemperaturaSoloC { get; set; }

    [JsonPropertyName("precipitacaoMm")]
    public double? PrecipitacaoMm { get; set; }

    [JsonPropertyName("ph")]
    public double? Ph { get; set; }

    [JsonPropertyName("ecDsM")]
    public double? EcDsM { get; set; }
}
