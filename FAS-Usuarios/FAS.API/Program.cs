using FAS.API.Middleware;
using FAS.API.Seed;
using FAS.Infra.Data.Context;
using Prometheus;
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
            service: "fas-api",
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

    app.UseMiddleware<ExceptionMiddleware>();

    app.UseHttpMetrics();
    app.UseCors("CorsPolicy");

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapMetrics();

    Log.Information("API Identity (Produtor Rural) iniciada com sucesso!");

    app.MapHealthChecks("/health");

    var conn = app.Configuration.GetConnectionString("DefaultConnection");
    if (conn != null && conn.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) && conn.IndexOf(".db", StringComparison.OrdinalIgnoreCase) >= 0)
    {
        // SQLite (dev): EnsureCreated evita migrations com tipos SQL Server (ex: varbinary(max))
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (!db.Database.CanConnect())
            {
                db.Database.EnsureCreated();
            }
            else
            {
                // Banco já existe: garantir que a tabela Usuario existe (evitar fas.db antigo/corrompido)
                var connObj = db.Database.GetDbConnection();
                connObj.Open();
                try
                {
                    using var cmd = connObj.CreateCommand();
                    cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Usuario'";
                    var existe = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    if (!existe)
                    {
                        db.Database.EnsureDeleted();
                        db.Database.EnsureCreated();
                    }
                }
                finally
                {
                    connObj.Close();
                }
            }
        }
    }
    else if (!string.IsNullOrEmpty(conn))
    {
        // SQL Server (produção): aplicar migrations
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
        }
    }

    app.SeedIdentityIfEmpty();

    app.Run();
}
catch (Exception e)
{
    Log.Logger.Fatal(e, "API não pode ser iniciada devido a uma falha: {exception}", e.Message);
}
finally
{
    Log.CloseAndFlush();
}
