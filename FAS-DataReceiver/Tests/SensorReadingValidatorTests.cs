using Agro.DataReceiver.Application.DTOs;
using Agro.DataReceiver.Application.Validators;
using Xunit;

namespace Agro.DataReceiver.Tests;

public class SensorReadingValidatorTests
{
    private readonly SensorReadingValidator _validator = new();

    [Fact]
    public void Validate_ValidPayload_ReturnsValid()
    {
        var request = CreateValidRequest();

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_MissingDeviceId_ReturnsError()
    {
        var request = CreateValidRequest();
        request.DeviceId = null;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("deviceId"));
    }

    [Fact]
    public void Validate_MissingTimestamp_ReturnsError()
    {
        var request = CreateValidRequest();
        request.Timestamp = null;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("timestamp"));
    }

    [Fact]
    public void Validate_InvalidTimestamp_ReturnsError()
    {
        var request = CreateValidRequest();
        request.Timestamp = "not-a-date";

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("timestamp"));
    }

    [Fact]
    public void Validate_MissingGeo_ReturnsError()
    {
        var request = CreateValidRequest();
        request.Geo = null;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("geo"));
    }

    [Fact]
    public void Validate_InvalidLatitude_ReturnsError()
    {
        var request = CreateValidRequest();
        request.Geo!.Lat = -91;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("lat"));
    }

    [Fact]
    public void Validate_InvalidLongitude_ReturnsError()
    {
        var request = CreateValidRequest();
        request.Geo!.Lon = 181;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("lon"));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Validate_InvalidUmidadeSoloPct_ReturnsError(double value)
    {
        var request = CreateValidRequest();
        request.Leituras!.UmidadeSoloPct = value;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("umidadeSoloPct"));
    }

    [Theory]
    [InlineData(-41)]
    [InlineData(81)]
    public void Validate_InvalidTemperaturaSoloC_ReturnsError(double value)
    {
        var request = CreateValidRequest();
        request.Leituras!.TemperaturaSoloC = value;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("temperaturaSoloC"));
    }

    [Fact]
    public void Validate_NegativePrecipitacao_ReturnsError()
    {
        var request = CreateValidRequest();
        request.Leituras!.PrecipitacaoMm = -1;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("precipitacaoMm"));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(15)]
    public void Validate_InvalidPh_ReturnsError(double value)
    {
        var request = CreateValidRequest();
        request.Leituras!.Ph = value;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("ph"));
    }

    [Fact]
    public void Validate_NegativeEcDsM_ReturnsError()
    {
        var request = CreateValidRequest();
        request.Leituras!.EcDsM = -1;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("ecDsM"));
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
