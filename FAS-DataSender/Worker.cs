using System.Net.Http.Json;
using Agro.SensorSimulator.Worker.Models;
using Microsoft.Extensions.Options;

namespace Agro.SensorSimulator.Worker;

public sealed class Worker : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<Worker> _logger;
    private readonly SensorSimulatorOptions _opt;

    private readonly Random _rng = new();
    private double _umidade;
    private double _temp;
    private double _ph;
    private double _ec;
    private int _bateria;
    private long _seq;

    public Worker(
        IHttpClientFactory httpClientFactory,
        IOptions<SensorSimulatorOptions> options,
        ILogger<Worker> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _opt = options.Value;

        _umidade = _opt.Initial.UmidadeSoloPct;
        _temp = _opt.Initial.TemperaturaSoloC;
        _ph = _opt.Initial.Ph;
        _ec = _opt.Initial.EcDsM;
        _bateria = _opt.Initial.BateriaPct;
        _seq = 0;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Sensor Simulator iniciado. Endpoint={Endpoint} Interval={Interval}s Count={Count}",
            _opt.Endpoint, _opt.IntervalSeconds, _opt.Count);

        var sent = 0;
        var delay = TimeSpan.FromSeconds(Math.Max(1, _opt.IntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            var payload = BuildNextPayload();

            try
            {
                var client = _httpClientFactory.CreateClient("ingest");

                using var resp = await client.PostAsJsonAsync(_opt.Endpoint, payload, cancellationToken: stoppingToken);
                resp.EnsureSuccessStatusCode();

                _logger.LogInformation(
                    "Enviado OK | seq={Seq} device={DeviceId} ts={Timestamp:o} umid={Umidade}% temp={Temp}C prec={Prec}mm",
                    payload.Seq, payload.DeviceId, payload.Timestamp,
                    payload.Leituras.UmidadeSoloPct, payload.Leituras.TemperaturaSoloC, payload.Leituras.PrecipitacaoMm
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao enviar leitura | seq={Seq} endpoint={Endpoint}", payload.Seq, _opt.Endpoint);
            }

            sent++;
            if (_opt.Count > 0 && sent >= _opt.Count)
            {
                _logger.LogInformation("Count atingido ({Count}). Encerrando.", _opt.Count);
                return;
            }

            await Task.Delay(delay, stoppingToken);
        }
    }

    private SensorReadingPayload BuildNextPayload()
    {
        StepState();

        var leituras = new Leituras(
            UmidadeSoloPct: Math.Round(_umidade, 1),
            TemperaturaSoloC: Math.Round(_temp, 1),
            PrecipitacaoMm: GeneratePrecipitationMm(),
            Ph: Math.Round(_ph, 2),
            EcDsM: Math.Round(_ec, 2)
        );

        var payload = new SensorReadingPayload(
            DeviceId: _opt.DeviceId,
            Timestamp: DateTimeOffset.UtcNow,
            Geo: new Geo(_opt.Lat, _opt.Lon),
            Leituras: leituras,
            BateriaPct: _bateria,
            RssiDbm: _rng.Next(-90, -55),
            Seq: _seq
        );

        return payload;
    }

    private void StepState()
    {
        var r = _opt.Ranges;

        // simula drift e ruido
        _umidade = Clamp(_umidade + NextDouble(-0.6, 0.6), r.UmidadeMin, r.UmidadeMax);
        _temp = Clamp(_temp + NextDouble(-0.3, 0.3), r.TempMin, r.TempMax);
        _ph = Clamp(_ph + NextDouble(-0.05, 0.05), r.PhMin, r.PhMax);
        _ec = Clamp(_ec + NextDouble(-0.03, 0.03), r.EcMin, r.EcMax);

        // Simula queda de bateria
        if (_bateria > 0 && _rng.NextDouble() < 0.25) _bateria--;

        _seq++;
    }

    private double GeneratePrecipitationMm()
    {
        // 90% das vezes é 0; 10% evento leve/moderado
        if (_rng.NextDouble() < 0.90) return 0.0;
        return Math.Round(NextDouble(0.2, 8.0), 1);
    }

    private double NextDouble(double min, double max) => min + _rng.NextDouble() * (max - min);

    private static double Clamp(double v, double lo, double hi) => Math.Max(lo, Math.Min(hi, v));
}
