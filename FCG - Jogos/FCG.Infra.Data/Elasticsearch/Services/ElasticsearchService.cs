using FCG.Domain.Entities;
using FCG.Domain.Interfaces;
using FCG.Domain.DTOs;
using FCG.Infra.Data.Elasticsearch.Interfaces;
using Microsoft.Extensions.Logging;

namespace FCG.Infra.Data.Elasticsearch.Services
{
    public class ElasticsearchService : IElasticsearchService
    {
        private readonly IJogoCrudComponent _jogoCrudComponent;
        private readonly IJogoSearchComponent _jogoSearchComponent;
        private readonly IUserTrackingComponent _userTrackingComponent;
        private readonly ILogger<ElasticsearchService> _logger;

        public ElasticsearchService(
            IJogoCrudComponent jogoCrudComponent,
            IJogoSearchComponent jogoSearchComponent,
            IUserTrackingComponent userTrackingComponent,
            ILogger<ElasticsearchService> logger)
        {
            _jogoCrudComponent = jogoCrudComponent;
            _jogoSearchComponent = jogoSearchComponent;
            _userTrackingComponent = userTrackingComponent;
            _logger = logger;
        }

        public async Task<bool> IndexJogoAsync(Jogo jogo)
        {
            _logger.LogDebug("Delegando IndexJogo para JogoCrudComponent");
            return await _jogoCrudComponent.IndexJogoAsync(jogo);
        }

        public async Task<bool> UpdateJogoAsync(Jogo jogo)
        {
            _logger.LogDebug("Delegando UpdateJogo para JogoCrudComponent");
            return await _jogoCrudComponent.UpdateJogoAsync(jogo);
        }

        public async Task<bool> DeleteJogoAsync(int jogoId)
        {
            _logger.LogDebug("Delegando DeleteJogo para JogoCrudComponent");
            return await _jogoCrudComponent.DeleteJogoAsync(jogoId);
        }

        public async Task<bool> BulkIndexJogosAsync(IEnumerable<Jogo> jogos)
        {
            _logger.LogDebug("Delegando BulkIndexJogos para JogoCrudComponent");
            return await _jogoCrudComponent.BulkIndexJogosAsync(jogos);
        }

        public async Task<SyncReportResult> SyncJogosWithDetailedReportAsync(IEnumerable<Jogo> jogos)
        {
            _logger.LogDebug("Delegando SyncJogosWithDetailedReport para JogoCrudComponent");
            return await _jogoCrudComponent.SyncJogosWithDetailedReportAsync(jogos);
        }

        public async Task<IEnumerable<JogoElasticsearchResponse>> SearchJogosAsync(string searchTerm, int from = 0, int size = 10)
        {
            _logger.LogDebug("Delegando SearchJogos para JogoSearchComponent");
            return await _jogoSearchComponent.SearchJogosAsync(searchTerm, from, size);
        }

        public async Task<bool> TrackUserSearchAsync(int usuarioId, string searchTerm, string sessionId, int resultCount, List<string> foundGenres, List<string> foundDevelopers, List<string> foundPlatforms, List<string> foundGameNames)
        {
            _logger.LogDebug("Delegando TrackUserSearch para UserTrackingComponent");
            return await _userTrackingComponent.TrackUserSearchAsync(usuarioId, searchTerm, sessionId, resultCount, foundGenres, foundDevelopers, foundPlatforms, foundGameNames);
        }

        public async Task<UserPreferencesDto> GetUserPreferencesAsync(int usuarioId)
        {
            _logger.LogDebug("Delegando GetUserPreferences para UserTrackingComponent");
            return await _userTrackingComponent.GetUserPreferencesAsync(usuarioId);
        }

        public async Task<PopularGamesResponse> GetTopPopularGamesAsync(int limit = 5)
        {
            _logger.LogDebug("Delegando GetTopPopularGames para UserTrackingComponent");
            return await _userTrackingComponent.GetTopPopularGamesAsync(limit);
        }
    }
}