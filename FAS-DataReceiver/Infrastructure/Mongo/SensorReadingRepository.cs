using Agro.DataReceiver.Application.Interfaces;
using Agro.DataReceiver.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Agro.DataReceiver.Infrastructure.Mongo;

public sealed class SensorReadingRepository : ISensorReadingRepository
{
    private readonly IMongoCollection<SensorReadingDocument> _collection;
    private readonly ILogger<SensorReadingRepository> _logger;

    public SensorReadingRepository(IMongoDatabase database, ILogger<SensorReadingRepository> logger)
    {
        _collection = database.GetCollection<SensorReadingDocument>("sensor_readings");
        _logger = logger;
    }

    public async Task<IReadOnlyList<LatestReadingByTalhao>> GetLatestByTalhaoIdsAsync(
        IEnumerable<string> talhaoIds,
        CancellationToken cancellationToken = default)
    {
        var ids = talhaoIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
        if (ids.Count == 0)
            return Array.Empty<LatestReadingByTalhao>();

        var results = new List<LatestReadingByTalhao>();
        foreach (var talhaoId in ids)
        {
            var doc = await _collection
                .Find(Builders<SensorReadingDocument>.Filter.Eq(x => x.TalhaoId, talhaoId))
                .SortByDescending(x => x.Timestamp)
                .Limit(1)
                .FirstOrDefaultAsync(cancellationToken);
            if (doc != null)
                results.Add(new LatestReadingByTalhao
                {
                    TalhaoId = doc.TalhaoId,
                    UmidadeSoloPct = doc.Leituras?.UmidadeSoloPct,
                    Timestamp = doc.Timestamp
                });
        }
        return results;
    }

    public async Task InsertAsync(SensorReading reading, CancellationToken cancellationToken = default)
    {
        var document = new SensorReadingDocument
        {
            EventId = reading.EventId,
            DeviceId = reading.DeviceId,
            TalhaoId = reading.TalhaoId,
            ResolvedBy = reading.ResolvedBy,
            Timestamp = reading.Timestamp,
            Geo = new GeoLocationDocument
            {
                Lat = reading.Geo.Lat,
                Lon = reading.Geo.Lon
            },
            Leituras = new LeiturasDocument
            {
                UmidadeSoloPct = reading.Leituras.UmidadeSoloPct,
                TemperaturaSoloC = reading.Leituras.TemperaturaSoloC,
                PrecipitacaoMm = reading.Leituras.PrecipitacaoMm,
                Ph = reading.Leituras.Ph,
                EcDsM = reading.Leituras.EcDsM
            },
            BateriaPct = reading.BateriaPct,
            RssiDbm = reading.RssiDbm,
            Seq = reading.Seq,
            IngestedAtUtc = reading.IngestedAtUtc
        };

        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
        _logger.LogDebug("Inserted sensor reading {EventId}", reading.EventId);
    }

    public async Task<IReadOnlyList<HourlyUmidade>> GetHourlyAverageUmidadeLast24hAsync(
        IEnumerable<string> talhaoIds,
        CancellationToken cancellationToken = default)
    {
        var ids = talhaoIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
        if (ids.Count == 0)
            return BuildEmptyHourlySlots();

        var since = DateTime.UtcNow.AddHours(-24);
        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument
            {
                { "talhaoId", new BsonDocument("$in", new BsonArray(ids)) },
                { "timestamp", new BsonDocument("$gte", since) },
                { "leituras.umidadeSoloPct", new BsonDocument("$exists", true) }
            }),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", new BsonDocument("$hour", "$timestamp") },
                { "avgUmidade", new BsonDocument("$avg", "$leituras.umidadeSoloPct") }
            }),
            new BsonDocument("$sort", new BsonDocument("_id", 1))
        };

        var bsonCollection = _collection.Database.GetCollection<BsonDocument>(_collection.CollectionNamespace.CollectionName);
        var cursor = await bsonCollection.AggregateAsync<BsonDocument>(pipeline, cancellationToken: cancellationToken);
        var list = await cursor.ToListAsync(cancellationToken);

        var byHour = new Dictionary<int, double>();
        foreach (var doc in list)
        {
            if (!doc.TryGetValue("_id", out var idVal) || idVal.BsonType != BsonType.Int32 ||
                !doc.TryGetValue("avgUmidade", out var avgVal))
                continue;
            var avg = avgVal.BsonType == BsonType.Double
                ? avgVal.AsDouble
                : (avgVal.BsonType == BsonType.Int32 ? avgVal.AsInt32 : (avgVal.BsonType == BsonType.Int64 ? (double)avgVal.AsInt64 : 0));
            byHour[idVal.AsInt32] = avg;
        }

        var result = new List<HourlyUmidade>(24);
        for (var h = 0; h < 24; h++)
        {
            var hourStr = h < 10 ? $"0{h}" : h.ToString();
            result.Add(new HourlyUmidade
            {
                Hour = hourStr,
                UmidadePct = byHour.TryGetValue(h, out var v) ? Math.Round(v, 1) : 0
            });
        }
        return result;
    }

    private static List<HourlyUmidade> BuildEmptyHourlySlots()
    {
        var list = new List<HourlyUmidade>(24);
        for (var h = 0; h < 24; h++)
            list.Add(new HourlyUmidade { Hour = h < 10 ? $"0{h}" : h.ToString(), UmidadePct = 0 });
        return list;
    }
}

[BsonIgnoreExtraElements]
internal sealed class SensorReadingDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("eventId")]
    public string EventId { get; set; } = string.Empty;

    [BsonElement("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [BsonElement("talhaoId")]
    public string TalhaoId { get; set; } = string.Empty;

    [BsonElement("resolvedBy")]
    public string ResolvedBy { get; set; } = string.Empty;

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }

    [BsonElement("geo")]
    public GeoLocationDocument Geo { get; set; } = new();

    [BsonElement("leituras")]
    public LeiturasDocument Leituras { get; set; } = new();

    [BsonElement("bateriaPct")]
    public double? BateriaPct { get; set; }

    [BsonElement("rssiDbm")]
    public int? RssiDbm { get; set; }

    [BsonElement("seq")]
    public int? Seq { get; set; }

    [BsonElement("ingestedAtUtc")]
    public DateTime IngestedAtUtc { get; set; }
}

internal sealed class GeoLocationDocument
{
    [BsonElement("lat")]
    public double Lat { get; set; }

    [BsonElement("lon")]
    public double Lon { get; set; }
}

internal sealed class LeiturasDocument
{
    [BsonElement("umidadeSoloPct")]
    public double? UmidadeSoloPct { get; set; }

    [BsonElement("temperaturaSoloC")]
    public double? TemperaturaSoloC { get; set; }

    [BsonElement("precipitacaoMm")]
    public double? PrecipitacaoMm { get; set; }

    [BsonElement("ph")]
    public double? Ph { get; set; }

    [BsonElement("ecDsM")]
    public double? EcDsM { get; set; }
}
