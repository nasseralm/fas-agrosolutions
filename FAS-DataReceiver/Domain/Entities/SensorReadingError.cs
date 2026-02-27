namespace Agro.DataReceiver.Domain.Entities;

public sealed class SensorReadingError
{
    public string EventId { get; set; } = string.Empty;
    public string? DeviceId { get; set; }
    public DateTime? Timestamp { get; set; }
    public GeoLocation? Geo { get; set; }
    public object? RawPayload { get; set; }
    public string ErrorType { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime IngestedAtUtc { get; set; }
}

public static class ErrorTypes
{
    public const string ValidationError = "ValidationError";
    public const string ResolutionError = "ResolutionError";
    public const string GeoFallbackError = "GeoFallbackError";
    public const string ProcessingError = "ProcessingError";
}

public static class ErrorCodes
{
    public const string InvalidRange = "INVALID_RANGE";
    public const string TalhaoNotFound = "TALHAO_NOT_FOUND";
    public const string GeoJsonNotMatch = "GEOJSON_NOT_MATCH";
    public const string DeviceNotFound = "DEVICE_NOT_FOUND";
    public const string Exception = "EXCEPTION";
    public const string InvalidPayload = "INVALID_PAYLOAD";
    public const string InvalidTimestamp = "INVALID_TIMESTAMP";
    public const string InvalidGeo = "INVALID_GEO";
}
