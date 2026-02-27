using Agro.DataReceiver.Application.DTOs;
using Agro.DataReceiver.Application.Interfaces;
using Agro.DataReceiver.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Agro.DataReceiver.Api.Controllers;

[ApiController]
[Route("v1/readings")]
public sealed class ReadingsController : ControllerBase
{
    private readonly IngestionService _ingestionService;
    private readonly ISensorReadingRepository _sensorReadingRepository;
    private readonly ILogger<ReadingsController> _logger;

    public ReadingsController(
        IngestionService ingestionService,
        ISensorReadingRepository sensorReadingRepository,
        ILogger<ReadingsController> logger)
    {
        _ingestionService = ingestionService;
        _sensorReadingRepository = sensorReadingRepository;
        _logger = logger;
    }

    /// <summary>
    /// Última leitura por talhão (umidade, etc.) para o dashboard.
    /// </summary>
    [HttpGet("latest")]
    [ProducesResponseType(typeof(IReadOnlyList<LatestReadingByTalhao>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLatest(
        [FromQuery] string? talhaoIds,
        CancellationToken cancellationToken)
    {
        var ids = string.IsNullOrWhiteSpace(talhaoIds)
            ? Array.Empty<string>()
            : talhaoIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var list = await _sensorReadingRepository.GetLatestByTalhaoIdsAsync(ids, cancellationToken);
        return Ok(list);
    }

    /// <summary>
    /// Média de umidade por hora nas últimas 24h (gráfico histórico do dashboard).
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(IReadOnlyList<HourlyUmidade>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string? talhaoIds,
        CancellationToken cancellationToken = default)
    {
        var ids = string.IsNullOrWhiteSpace(talhaoIds)
            ? Array.Empty<string>()
            : talhaoIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var list = await _sensorReadingRepository.GetHourlyAverageUmidadeLast24hAsync(ids, cancellationToken);
        return Ok(list);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SensorReadingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(SensorReadingResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Post(
        [FromBody] SensorReadingRequest request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received sensor reading from device {DeviceId}", request.DeviceId);

        IngestionResult result;
        try
        {
            result = await _ingestionService.ProcessReadingAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception processing reading for device {DeviceId}", request.DeviceId);
            var eventId = request?.DeviceId != null && request?.Timestamp != null
                ? $"{request.DeviceId}:{request.Timestamp}:{request.Seq ?? 0}"
                : "unknown";
            return StatusCode(500, new ErrorResponse
            {
                EventId = eventId,
                ErrorType = "ProcessingError",
                ErrorCode = "EXCEPTION",
                ErrorMessage = ex.Message
            });
        }

        return result.ResultType switch
        {
            IngestionResultType.Success => StatusCode(201, new SensorReadingResponse
            {
                EventId = result.EventId,
                DeviceId = result.DeviceId!,
                TalhaoId = result.TalhaoId!,
                ResolvedBy = result.ResolvedBy!,
                Timestamp = result.Timestamp!.Value,
                Message = "Reading processed successfully"
            }),

            IngestionResultType.Duplicate => StatusCode(202, new SensorReadingResponse
            {
                EventId = result.EventId,
                Message = "Reading already processed (duplicate)"
            }),

            IngestionResultType.ValidationError => BadRequest(new ErrorResponse
            {
                EventId = result.EventId,
                ErrorType = "ValidationError",
                ErrorCode = "INVALID_PAYLOAD",
                ErrorMessage = result.ErrorMessage!
            }),

            IngestionResultType.ResolutionError => UnprocessableEntity(new ErrorResponse
            {
                EventId = result.EventId,
                ErrorType = "ResolutionError",
                ErrorCode = "DEVICE_NOT_FOUND",
                ErrorMessage = result.ErrorMessage!
            }),

            IngestionResultType.TalhaoNotFound => UnprocessableEntity(new ErrorResponse
            {
                EventId = result.EventId,
                ErrorType = "ResolutionError",
                ErrorCode = "TALHAO_NOT_FOUND",
                ErrorMessage = result.ErrorMessage!
            }),

            _ => StatusCode(500, new ErrorResponse
            {
                EventId = result.EventId,
                ErrorType = "ProcessingError",
                ErrorCode = "EXCEPTION",
                ErrorMessage = "Unexpected error"
            })
        };
    }
}
