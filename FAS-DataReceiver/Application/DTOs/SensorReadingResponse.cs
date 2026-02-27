namespace Agro.DataReceiver.Application.DTOs;

public sealed class SensorReadingResponse
{
    public string EventId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string TalhaoId { get; set; } = string.Empty;
    public string ResolvedBy { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
}
