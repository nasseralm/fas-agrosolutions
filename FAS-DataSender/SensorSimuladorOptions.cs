namespace Agro.SensorSimulator.Worker;

public sealed class SensorSimulatorOptions
{
    public const string SectionName = "SensorSimulator";

    public string Endpoint { get; set; } = default!;
    public string ApiKey { get; set; } = default!;
    public int IntervalSeconds { get; set; } = 30;

    /// Numero de disparos, 0 = infinito
    public int Count { get; set; } = 0;

    public string DeviceId { get; set; } = "SENS-001";

    public double Lat { get; set; } = -23.532;
    public double Lon { get; set; } = -46.791;

    public InitialValues Initial { get; set; } = new();
    public RangesValues Ranges { get; set; } = new();

    public sealed class InitialValues
    {
        public double UmidadeSoloPct { get; set; } = 55.0;
        public double TemperaturaSoloC { get; set; } = 26.0;
        public double Ph { get; set; } = 6.2;
        public double EcDsM { get; set; } = 0.35;
        public int BateriaPct { get; set; } = 100;
    }

    public sealed class RangesValues
    {
        public double UmidadeMin { get; set; } = 45.0;
        public double UmidadeMax { get; set; } = 70.0;

        public double TempMin { get; set; } = 5.0;
        public double TempMax { get; set; } = 45.0;

        public double PhMin { get; set; } = 4.5;
        public double PhMax { get; set; } = 8.5;

        public double EcMin { get; set; } = 0.05;
        public double EcMax { get; set; } = 3.0;
    }
}
