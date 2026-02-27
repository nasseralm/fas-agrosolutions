namespace Agro.DataReceiver.Domain.Entities;

public sealed class SensorReading
{
    public string EventId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string TalhaoId { get; set; } = string.Empty;
    public string ResolvedBy { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public GeoLocation Geo { get; set; } = new();
    public SensorLeituras Leituras { get; set; } = new();
    public double? BateriaPct { get; set; }
    public int? RssiDbm { get; set; }
    public int? Seq { get; set; }
    public DateTime IngestedAtUtc { get; set; }
}

public sealed class GeoLocation
{
    public double Lat { get; set; }
    public double Lon { get; set; }
}

public sealed class SensorLeituras
{
    public double? UmidadeSoloPct { get; set; }
    public double? TemperaturaSoloC { get; set; }
    public double? PrecipitacaoMm { get; set; }
    public double? Ph { get; set; }
    public double? EcDsM { get; set; }
}
