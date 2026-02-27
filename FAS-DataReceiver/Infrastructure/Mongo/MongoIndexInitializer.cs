using MongoDB.Driver;

namespace Agro.DataReceiver.Infrastructure.Mongo;

public sealed class MongoIndexInitializer : IHostedService
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<MongoIndexInitializer> _logger;

    public MongoIndexInitializer(IMongoDatabase database, ILogger<MongoIndexInitializer> logger)
    {
        _database = database;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing MongoDB indexes...");

        await CreateSensorReadingsIndexesAsync(cancellationToken);
        await CreateSensorReadingErrorsIndexesAsync(cancellationToken);

        _logger.LogInformation("MongoDB indexes initialization complete");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task CreateSensorReadingsIndexesAsync(CancellationToken cancellationToken)
    {
        var collection = _database.GetCollection<MongoDB.Bson.BsonDocument>("sensor_readings");

        var indexes = new[]
        {
            new CreateIndexModel<MongoDB.Bson.BsonDocument>(
                Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending("eventId"),
                new CreateIndexOptions { Unique = true, Name = "idx_eventId_unique" }),

            new CreateIndexModel<MongoDB.Bson.BsonDocument>(
                Builders<MongoDB.Bson.BsonDocument>.IndexKeys
                    .Ascending("deviceId")
                    .Descending("timestamp"),
                new CreateIndexOptions { Name = "idx_deviceId_timestamp" }),

            new CreateIndexModel<MongoDB.Bson.BsonDocument>(
                Builders<MongoDB.Bson.BsonDocument>.IndexKeys
                    .Ascending("talhaoId")
                    .Descending("timestamp"),
                new CreateIndexOptions { Name = "idx_talhaoId_timestamp" })
        };

        try
        {
            await collection.Indexes.CreateManyAsync(indexes, cancellationToken);
            _logger.LogDebug("sensor_readings indexes created/verified");
        }
        catch (MongoCommandException ex) when (ex.CodeName == "IndexOptionsConflict")
        {
            _logger.LogWarning("Some sensor_readings indexes already exist with different options");
        }
    }

    private async Task CreateSensorReadingErrorsIndexesAsync(CancellationToken cancellationToken)
    {
        var collection = _database.GetCollection<MongoDB.Bson.BsonDocument>("sensor_reading_errors");

        var indexes = new[]
        {
            new CreateIndexModel<MongoDB.Bson.BsonDocument>(
                Builders<MongoDB.Bson.BsonDocument>.IndexKeys.Ascending("eventId"),
                new CreateIndexOptions { Unique = true, Name = "idx_eventId_unique" }),

            new CreateIndexModel<MongoDB.Bson.BsonDocument>(
                Builders<MongoDB.Bson.BsonDocument>.IndexKeys
                    .Ascending("deviceId")
                    .Descending("timestamp"),
                new CreateIndexOptions { Name = "idx_deviceId_timestamp" })
        };

        try
        {
            await collection.Indexes.CreateManyAsync(indexes, cancellationToken);
            _logger.LogDebug("sensor_reading_errors indexes created/verified");
        }
        catch (MongoCommandException ex) when (ex.CodeName == "IndexOptionsConflict")
        {
            _logger.LogWarning("Some sensor_reading_errors indexes already exist with different options");
        }
    }
}
