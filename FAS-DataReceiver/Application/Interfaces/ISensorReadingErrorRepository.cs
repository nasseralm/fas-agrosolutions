using Agro.DataReceiver.Domain.Entities;

namespace Agro.DataReceiver.Application.Interfaces;

public interface ISensorReadingErrorRepository
{
    Task InsertAsync(SensorReadingError error, CancellationToken cancellationToken = default);
}
