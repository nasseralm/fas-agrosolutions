using FCG.Domain.Entities;
using FCG.Domain.DTOs;

namespace FCG.Domain.Interfaces
{
    public interface IElasticsearchService
    {
        Task<bool> IndexJogoAsync(Jogo jogo);
        Task<bool> UpdateJogoAsync(Jogo jogo);
        Task<bool> DeleteJogoAsync(int jogoId);
        Task<IEnumerable<JogoElasticsearchResponse>> SearchJogosAsync(string searchTerm, int from = 0, int size = 10);
        Task<bool> BulkIndexJogosAsync(IEnumerable<Jogo> jogos);
        Task<SyncReportResult> SyncJogosWithDetailedReportAsync(IEnumerable<Jogo> jogos);
        Task<bool> TrackUserSearchAsync(int usuarioId, string searchTerm, string sessionId, int resultCount, List<string> foundGenres, List<string> foundDevelopers, List<string> foundPlatforms, List<string> foundGameNames);
        Task<UserPreferencesDto> GetUserPreferencesAsync(int usuarioId);
        Task<PopularGamesResponse> GetTopPopularGamesAsync(int limit = 5);
    }
}