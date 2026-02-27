using FCG.API.Middleware;
using FCG.Infra.Ioc;
using Serilog;
using Serilog.Events;
using FCG.API.Extensions;

Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.Console(formatter: new Serilog.Formatting.Json.JsonFormatter())
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft.Extensions", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.WebTools", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
        .WriteTo.DatadogLogs(
            apiKey: "0c2d73dc31727a243ac17d54f3ab3b42",
            source: "aspnetcore",
            service: "fcg-api",
            host: "localhost",
            tags: new[] { "env:dev", "project:fcg" },
            configuration: new Serilog.Sinks.Datadog.Logs.DatadogConfiguration
    {
    Url = "https://http-intake.logs.us5.datadoghq.com",
    UseSSL = true,
    UseTCP = false
    })
        .CreateLogger();


try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddOpenTelemetryConfiguration(builder.Configuration);

    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddInfrastructureSwagger();

    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddHealthChecks();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy", builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseMiddleware<OpenTelemetryEnrichmentMiddleware>();

    app.UseMiddleware<ExceptionMiddleware>();
    app.UseMiddleware<ElasticsearchInitializationMiddleware>();

    app.UseCors("CorsPolicy");

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.MapHealthChecks("/health");

    Log.Information("API iniciada com sucesso com OpenTelemetry configurado!");

    app.Run();
}
catch (Exception e)
{
    Log.Logger.Fatal(e, "API não pode ser iniciada devido a uma falha: {exception}", e.Message);
}
finally
{
    OpenTelemetryExtensions.ActivitySource.Dispose();
    Log.CloseAndFlush();
}
