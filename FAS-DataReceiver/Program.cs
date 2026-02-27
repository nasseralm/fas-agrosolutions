using System.Text.Json;
using Agro.DataReceiver.Api.Middleware;
using Agro.DataReceiver.Application.Interfaces;
using Agro.DataReceiver.Application.Services;
using Agro.DataReceiver.Application.Validators;
using Agro.DataReceiver.Infrastructure.GeoJson;
using Agro.DataReceiver.Infrastructure.Kafka;
using Agro.DataReceiver.Infrastructure.Mongo;
using Agro.DataReceiver.Infrastructure.Redis;
using Agro.DataReceiver.Infrastructure.SqlServer;
using MongoDB.Driver;
using Prometheus;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Agro.DataReceiver - Ingestion API", Version = "v1" });
    c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "X-API-Key",
        Description = "API Key for authentication"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

var mongoConnectionString = builder.Configuration["Mongo:ConnectionString"] 
    ?? throw new InvalidOperationException("Mongo:ConnectionString not configured in appsettings.json");
var mongoDatabase = builder.Configuration["Mongo:Database"] 
    ?? throw new InvalidOperationException("Mongo:Database not configured in appsettings.json");
var mongoClient = new MongoClient(mongoConnectionString);
builder.Services.AddSingleton<IMongoClient>(mongoClient);
builder.Services.AddSingleton(mongoClient.GetDatabase(mongoDatabase));

var redisConnectionString = builder.Configuration["Redis:ConnectionString"] 
    ?? throw new InvalidOperationException("Redis:ConnectionString not configured in appsettings.json");
var redis = ConnectionMultiplexer.Connect(redisConnectionString);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

builder.Services.AddSingleton<IDeduplicationService, RedisDeduplicationService>();
builder.Services.AddSingleton<IDeviceCacheService, RedisDeviceCacheService>();

builder.Services.AddScoped<IDispositivoRepository, DispositivoRepository>();
builder.Services.AddScoped<ITalhaoRepository, TalhaoRepository>();

builder.Services.AddScoped<ISensorReadingRepository, SensorReadingRepository>();
builder.Services.AddScoped<ISensorReadingErrorRepository, SensorReadingErrorRepository>();

builder.Services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

builder.Services.AddSingleton<IGeoFenceService, GeoFenceService>();

builder.Services.AddScoped<SensorReadingValidator>();
builder.Services.AddScoped<IngestionService>();

builder.Services.AddHostedService<MongoIndexInitializer>();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Swagger disponível em todos os ambientes (incl. Docker) para documentação e testes
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ingestion API v1");
});

app.MapHealthChecks("/health");

app.UseHttpMetrics();
app.UseCors();

app.UseApiKeyAuthentication();

app.MapControllers();
app.MapMetrics();

app.Run();
