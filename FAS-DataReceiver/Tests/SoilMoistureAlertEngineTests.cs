using Agro.DataReceiver.Domain.Services;
using Xunit;

namespace Agro.DataReceiver.Tests;

public class SoilMoistureAlertEngineTests
{
    [Theory]
    [InlineData(null, SoilMoistureAlertStatus.Normal)]
    [InlineData(50, SoilMoistureAlertStatus.Normal)]
    [InlineData(45, SoilMoistureAlertStatus.Normal)]
    [InlineData(44.9, SoilMoistureAlertStatus.Atencao)]
    [InlineData(30, SoilMoistureAlertStatus.Atencao)]
    [InlineData(29.9, SoilMoistureAlertStatus.Seca)]
    [InlineData(0, SoilMoistureAlertStatus.Seca)]
    public void Classify_RetornaStatusEsperado(double? umidade, SoilMoistureAlertStatus esperado)
    {
        var status = SoilMoistureAlertEngine.Classify(umidade);

        Assert.Equal(esperado, status);
    }
}

