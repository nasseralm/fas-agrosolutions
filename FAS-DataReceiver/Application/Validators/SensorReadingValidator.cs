using Agro.DataReceiver.Application.DTOs;
using Agro.DataReceiver.Domain.Entities;

namespace Agro.DataReceiver.Application.Validators;

public sealed class SensorReadingValidator
{
    public ValidationResult Validate(SensorReadingRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.DeviceId))
        {
            errors.Add("deviceId is required");
        }

        if (string.IsNullOrWhiteSpace(request.Timestamp))
        {
            errors.Add("timestamp is required");
        }
        else if (!DateTime.TryParse(request.Timestamp, out var ts) || ts.Kind == DateTimeKind.Unspecified)
        {
            if (!DateTime.TryParse(request.Timestamp, null, System.Globalization.DateTimeStyles.RoundtripKind, out _))
            {
                errors.Add("timestamp must be a valid ISO 8601 UTC datetime");
            }
        }

        if (request.Geo is null)
        {
            errors.Add("geo is required for fallback resolution");
        }
        else
        {
            if (request.Geo.Lat is null || request.Geo.Lat < -90 || request.Geo.Lat > 90)
            {
                errors.Add("geo.lat must be between -90 and 90");
            }
            if (request.Geo.Lon is null || request.Geo.Lon < -180 || request.Geo.Lon > 180)
            {
                errors.Add("geo.lon must be between -180 and 180");
            }
        }

        if (request.Leituras is not null)
        {
            if (request.Leituras.UmidadeSoloPct.HasValue && 
                (request.Leituras.UmidadeSoloPct < 0 || request.Leituras.UmidadeSoloPct > 100))
            {
                errors.Add("leituras.umidadeSoloPct must be between 0 and 100");
            }

            if (request.Leituras.TemperaturaSoloC.HasValue && 
                (request.Leituras.TemperaturaSoloC < -40 || request.Leituras.TemperaturaSoloC > 80))
            {
                errors.Add("leituras.temperaturaSoloC must be between -40 and 80");
            }

            if (request.Leituras.PrecipitacaoMm.HasValue && request.Leituras.PrecipitacaoMm < 0)
            {
                errors.Add("leituras.precipitacaoMm must be >= 0");
            }

            if (request.Leituras.Ph.HasValue && (request.Leituras.Ph < 0 || request.Leituras.Ph > 14))
            {
                errors.Add("leituras.ph must be between 0 and 14");
            }

            if (request.Leituras.EcDsM.HasValue && request.Leituras.EcDsM < 0)
            {
                errors.Add("leituras.ecDsM must be >= 0");
            }
        }

        return new ValidationResult(errors.Count == 0, errors);
    }
}

public sealed record ValidationResult(bool IsValid, IReadOnlyList<string> Errors);
