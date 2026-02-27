using Agro.DataReceiver.Application.DTOs;
using Agro.DataReceiver.Application.Interfaces;
using Confluent.Kafka;
using System.Text.Json;

namespace Agro.DataReceiver.Infrastructure.Kafka;

public sealed class KafkaEventPublisher : IEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;
    private readonly ILogger<KafkaEventPublisher> _logger;

    public KafkaEventPublisher(IConfiguration configuration, ILogger<KafkaEventPublisher> logger)
    {
        _logger = logger;
        
        var bootstrapServers = configuration["Kafka:BootstrapServers"] 
            ?? throw new InvalidOperationException("Kafka:BootstrapServers not configured in appsettings.json");
        _topic = configuration["Kafka:Topic"] 
            ?? throw new InvalidOperationException("Kafka:Topic not configured in appsettings.json");

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 100
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
        
        _logger.LogInformation("Kafka producer initialized with bootstrap servers: {BootstrapServers}, topic: {Topic}", 
            bootstrapServers, _topic);
    }

    public async Task PublishAsync(SensorReadingReceivedEvent @event, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(@event, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var message = new Message<string, string>
        {
            Key = @event.DeviceId,
            Value = json
        };

        try
        {
            var result = await _producer.ProduceAsync(_topic, message, cancellationToken);
            
            _logger.LogDebug("Published event {EventId} to {Topic}, partition: {Partition}, offset: {Offset}",
                @event.EventId, _topic, result.Partition.Value, result.Offset.Value);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventId} to Kafka", @event.EventId);
            throw;
        }
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}
