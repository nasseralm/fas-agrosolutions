using Agro.DataReceiver.Application.DTOs;
using Agro.DataReceiver.Application.Interfaces;
using Agro.DataReceiver.Application.Services;
using Agro.DataReceiver.Application.Validators;
using Agro.DataReceiver.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Agro.DataReceiver.Tests;

public class IngestionServiceTests
{
    private readonly Mock<IDeduplicationService> _deduplicationService = new();
    private readonly Mock<IDeviceCacheService> _deviceCacheService = new();
    private readonly Mock<IDispositivoRepository> _dispositivoRepository = new();
    private readonly Mock<ITalhaoRepository> _talhaoRepository = new();
    private readonly Mock<ISensorReadingRepository> _sensorReadingRepository = new();
    private readonly Mock<ISensorReadingErrorRepository> _sensorReadingErrorRepository = new();
    private readonly Mock<IEventPublisher> _eventPublisher = new();
    private readonly Mock<IGeoFenceService> _geoFenceService = new();
    private readonly SensorReadingValidator _validator = new();
    private readonly Mock<ILogger<IngestionService>> _logger = new();

    private IngestionService CreateService() => new(
        _deduplicationService.Object,
        _deviceCacheService.Object,
        _dispositivoRepository.Object,
        _talhaoRepository.Object,
        _sensorReadingRepository.Object,
        _sensorReadingErrorRepository.Object,
        _eventPublisher.Object,
        _geoFenceService.Object,
        _validator,
        _logger.Object
    );

    [Fact]
    public async Task ProcessReading_ValidPayload_ResolvesViaSqlAndSaves()
    {
        var request = CreateValidRequest();
        var service = CreateService();

        _deduplicationService.Setup(x => x.IsDuplicateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _deviceCacheService.Setup(x => x.GetTalhaoIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        _dispositivoRepository.Setup(x => x.GetTalhaoIdByDeviceIdAsync("SENS-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync("TAL-001");
        _talhaoRepository.Setup(x => x.ExistsAsync("TAL-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.ProcessReadingAsync(request);

        Assert.Equal(IngestionResultType.Success, result.ResultType);
        Assert.Equal("TAL-001", result.TalhaoId);
        Assert.Equal("deviceId", result.ResolvedBy);
        
        _sensorReadingRepository.Verify(x => x.InsertAsync(It.IsAny<SensorReading>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<SensorReadingReceivedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _deviceCacheService.Verify(x => x.SetTalhaoIdAsync("SENS-001", "TAL-001", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessReading_ValidPayload_ResolvesViaRedisAndSaves()
    {
        var request = CreateValidRequest();
        var service = CreateService();

        _deduplicationService.Setup(x => x.IsDuplicateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _deviceCacheService.Setup(x => x.GetTalhaoIdAsync("SENS-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync("TAL-001");
        _talhaoRepository.Setup(x => x.ExistsAsync("TAL-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.ProcessReadingAsync(request);

        Assert.Equal(IngestionResultType.Success, result.ResultType);
        Assert.Equal("TAL-001", result.TalhaoId);
        Assert.Equal("deviceId", result.ResolvedBy);
        
        _dispositivoRepository.Verify(x => x.GetTalhaoIdByDeviceIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _sensorReadingRepository.Verify(x => x.InsertAsync(It.IsAny<SensorReading>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<SensorReadingReceivedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessReading_ValidPayload_GeoFallbackResolvesAndSaves()
    {
        var request = CreateValidRequest();
        var service = CreateService();

        _deduplicationService.Setup(x => x.IsDuplicateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _deviceCacheService.Setup(x => x.GetTalhaoIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        _dispositivoRepository.Setup(x => x.GetTalhaoIdByDeviceIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        _geoFenceService.Setup(x => x.FindTalhaoByLocationAsync(-23.532, -46.791, It.IsAny<CancellationToken>()))
            .ReturnsAsync("TAL-002");
        _talhaoRepository.Setup(x => x.ExistsAsync("TAL-002", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.ProcessReadingAsync(request);

        Assert.Equal(IngestionResultType.Success, result.ResultType);
        Assert.Equal("TAL-002", result.TalhaoId);
        Assert.Equal("geo", result.ResolvedBy);
        
        _sensorReadingRepository.Verify(x => x.InsertAsync(It.IsAny<SensorReading>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<SensorReadingReceivedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessReading_GeoFallbackFails_Returns422AndSavesError()
    {
        var request = CreateValidRequest();
        var service = CreateService();

        _deduplicationService.Setup(x => x.IsDuplicateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _deviceCacheService.Setup(x => x.GetTalhaoIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        _dispositivoRepository.Setup(x => x.GetTalhaoIdByDeviceIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        _geoFenceService.Setup(x => x.FindTalhaoByLocationAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var result = await service.ProcessReadingAsync(request);

        Assert.Equal(IngestionResultType.ResolutionError, result.ResultType);
        
        _sensorReadingErrorRepository.Verify(x => x.InsertAsync(It.IsAny<SensorReadingError>(), It.IsAny<CancellationToken>()), Times.Once);
        _sensorReadingRepository.Verify(x => x.InsertAsync(It.IsAny<SensorReading>(), It.IsAny<CancellationToken>()), Times.Never);
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<SensorReadingReceivedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessReading_InvalidPayload_Returns400AndSavesError()
    {
        var request = CreateValidRequest();
        request.DeviceId = null;
        var service = CreateService();

        var result = await service.ProcessReadingAsync(request);

        Assert.Equal(IngestionResultType.ValidationError, result.ResultType);
        
        _sensorReadingErrorRepository.Verify(x => x.InsertAsync(It.IsAny<SensorReadingError>(), It.IsAny<CancellationToken>()), Times.Once);
        _sensorReadingRepository.Verify(x => x.InsertAsync(It.IsAny<SensorReading>(), It.IsAny<CancellationToken>()), Times.Never);
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<SensorReadingReceivedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessReading_TalhaoNotFound_Returns422AndSavesError()
    {
        var request = CreateValidRequest();
        var service = CreateService();

        _deduplicationService.Setup(x => x.IsDuplicateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _deviceCacheService.Setup(x => x.GetTalhaoIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("TAL-INVALID");
        _talhaoRepository.Setup(x => x.ExistsAsync("TAL-INVALID", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await service.ProcessReadingAsync(request);

        Assert.Equal(IngestionResultType.TalhaoNotFound, result.ResultType);
        
        _sensorReadingErrorRepository.Verify(x => x.InsertAsync(It.IsAny<SensorReadingError>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<SensorReadingReceivedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessReading_DuplicateEvent_Returns202()
    {
        var request = CreateValidRequest();
        var service = CreateService();

        _deduplicationService.Setup(x => x.IsDuplicateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.ProcessReadingAsync(request);

        Assert.Equal(IngestionResultType.Duplicate, result.ResultType);
        
        _sensorReadingRepository.Verify(x => x.InsertAsync(It.IsAny<SensorReading>(), It.IsAny<CancellationToken>()), Times.Never);
        _sensorReadingErrorRepository.Verify(x => x.InsertAsync(It.IsAny<SensorReadingError>(), It.IsAny<CancellationToken>()), Times.Never);
        _eventPublisher.Verify(x => x.PublishAsync(It.IsAny<SensorReadingReceivedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessReading_Success_PublishesKafkaEvent()
    {
        var request = CreateValidRequest();
        var service = CreateService();
        SensorReadingReceivedEvent? publishedEvent = null;

        _deduplicationService.Setup(x => x.IsDuplicateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _deviceCacheService.Setup(x => x.GetTalhaoIdAsync("SENS-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync("TAL-001");
        _talhaoRepository.Setup(x => x.ExistsAsync("TAL-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _eventPublisher.Setup(x => x.PublishAsync(It.IsAny<SensorReadingReceivedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<SensorReadingReceivedEvent, CancellationToken>((e, _) => publishedEvent = e)
            .Returns(Task.CompletedTask);

        await service.ProcessReadingAsync(request);

        Assert.NotNull(publishedEvent);
        Assert.Equal("SENS-001", publishedEvent!.DeviceId);
        Assert.Equal("TAL-001", publishedEvent.TalhaoId);
        Assert.Equal("deviceId", publishedEvent.ResolvedBy);
        Assert.Equal(32.5, publishedEvent.Summary.UmidadeSoloPct);
        Assert.Equal(24.1, publishedEvent.Summary.TemperaturaSoloC);
        Assert.Equal(0.0, publishedEvent.Summary.PrecipitacaoMm);
    }

    private static SensorReadingRequest CreateValidRequest() => new()
    {
        DeviceId = "SENS-001",
        Timestamp = "2024-06-07T15:30:00.000Z",
        Geo = new GeoRequest { Lat = -23.532, Lon = -46.791 },
        Leituras = new LeiturasRequest
        {
            UmidadeSoloPct = 32.5,
            TemperaturaSoloC = 24.1,
            PrecipitacaoMm = 0.0,
            Ph = 6.45,
            EcDsM = 1.23
        },
        BateriaPct = 98,
        RssiDbm = -67,
        Seq = 12
    };
}
