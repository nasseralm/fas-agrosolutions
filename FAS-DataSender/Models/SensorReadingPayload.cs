namespace Agro.SensorSimulator.Worker.Models;

public sealed record SensorReadingPayload(
    string DeviceId,
    DateTimeOffset Timestamp,
    Geo Geo,
    Leituras Leituras,
    int BateriaPct,
    int RssiDbm,
    long Seq
);

public sealed record Geo(double Lat, double Lon);

public sealed record Leituras(
    double UmidadeSoloPct,
    double TemperaturaSoloC,
    double PrecipitacaoMm,
    double Ph,
    double EcDsM
);
