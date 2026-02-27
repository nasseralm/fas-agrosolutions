namespace Agro.DataReceiver.Application.DTOs;

public sealed class ErrorResponse
{
    public string EventId { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
