using Agro.DataReceiver.Application.Interfaces;
using Agro.DataReceiver.Domain.Entities;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Agro.DataReceiver.Infrastructure.GeoJson;

public sealed class GeoFenceService : IGeoFenceService
{
    private readonly ITalhaoRepository _talhaoRepository;
    private readonly ILogger<GeoFenceService> _logger;
    private readonly GeometryFactory _geometryFactory;
    private readonly GeoJsonReader _geoJsonReader;
    
    private ConcurrentDictionary<string, TalhaoGeoData> _talhaoCache = new();
    private DateTime _lastCacheRefresh = DateTime.MinValue;
    private readonly TimeSpan _cacheTtl = TimeSpan.FromMinutes(5);
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public GeoFenceService(ITalhaoRepository talhaoRepository, ILogger<GeoFenceService> logger)
    {
        _talhaoRepository = talhaoRepository;
        _logger = logger;
        _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        _geoJsonReader = new GeoJsonReader(_geometryFactory, new JsonSerializerSettings());
    }

    public async Task<string?> FindTalhaoByLocationAsync(double lat, double lon, CancellationToken cancellationToken = default)
    {
        await EnsureCacheLoadedAsync(cancellationToken);

        var point = _geometryFactory.CreatePoint(new Coordinate(lon, lat));

        foreach (var (talhaoId, geoData) in _talhaoCache)
        {
            try
            {
                if (!geoData.BoundingBox.Contains(point))
                    continue;

                if (geoData.Geometry.Contains(point))
                {
                    _logger.LogDebug("Point ({Lat}, {Lon}) is inside talhao {TalhaoId}", lat, lon, talhaoId);
                    return talhaoId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking point against talhao {TalhaoId}", talhaoId);
            }
        }

        _logger.LogDebug("Point ({Lat}, {Lon}) not found in any talhao", lat, lon);
        return null;
    }

    public async Task RefreshCacheAsync(CancellationToken cancellationToken = default)
    {
        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Refreshing geofence cache...");

            var talhoes = await _talhaoRepository.GetActiveTalhoesWithGeoJsonAsync(cancellationToken);
            var newCache = new ConcurrentDictionary<string, TalhaoGeoData>();

            foreach (var talhao in talhoes)
            {
                if (string.IsNullOrWhiteSpace(talhao.GeoJson))
                    continue;

                try
                {
                    var geometry = ParseGeoJson(talhao.GeoJson);
                    if (geometry != null)
                    {
                        var envelope = geometry.EnvelopeInternal;
                        var bbox = _geometryFactory.ToGeometry(envelope);

                        newCache[talhao.Id] = new TalhaoGeoData(geometry, bbox);
                        _logger.LogDebug("Loaded geofence for talhao {TalhaoId}", talhao.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse GeoJSON for talhao {TalhaoId}", talhao.Id);
                }
            }

            _talhaoCache = newCache;
            _lastCacheRefresh = DateTime.UtcNow;

            _logger.LogInformation("Geofence cache refreshed with {Count} talhoes", newCache.Count);
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task EnsureCacheLoadedAsync(CancellationToken cancellationToken)
    {
        if (_talhaoCache.IsEmpty || DateTime.UtcNow - _lastCacheRefresh > _cacheTtl)
        {
            await RefreshCacheAsync(cancellationToken);
        }
    }

    private Geometry? ParseGeoJson(string geoJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(geoJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("type", out var typeElement))
            {
                var type = typeElement.GetString();
                
                if (type == "Feature" && root.TryGetProperty("geometry", out var geometryElement))
                {
                    return _geoJsonReader.Read<Geometry>(geometryElement.GetRawText());
                }
                else if (type == "FeatureCollection" && root.TryGetProperty("features", out var featuresElement))
                {
                    var features = featuresElement.EnumerateArray().ToList();
                    if (features.Count > 0 && features[0].TryGetProperty("geometry", out var firstGeometry))
                    {
                        return _geoJsonReader.Read<Geometry>(firstGeometry.GetRawText());
                    }
                }
                else if (type == "Polygon" || type == "MultiPolygon")
                {
                    return _geoJsonReader.Read<Geometry>(geoJson);
                }
            }

            return _geoJsonReader.Read<Geometry>(geoJson);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse GeoJSON");
            return null;
        }
    }

    private sealed record TalhaoGeoData(Geometry Geometry, Geometry BoundingBox);
}
