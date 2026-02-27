using FCG.Domain.DTOs;

namespace FCG.Infra.Data.Elasticsearch.Interfaces
{
    public interface IUserTrackingComponent : IElasticsearchComponent
    {
        Task<bool> TrackUserSearchAsync(int usuarioId, string searchTerm, string sessionId, int resultCount, List<string> foundGenres, List<string> foundDevelopers, List<string> foundPlatforms, List<string> foundGameNames);
        Task<UserPreferencesDto> GetUserPreferencesAsync(int usuarioId);
        Task<PopularGamesResponse> GetTopPopularGamesAsync(int limit = 5);
        Task<bool> CreateUserSearchIndexAsync();
        Task<bool> DeleteUserSearchIndexAsync();
    }
}