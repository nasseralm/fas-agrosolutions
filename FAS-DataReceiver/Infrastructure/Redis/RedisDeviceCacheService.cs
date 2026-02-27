using Agro.DataReceiver.Application.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace Agro.DataReceiver.Infrastructure.Redis;

public sealed class RedisDeviceCacheService : IDeviceCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisDeviceCacheService> _logger;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(24);

    public RedisDeviceCacheService(IConnectionMultiplexer redis, ILogger<RedisDeviceCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<string?> GetTalhaoIdAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"map:device:{deviceId}";
            var value = await db.StringGetAsync(key);

            if (value.IsNullOrEmpty)
                return null;

            var cached = JsonSerializer.Deserialize<DeviceCacheEntry>(value!);
            return cached?.TalhaoId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached talhao for device {DeviceId}", deviceId);
            return null;
        }
    }

    public async Task SetTalhaoIdAsync(string deviceId, string talhaoId, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"map:device:{deviceId}";
            var entry = new DeviceCacheEntry
            {
                TalhaoId = talhaoId,
                UpdatedAt = DateTime.UtcNow
            };
            var json = JsonSerializer.Serialize(entry);
            await db.StringSetAsync(key, json, CacheTtl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching talhao {TalhaoId} for device {DeviceId}", talhaoId, deviceId);
        }
    }

    private sealed class DeviceCacheEntry
    {
        public string TalhaoId { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }
}
