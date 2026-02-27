using Agro.DataReceiver.Application.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace Agro.DataReceiver.Infrastructure.Redis;

public sealed class RedisDeduplicationService : IDeduplicationService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisDeduplicationService> _logger;
    private static readonly TimeSpan DeduplicationTtl = TimeSpan.FromDays(7);

    public RedisDeduplicationService(IConnectionMultiplexer redis, ILogger<RedisDeduplicationService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<bool> IsDuplicateAsync(string eventId, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"dedupe:{eventId}";
            return await db.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking deduplication for {EventId}", eventId);
            return false;
        }
    }

    public async Task MarkAsProcessedAsync(string eventId, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"dedupe:{eventId}";
            await db.StringSetAsync(key, DateTime.UtcNow.ToString("O"), DeduplicationTtl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking {EventId} as processed", eventId);
        }
    }
}
