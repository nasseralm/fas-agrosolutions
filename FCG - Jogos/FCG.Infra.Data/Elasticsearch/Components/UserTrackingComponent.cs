using FCG.Domain.DTOs;
using FCG.Infra.Data.Elasticsearch.Components.Base;
using FCG.Infra.Data.Elasticsearch.Configuration;
using FCG.Infra.Data.Elasticsearch.Interfaces;
using FCG.Infra.Data.Elasticsearch.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

namespace FCG.Infra.Data.Elasticsearch.Components
{
    public class UserTrackingComponent : ElasticsearchComponentBase, IUserTrackingComponent
    {
        private readonly string UserSearchIndex;
        public override string ComponentName => "UserTracking";

        public UserTrackingComponent(
            IElasticClient elasticClient,
            ILogger<UserTrackingComponent> logger,
            IOptions<ElasticsearchSettings> settings)
            : base(elasticClient, logger, settings)
        {
            UserSearchIndex = $"{DefaultIndex}-user-search-history";
        }

        public async Task<bool> TrackUserSearchAsync(int usuarioId, string searchTerm, string sessionId, int resultCount, List<string> foundGenres, List<string> foundDevelopers, List<string> foundPlatforms, List<string> foundGameNames)
        {
            try
            {
                var searchDoc = new UserSearchHistoryDocument
                {
                    UsuarioId = usuarioId,
                    SearchTerm = searchTerm?.ToLowerInvariant(),
                    SessionId = sessionId,
                    ResultCount = resultCount,
                    FoundGenres = foundGenres ?? new List<string>(),
                    FoundDevelopers = foundDevelopers ?? new List<string>(),
                    FoundPlatforms = foundPlatforms ?? new List<string>(),
                    FoundGameNames = foundGameNames ?? new List<string>(),
                    Timestamp = DateTime.UtcNow
                };

                var response = await ElasticClient.IndexAsync(searchDoc, idx => idx
                    .Index(UserSearchIndex)
                    .Id(searchDoc.Id));

                if (response.IsValid)
                {
                    LogOperation("TrackUserSearch - Sucesso", new { 
                        UsuarioId = usuarioId, 
                        SearchTerm = searchTerm
                    });
                    
                    await ElasticClient.Indices.RefreshAsync(UserSearchIndex);
                    
                    return true;
                }

                LogError("TrackUserSearch", new Exception(response.OriginalException?.Message ?? response.ServerError?.ToString()), 
                    new { UsuarioId = usuarioId, SearchTerm = searchTerm });
                return false;
            }
            catch (Exception ex)
            {
                LogError("TrackUserSearch", ex, new { UsuarioId = usuarioId, SearchTerm = searchTerm });
                return false;
            }
        }

        public async Task<UserPreferencesDto> GetUserPreferencesAsync(int usuarioId)
        {
            try
            {
                LogOperation("GetUserPreferences", new { UsuarioId = usuarioId });

                var response = await ElasticClient.SearchAsync<UserSearchHistoryDocument>(s => s
                    .Index(UserSearchIndex)
                    .Size(0)
                    .Query(q => q.Term(t => t.Field(f => f.UsuarioId).Value(usuarioId)))
                    .Aggregations(a => a
                        .Terms("top_search_terms", t => t
                            .Field(f => f.SearchTerm)
                            .Size(10)
                            .MinimumDocumentCount(1))
                        .Terms("top_genres", t => t
                            .Field(f => f.FoundGenres)
                            .Size(10)
                            .MinimumDocumentCount(1))
                        .Terms("top_developers", t => t
                            .Field(f => f.FoundDevelopers)
                            .Size(10)
                            .MinimumDocumentCount(1))
                        .Terms("top_platforms", t => t
                            .Field(f => f.FoundPlatforms)
                            .Size(10)
                            .MinimumDocumentCount(1))
                        .Terms("top_game_names", t => t
                            .Field(f => f.FoundGameNames)
                            .Size(10)
                            .MinimumDocumentCount(1))
                        .Max("last_search", m => m.Field(f => f.Timestamp))));

                if (response.IsValid)
                {
                    var preferences = BuildUserPreferences(usuarioId, response);
                    LogOperation("GetUserPreferences - Sucesso", new { 
                        UsuarioId = usuarioId, 
                        TotalSearches = preferences.TotalSearches
                    });
                    return preferences;
                }

                var errorMessage = response.OriginalException?.Message ?? response.ServerError?.ToString() ?? "Erro desconhecido";
                
                LogError("GetUserPreferences", new Exception(errorMessage), new { UsuarioId = usuarioId });
                
                throw new InvalidOperationException($"Falha ao consultar preferências do usuário {usuarioId}. " +
                    $"Erro: {errorMessage}. Verifique o mapeamento do índice e se os campos estão configurados como KEYWORD para agregações.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogError("GetUserPreferences", ex, new { UsuarioId = usuarioId });
                throw new InvalidOperationException($"Erro inesperado ao obter preferências do usuário {usuarioId}: {ex.Message}", ex);
            }
        }

        public async Task<bool> CreateUserSearchIndexAsync()
        {
            try
            {
                LogOperation("CreateUserSearchIndex", new { IndexName = UserSearchIndex });

                var existsResponse = await ElasticClient.Indices.ExistsAsync(UserSearchIndex);
                
                if (!existsResponse.IsValid && existsResponse.ApiCall.HttpStatusCode != 404)
                {
                    LogError("CreateUserSearchIndex", new Exception("Erro ao verificar ocorrência do índice"), 
                        new { IndexName = UserSearchIndex, StatusCode = existsResponse.ApiCall.HttpStatusCode });
                    return false;
                }

                if (existsResponse.Exists && existsResponse.ApiCall.HttpStatusCode != 404)
                {
                    LogWarning("CreateUserSearchIndex", "Índice já existe - DELETANDO para recriar com mapeamento correto", new { IndexName = UserSearchIndex });
                    
                    var deleteResponse = await ElasticClient.Indices.DeleteAsync(UserSearchIndex);
                    if (!deleteResponse.IsValid && deleteResponse.ApiCall.HttpStatusCode != 404)
                    {
                        LogError("CreateUserSearchIndex", new Exception("Erro ao deletar índice existente"), 
                            new { IndexName = UserSearchIndex, StatusCode = deleteResponse.ApiCall.HttpStatusCode });
                        return false;
                    }
                    
                    await Task.Delay(1000);
                }

                var createResponse = await ElasticClient.Indices.CreateAsync(UserSearchIndex, c => c
                    .Settings(s => s
                        .NumberOfShards(1)
                        .NumberOfReplicas(0))
                    .Map<UserSearchHistoryDocument>(m => m
                        .Properties(p => p
                            .Number(n => n.Name(nm => nm.UsuarioId).Type(NumberType.Integer))
                            .Keyword(k => k.Name(n => n.SearchTerm))
                            .Date(d => d.Name(n => n.Timestamp))
                            .Keyword(k => k.Name(n => n.SessionId))
                            .Number(n => n.Name(nm => nm.ResultCount).Type(NumberType.Integer))
                            .Keyword(k => k.Name(n => n.FoundGenres))
                            .Keyword(k => k.Name(n => n.FoundDevelopers))
                            .Keyword(k => k.Name(n => n.FoundPlatforms))
                            .Keyword(k => k.Name(n => n.FoundGameNames)))));


                if (createResponse.IsValid)
                {
                    LogOperation("CreateUserSearchIndex - Sucesso com mapeamento KEYWORD", new { IndexName = UserSearchIndex });
                    return true;
                }

                LogError("CreateUserSearchIndex", new Exception(createResponse.OriginalException?.Message ?? createResponse.ServerError?.ToString()), 
                    new { IndexName = UserSearchIndex, StatusCode = createResponse.ApiCall?.HttpStatusCode });
                return false;
            }
            catch (Exception ex)
            {
                LogError("CreateUserSearchIndex", ex, new { IndexName = UserSearchIndex });
                return false;
            }
        }

        public async Task<bool> DeleteUserSearchIndexAsync()
        {
            try
            {
                LogOperation("DeleteUserSearchIndex", new { IndexName = UserSearchIndex });

                var existsResponse = await ElasticClient.Indices.ExistsAsync(UserSearchIndex);
                
                if (!existsResponse.IsValid && existsResponse.ApiCall.HttpStatusCode != 404)
                {
                    LogError("DeleteUserSearchIndex", new Exception("Erro ao verificar existência do índice"), 
                        new { IndexName = UserSearchIndex, StatusCode = existsResponse.ApiCall.HttpStatusCode });
                    return false;
                }

                if (!existsResponse.Exists || existsResponse.ApiCall.HttpStatusCode == 404)
                {
                    LogWarning("DeleteUserSearchIndex", "Índice não existe (já foi deletado ou nunca criado)", 
                        new { IndexName = UserSearchIndex, StatusCode = existsResponse.ApiCall.HttpStatusCode });
                    return true;
                }

                var deleteResponse = await ElasticClient.Indices.DeleteAsync(UserSearchIndex);

                if (deleteResponse.IsValid || deleteResponse.ApiCall.HttpStatusCode == 404)
                {
                    LogOperation("DeleteUserSearchIndex - Sucesso", new { IndexName = UserSearchIndex });
                    return true;
                }

                LogError("DeleteUserSearchIndex", new Exception(deleteResponse.OriginalException?.Message ?? deleteResponse.ServerError?.ToString()), 
                    new { IndexName = UserSearchIndex, StatusCode = deleteResponse.ApiCall.HttpStatusCode });
                return false;
            }
            catch (Exception ex)
            {
                LogError("DeleteUserSearchIndex", ex, new { IndexName = UserSearchIndex });
                return false;
            }
        }

        public async Task<PopularGamesResponse> GetTopPopularGamesAsync(int limit = 5)
        {
            try
            {
                LogOperation("GetTopPopularGames", new { Limit = limit });

                var response = await ElasticClient.SearchAsync<UserSearchHistoryDocument>(s => s
                    .Index(UserSearchIndex)
                    .Size(0)
                    .Aggregations(a => a
                        .Terms("top_genres", t => t
                            .Field(f => f.FoundGenres)
                            .Size(limit)
                            .Order(o => o.CountDescending())
                            .MinimumDocumentCount(1))
                        .Cardinality("unique_users", c => c
                            .Field(f => f.UsuarioId))
                        .ValueCount("total_searches", vc => vc
                            .Field(f => f.SearchTerm))));

                if (response.IsValid)
                {
                    var result = BuildPopularGamesResponse(response, limit);
                    LogOperation("GetTopPopularGames - Sucesso", new { 
                        TopGenres = result.TopGenres.Count,
                        TotalSearches = result.TotalSearches
                    });
                    return result;
                }

                LogError("GetTopPopularGames", new Exception(response.OriginalException?.Message ?? response.ServerError?.ToString()));
                return new PopularGamesResponse();
            }
            catch (Exception ex)
            {
                LogError("GetTopPopularGames", ex, new { Limit = limit });
                return new PopularGamesResponse();
            }
        }

        private PopularGamesResponse BuildPopularGamesResponse(ISearchResponse<UserSearchHistoryDocument> response, int limit)
        {
            var result = new PopularGamesResponse
            {
                LastUpdated = DateTime.UtcNow
            };

            if (response.Aggregations?.Terms("top_genres") != null)
            {
                var genresBuckets = response.Aggregations.Terms("top_genres").Buckets;
                result.TopGenres = genresBuckets.Take(limit).Select(bucket => new PopularGenreItem
                {
                    Genre = bucket.Key,
                    SearchCount = (int)bucket.DocCount,
                    PopularityScore = CalculatePopularityScore((int)bucket.DocCount)
                }).ToList();
            }

            if (response.Aggregations?.ValueCount("total_searches")?.Value.HasValue == true)
            {
                result.TotalSearches = (int)response.Aggregations.ValueCount("total_searches").Value.Value;
            }

            if (response.Aggregations?.Cardinality("unique_users")?.Value.HasValue == true)
            {
                result.UniqueUsers = (int)response.Aggregations.Cardinality("unique_users").Value.Value;
            }

            return result;
        }

        private double CalculatePopularityScore(int searchCount)
        {
            return Math.Log10(searchCount + 1) * searchCount;
        }

        private UserPreferencesDto BuildUserPreferences(int usuarioId, ISearchResponse<UserSearchHistoryDocument> response)
        {
            var preferences = new UserPreferencesDto
            {
                UsuarioId = usuarioId,
                TotalSearches = (int)response.Total
            };

            if (response.Aggregations != null)
            {
                if (response.Aggregations.Terms("top_search_terms") != null)
                {
                    var topTerms = response.Aggregations.Terms("top_search_terms");
                    preferences.TopSearchTerms = topTerms.Buckets
                        .Where(b => !string.IsNullOrEmpty(b.Key))
                        .Take(3)
                        .Select(b => b.Key)
                        .ToList();
                }

                if (response.Aggregations.Terms("top_genres") != null)
                {
                    var topGenres = response.Aggregations.Terms("top_genres");
                    preferences.TopGenres = topGenres.Buckets
                        .Where(b => !string.IsNullOrEmpty(b.Key))
                        .Take(3)
                        .Select(b => b.Key)
                        .ToList();
                }

                if (response.Aggregations.Terms("top_developers") != null)
                {
                    var topDevelopers = response.Aggregations.Terms("top_developers");
                    preferences.TopDevelopers = topDevelopers.Buckets
                        .Where(b => !string.IsNullOrEmpty(b.Key))
                        .Take(3)
                        .Select(b => b.Key)
                        .ToList();
                }

                if (response.Aggregations.Terms("top_platforms") != null)
                {
                    var topPlatforms = response.Aggregations.Terms("top_platforms");
                    preferences.TopPlatforms = topPlatforms.Buckets
                        .Where(b => !string.IsNullOrEmpty(b.Key))
                        .Take(3)
                        .Select(b => b.Key)
                        .ToList();
                }

                if (response.Aggregations.Terms("top_game_names") != null)
                {
                    var topGameNames = response.Aggregations.Terms("top_game_names");
                    preferences.TopGameNames = topGameNames.Buckets
                        .Where(b => !string.IsNullOrEmpty(b.Key))
                        .Take(3)
                        .Select(b => b.Key)
                        .ToList();
                }

                var lastSearch = response.Aggregations.Max("last_search");
                if (lastSearch?.Value.HasValue == true)
                {
                    preferences.LastSearchDate = DateTimeOffset.FromUnixTimeMilliseconds((long)lastSearch.Value.Value).DateTime;
                }
                else
                {
                    preferences.LastSearchDate = DateTime.MinValue;
                }
            }

            return preferences;
        }
    }
}