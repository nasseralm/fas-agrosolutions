using FCG.API.Models;
using FCG.Application.DTOs;
using FCG.Domain.Interfaces;
using FCG.Domain.Notifications;
using FCG.Domain.DTOs;

namespace FCG.Application.Interfaces
{
    public interface IJogoEnhancedService : IJogoService
    {
        Task<DomainNotificationsResult<IEnumerable<JogoElasticsearchResponse>>> SearchJogosAsync(string searchTerm, int page = 1, int pageSize = 10, int? usuarioId = null);
        Task<DomainNotificationsResult<SyncReportResult>> SyncElasticSearchWithBaseAsync();
        Task<DomainNotificationsResult<UserPreferencesDto>> GetUserPreferencesAsync(int usuarioId);
    }
}