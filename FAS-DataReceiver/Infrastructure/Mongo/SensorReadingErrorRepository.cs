using Agro.DataReceiver.Application.Interfaces;
using Agro.DataReceiver.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Agro.DataReceiver.Infrastructure.Mongo;

public sealed class SensorReadingErrorRepository : ISensorReadingErrorRepository
{
    private readonly IMongoCollection<SensorReadingErrorDocument> _collection;
    private readonly ILogger<SensorReadingErrorRepository> _logger;

    public SensorReadingErrorRepository(IMongoDatabase database, ILogger<SensorReadingErrorRepository> logger)
    {
        _collection = database.GetCollection<SensorReadingErrorDocument>("sensor_reading_errors");
        _logger = logger;
    }

    public async Task InsertAsync(SensorReadingError error, CancellationToken cancellationToken = default)
    {
        var document = new SensorReadingErrorDocument
        {
            EventId = error.EventId,
            DeviceId = error.DeviceId,
            Timestamp = error.Timestamp,
            Geo = error.Geo != null ? new GeoLocationErrorDocument
            {
                Lat = error.Geo.Lat,
                Lon = error.Geo.Lon
            } : null,
            RawPayload = error.RawPayload != null ? error.RawPayload.ToBsonDocument() : null,
            ErrorType = error.ErrorType,
            ErrorCode = error.ErrorCode,
            ErrorMessage = error.ErrorMessage,
            IngestedAtUtc = error.IngestedAtUtc
        };

        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
        _logger.LogDebug("Inserted sensor reading error {EventId}: {ErrorType}/{ErrorCode}", 
            error.EventId, error.ErrorType, error.ErrorCode);
    }
}

[BsonIgnoreExtraElements]
internal sealed class SensorReadingErrorDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("eventId")]
    public string EventId { get; set; } = string.Empty;

    [BsonElement("deviceId")]
    public string? DeviceId { get; set; }

    [BsonElement("timestamp")]
    public DateTime? Timestamp { get; set; }

    [BsonElement("geo")]
    public GeoLocationErrorDocument? Geo { get; set; }

    [BsonElement("rawPayload")]
    public BsonDocument? RawPayload { get; set; }

    [BsonElement("errorType")]
    public string ErrorType { get; set; } = string.Empty;

    [BsonElement("errorCode")]
    public string ErrorCode { get; set; } = string.Empty;

    [BsonElement("errorMessage")]
    public string ErrorMessage { get; set; } = string.Empty;

    [BsonElement("ingestedAtUtc")]
    public DateTime IngestedAtUtc { get; set; }
}

internal sealed class GeoLocationErrorDocument
{
    [BsonElement("lat")]
    public double Lat { get; set; }

    [BsonElement("lon")]
    public double Lon { get; set; }
}
