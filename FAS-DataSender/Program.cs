using Agro.SensorSimulator.Worker;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddOptions<SensorSimulatorOptions>()
    .Bind(builder.Configuration.GetSection(SensorSimulatorOptions.SectionName))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Endpoint), "Endpoint é obrigatório.")
    .ValidateOnStart();

builder.Services.AddHttpClient("ingest", (sp, client) =>
{
    var opt = sp.GetRequiredService<IOptions<SensorSimulatorOptions>>().Value;
    client.DefaultRequestHeaders.Add("X-Api-Key", opt.ApiKey);
    client.BaseAddress = null;
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();
