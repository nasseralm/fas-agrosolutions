using FAS.API.Middleware;
using FAS.API.Seed;
using FAS.Infra.Data.Context;
using FAS.Infra.Ioc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

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
            service: "fas-properties-api",
            host: "localhost",
            tags: new[] { "env:dev", "project:fas" },
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

    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddInfrastructureSwagger();

    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddHealthChecks();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy", corsBuilder =>
        {
            corsBuilder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseMiddleware<ExceptionMiddleware>();

    app.UseCors("CorsPolicy");

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("API Properties (Propriedades/Talhões) iniciada com sucesso!");

    app.MapHealthChecks("/health");

    var conn = app.Configuration.GetConnectionString("DefaultConnection");
    if (conn != null && conn.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) && conn.IndexOf(".db", StringComparison.OrdinalIgnoreCase) >= 0)
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (!db.Database.CanConnect())
            {
                db.Database.EnsureCreated();
            }
        }
    }
    else if (!string.IsNullOrEmpty(conn))
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
        }
    }

    app.SeedPropertiesIfEmpty();

    app.Run();
}
catch (Exception e)
{
    if (e is Microsoft.Extensions.Hosting.HostAbortedException)
    {
        // dotnet-ef usa HostFactoryResolver e pode abortar o host durante design-time.
        // Evita logar "Fatal" nesses casos.
        return;
    }
    Log.Logger.Fatal(e, "API não pode ser iniciada devido a uma falha: {exception}", e.Message);
}
finally
{
    Log.CloseAndFlush();
}
