namespace Agro.DataReceiver.Domain.Services;

/// <summary>
/// Motor simples de classificação de alerta de umidade do solo.
/// Regra de seca: umidade < 30%.
/// </summary>
public enum SoilMoistureAlertStatus
{
    Normal,
    Atencao,
    Seca
}

public static class SoilMoistureAlertEngine
{
    public static SoilMoistureAlertStatus Classify(double? umidadeSoloPct)
    {
        if (umidadeSoloPct is null)
        {
            return SoilMoistureAlertStatus.Normal;
        }

        if (umidadeSoloPct < 30)
        {
            return SoilMoistureAlertStatus.Seca;
        }

        if (umidadeSoloPct < 45)
        {
            return SoilMoistureAlertStatus.Atencao;
        }

        return SoilMoistureAlertStatus.Normal;
    }
}

