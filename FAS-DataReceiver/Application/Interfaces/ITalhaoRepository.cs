using Agro.DataReceiver.Domain.Entities;

namespace Agro.DataReceiver.Application.Interfaces;

public interface ITalhaoRepository
{
    Task<bool> ExistsAsync(string talhaoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Talhao>> GetActiveTalhoesWithGeoJsonAsync(CancellationToken cancellationToken = default);
}
