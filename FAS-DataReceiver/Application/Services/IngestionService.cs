using Agro.DataReceiver.Application.DTOs;
using Agro.DataReceiver.Application.Interfaces;
using Agro.DataReceiver.Application.Validators;
using Agro.DataReceiver.Domain.Entities;

namespace Agro.DataReceiver.Application.Services;

public sealed class IngestionService
{
    private readonly IDeduplicationService _deduplicationService;
    private readonly IDeviceCacheService _deviceCacheService;
    private readonly IDispositivoRepository _dispositivoRepository;
    private readonly ITalhaoRepository _talhaoRepository;
    private readonly ISensorReadingRepository _sensorReadingRepository;
    private readonly ISensorReadingErrorRepository _sensorReadingErrorRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IGeoFenceService _geoFenceService;
    private readonly SensorReadingValidator _validator;
    private readonly ILogger<IngestionService> _logger;

    public IngestionService(
        IDeduplicationService deduplicationService,
        IDeviceCacheService deviceCacheService,
        IDispositivoRepository dispositivoRepository,
        ITalhaoRepository talhaoRepository,
        ISensorReadingRepository sensorReadingRepository,
        ISensorReadingErrorRepository sensorReadingErrorRepository,
        IEventPublisher eventPublisher,
        IGeoFenceService geoFenceService,
        SensorReadingValidator validator,
        ILogger<IngestionService> logger)
    {
        _deduplicationService = deduplicationService;
        _deviceCacheService = deviceCacheService;
        _dispositivoRepository = dispositivoRepository;
        _talhaoRepository = talhaoRepository;
        _sensorReadingRepository = sensorReadingRepository;
        _sensorReadingErrorRepository = sensorReadingErrorRepository;
        _eventPublisher = eventPublisher;
        _geoFenceService = geoFenceService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<IngestionResult> ProcessReadingAsync(
        SensorReadingRequest request,
        CancellationToken cancellationToken = default)
    {
        var eventId = GenerateEventId(request);

        try
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for eventId {EventId}: {Errors}", 
                    eventId, string.Join("; ", validationResult.Errors));

                await SaveErrorAsync(eventId, request, ErrorTypes.ValidationError, 
                    ErrorCodes.InvalidPayload, string.Join("; ", validationResult.Errors), cancellationToken);

                return IngestionResult.ValidationError(eventId, validationResult.Errors);
            }

            if (await _deduplicationService.IsDuplicateAsync(eventId, cancellationToken))
            {
                _logger.LogInformation("Duplicate reading detected: {EventId}", eventId);
                return IngestionResult.Duplicate(eventId);
            }

            var (talhaoId, resolvedBy) = await ResolveTalhaoAsync(request, cancellationToken);

            if (string.IsNullOrEmpty(talhaoId))
            {
                _logger.LogWarning("Failed to resolve talhao for device {DeviceId}", request.DeviceId);

                await SaveErrorAsync(eventId, request, ErrorTypes.ResolutionError, 
                    ErrorCodes.DeviceNotFound, $"Could not resolve talhao for device {request.DeviceId}", cancellationToken);

                return IngestionResult.ResolutionError(eventId, $"Could not resolve talhao for device {request.DeviceId}");
            }

            var talhaoExists = await _talhaoRepository.ExistsAsync(talhaoId, cancellationToken);
            if (!talhaoExists)
            {
                _logger.LogWarning("Talhao {TalhaoId} does not exist or is inactive", talhaoId);

                await SaveErrorAsync(eventId, request, ErrorTypes.ResolutionError, 
                    ErrorCodes.TalhaoNotFound, $"Talhao {talhaoId} not found or inactive", cancellationToken);

                return IngestionResult.TalhaoNotFound(eventId, talhaoId);
            }

            var timestamp = DateTime.Parse(request.Timestamp!, null, 
                System.Globalization.DateTimeStyles.RoundtripKind);

            var reading = new SensorReading
            {
                EventId = eventId,
                DeviceId = request.DeviceId!,
                TalhaoId = talhaoId,
                ResolvedBy = resolvedBy,
                Timestamp = timestamp,
                Geo = new GeoLocation
                {
                    Lat = request.Geo!.Lat!.Value,
                    Lon = request.Geo!.Lon!.Value
                },
                Leituras = new SensorLeituras
                {
                    UmidadeSoloPct = request.Leituras?.UmidadeSoloPct,
                    TemperaturaSoloC = request.Leituras?.TemperaturaSoloC,
                    PrecipitacaoMm = request.Leituras?.PrecipitacaoMm,
                    Ph = request.Leituras?.Ph,
                    EcDsM = request.Leituras?.EcDsM
                },
                BateriaPct = request.BateriaPct,
                RssiDbm = request.RssiDbm,
                Seq = request.Seq,
                IngestedAtUtc = DateTime.UtcNow
            };

            await _sensorReadingRepository.InsertAsync(reading, cancellationToken);

            await _deduplicationService.MarkAsProcessedAsync(eventId, cancellationToken);

            var @event = new SensorReadingReceivedEvent
            {
                EventId = eventId,
                DeviceId = reading.DeviceId,
                TalhaoId = reading.TalhaoId,
                Timestamp = reading.Timestamp,
                ResolvedBy = reading.ResolvedBy,
                Summary = new ReadingSummary
                {
                    UmidadeSoloPct = reading.Leituras.UmidadeSoloPct,
                    TemperaturaSoloC = reading.Leituras.TemperaturaSoloC,
                    PrecipitacaoMm = reading.Leituras.PrecipitacaoMm
                }
            };

            await _eventPublisher.PublishAsync(@event, cancellationToken);

            _logger.LogInformation("Successfully processed reading {EventId} for device {DeviceId}, talhao {TalhaoId} (resolved by {ResolvedBy})",
                eventId, request.DeviceId, talhaoId, resolvedBy);

            return IngestionResult.Success(eventId, request.DeviceId!, talhaoId, resolvedBy, timestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing reading for device {DeviceId}", request.DeviceId);

            await SaveErrorAsync(eventId, request, ErrorTypes.ProcessingError, 
                ErrorCodes.Exception, ex.Message, cancellationToken);

            throw;
        }
    }

    private string GenerateEventId(SensorReadingRequest request)
    {
        var deviceId = request.DeviceId ?? "unknown";
        var timestamp = request.Timestamp ?? DateTime.UtcNow.ToString("O");
        var seq = request.Seq?.ToString() ?? "0";
        return $"{deviceId}:{timestamp}:{seq}";
    }

    private async Task<(string? TalhaoId, string ResolvedBy)> ResolveTalhaoAsync(
        SensorReadingRequest request, CancellationToken cancellationToken)
    {
        var cachedTalhaoId = await _deviceCacheService.GetTalhaoIdAsync(request.DeviceId!, cancellationToken);
        if (!string.IsNullOrEmpty(cachedTalhaoId))
        {
            return (cachedTalhaoId, "deviceId");
        }

        var talhaoIdFromDb = await _dispositivoRepository.GetTalhaoIdByDeviceIdAsync(request.DeviceId!, cancellationToken);
        if (!string.IsNullOrEmpty(talhaoIdFromDb))
        {
            await _deviceCacheService.SetTalhaoIdAsync(request.DeviceId!, talhaoIdFromDb, cancellationToken);
            return (talhaoIdFromDb, "deviceId");
        }

        if (request.Geo?.Lat != null && request.Geo?.Lon != null)
        {
            var geoTalhaoId = await _geoFenceService.FindTalhaoByLocationAsync(
                request.Geo.Lat.Value, request.Geo.Lon.Value, cancellationToken);

            if (!string.IsNullOrEmpty(geoTalhaoId))
            {
                return (geoTalhaoId, "geo");
            }
        }

        return (null, string.Empty);
    }

    private async Task SaveErrorAsync(
        string eventId,
        SensorReadingRequest request,
        string errorType,
        string errorCode,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        DateTime? timestamp = null;
        if (!string.IsNullOrWhiteSpace(request.Timestamp) &&
            DateTime.TryParse(request.Timestamp, null, System.Globalization.DateTimeStyles.RoundtripKind, out var ts))
        {
            timestamp = ts;
        }

        var error = new SensorReadingError
        {
            EventId = eventId,
            DeviceId = request.DeviceId,
            Timestamp = timestamp,
            Geo = request.Geo != null && request.Geo.Lat.HasValue && request.Geo.Lon.HasValue
                ? new GeoLocation { Lat = request.Geo.Lat.Value, Lon = request.Geo.Lon.Value }
                : null,
            RawPayload = request,
            ErrorType = errorType,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            IngestedAtUtc = DateTime.UtcNow
        };

        await _sensorReadingErrorRepository.InsertAsync(error, cancellationToken);
    }
}

public sealed class IngestionResult
{
    public IngestionResultType ResultType { get; private init; }
    public string EventId { get; private init; } = string.Empty;
    public string? DeviceId { get; private init; }
    public string? TalhaoId { get; private init; }
    public string? ResolvedBy { get; private init; }
    public DateTime? Timestamp { get; private init; }
    public string? ErrorMessage { get; private init; }
    public IReadOnlyList<string>? ValidationErrors { get; private init; }

    public static IngestionResult Success(string eventId, string deviceId, string talhaoId, string resolvedBy, DateTime timestamp)
        => new()
        {
            ResultType = IngestionResultType.Success,
            EventId = eventId,
            DeviceId = deviceId,
            TalhaoId = talhaoId,
            ResolvedBy = resolvedBy,
            Timestamp = timestamp
        };

    public static IngestionResult Duplicate(string eventId)
        => new()
        {
            ResultType = IngestionResultType.Duplicate,
            EventId = eventId
        };

    public static IngestionResult ValidationError(string eventId, IReadOnlyList<string> errors)
        => new()
        {
            ResultType = IngestionResultType.ValidationError,
            EventId = eventId,
            ValidationErrors = errors,
            ErrorMessage = string.Join("; ", errors)
        };

    public static IngestionResult ResolutionError(string eventId, string message)
        => new()
        {
            ResultType = IngestionResultType.ResolutionError,
            EventId = eventId,
            ErrorMessage = message
        };

    public static IngestionResult TalhaoNotFound(string eventId, string talhaoId)
        => new()
        {
            ResultType = IngestionResultType.TalhaoNotFound,
            EventId = eventId,
            TalhaoId = talhaoId,
            ErrorMessage = $"Talhao {talhaoId} not found or inactive"
        };
}

public enum IngestionResultType
{
    Success,
    Duplicate,
    ValidationError,
    ResolutionError,
    TalhaoNotFound
}
