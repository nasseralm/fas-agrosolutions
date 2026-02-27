using FCG.Infra.Data.Elasticsearch.Components.Base;
using FCG.Infra.Data.Elasticsearch.Configuration;
using FCG.Infra.Data.Elasticsearch.Interfaces;
using FCG.Infra.Data.Elasticsearch.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

namespace FCG.Infra.Data.Elasticsearch.Components
{
    public class IndexManagementComponent : ElasticsearchComponentBase, IIndexManagementComponent
    {
        private readonly string UserSearchIndex;
        public override string ComponentName => "IndexManagement";

        public IndexManagementComponent(
            IElasticClient elasticClient,
            ILogger<IndexManagementComponent> logger,
            IOptions<ElasticsearchSettings> settings)
            : base(elasticClient, logger, settings)
        {
            UserSearchIndex = $"{DefaultIndex}-user-search-history";
        }

        public async Task<bool> CreateJogosIndexAsync()
        {
            try
            {
                LogOperation("CreateJogosIndex", new { IndexName = DefaultIndex });

                var existsResponse = await ElasticClient.Indices.ExistsAsync(DefaultIndex);
                if (!existsResponse.IsValid)
                {
                    LogError("CreateJogosIndex", new Exception("Erro ao verificar existência do índice"), new { IndexName = DefaultIndex });
                    return false;
                }

                if (existsResponse.Exists)
                {
                    LogWarning("CreateJogosIndex", "Índice já existe", new { IndexName = DefaultIndex });
                    return true;
                }

                var createResponse = await ElasticClient.Indices.CreateAsync(DefaultIndex, c => c
                    .Settings(s => s
                        .NumberOfShards(1)
                        .NumberOfReplicas(0)
                        .Analysis(a => a
                            .Analyzers(an => an
                                .Standard("standard", std => std
                                    .StopWords("_portuguese_")))))
                    .Map<JogoDocument>(m => m
                        .AutoMap()
                        .Properties(p => p
                            .Text(t => t.Name(n => n.Nome).Analyzer("standard").Fields(f => f.Keyword(k => k.Name("keyword"))))
                            .Text(t => t.Name(n => n.Descricao).Analyzer("standard"))
                            .Text(t => t.Name(n => n.Desenvolvedor).Analyzer("standard").Fields(f => f.Keyword(k => k.Name("keyword"))))
                            .Text(t => t.Name(n => n.Distribuidora).Analyzer("standard").Fields(f => f.Keyword(k => k.Name("keyword"))))
                            .Keyword(k => k.Name(n => n.Genero))
                            .Keyword(k => k.Name(n => n.Plataforma))
                            .Keyword(k => k.Name(n => n.ClassificacaoIndicativa))
                            .Number(n => n.Name(nm => nm.Preco).Type(NumberType.ScaledFloat).ScalingFactor(100))
                            .Date(d => d.Name(n => n.DataLancamento))
                            .Date(d => d.Name(n => n.IndexedAt)))));
                if (createResponse.IsValid)
                {
                    LogOperation("CreateJogosIndex - Sucesso", new { IndexName = DefaultIndex });
                    return true;
                }

                LogError("CreateJogosIndex", new Exception(createResponse.OriginalException?.Message ?? createResponse.ServerError?.ToString()), 
                    new { IndexName = DefaultIndex });
                return false;
            }
            catch (Exception ex)
            {
                LogError("CreateJogosIndex", ex, new { IndexName = DefaultIndex });
                return false;
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
                    LogError("CreateUserSearchIndex", new Exception("Erro ao verificar existência do índice"), new { IndexName = UserSearchIndex });
                    return false;
                }

                if (existsResponse.Exists && existsResponse.ApiCall.HttpStatusCode != 404)
                {
                    LogWarning("CreateUserSearchIndex", "Índice já existe - DELETANDO para recriar com mapeamento KEYWORD correto", new { IndexName = UserSearchIndex });
                    
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
                    new { IndexName = UserSearchIndex });
                return false;
            }
            catch (Exception ex)
            {
                LogError("CreateUserSearchIndex", ex, new { IndexName = UserSearchIndex });
                return false;
            }
        }

        public async Task<bool> CreatePopularGamesIndexAsync()
        {
            LogWarning("CreatePopularGamesIndex", "PopularGames index is no longer used");
            return true;
        }

        public async Task<bool> DeletePopularGamesIndexAsync()
        {
            LogWarning("DeletePopularGamesIndex", "PopularGames index is no longer used");
            return true;
        }

        public async Task<bool> CreateAllIndicesAsync()
        {
            try
            {
                LogOperation("CreateAllIndices");

                var jogosResult = await CreateJogosIndexAsync();
                var userSearchResult = await CreateUserSearchIndexAsync();

                var success = jogosResult && userSearchResult;

                if (success)
                {
                    LogOperation("CreateAllIndices - Sucesso");
                }
                else
                {
                    LogWarning("CreateAllIndices", "Nem todos os índices foram criados com sucesso", 
                        new { JogosSuccess = jogosResult, UserSearchSuccess = userSearchResult });
                }

                return success;
            }
            catch (Exception ex)
            {
                LogError("CreateAllIndices", ex);
                return false;
            }
        }

        public async Task<bool> DeleteJogosIndexAsync()
        {
            try
            {
                LogOperation("DeleteJogosIndex", new { IndexName = DefaultIndex });

                var existsResponse = await ElasticClient.Indices.ExistsAsync(DefaultIndex);
                if (!existsResponse.IsValid)
                {
                    LogError("DeleteJogosIndex", new Exception("Erro ao verificar existência do índice"), new { IndexName = DefaultIndex });
                    return false;
                }

                if (!existsResponse.Exists)
                {
                    LogWarning("DeleteJogosIndex", "Índice não existe", new { IndexName = DefaultIndex });
                    return true;
                }

                var deleteResponse = await ElasticClient.Indices.DeleteAsync(DefaultIndex);

                if (deleteResponse.IsValid)
                {
                    LogOperation("DeleteJogosIndex - Sucesso", new { IndexName = DefaultIndex });
                    return true;
                }

                LogError("DeleteJogosIndex", new Exception(deleteResponse.OriginalException?.Message ?? deleteResponse.ServerError?.ToString()), 
                    new { IndexName = DefaultIndex });
                return false;
            }
            catch (Exception ex)
            {
                LogError("DeleteJogosIndex", ex, new { IndexName = DefaultIndex });
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

        public async Task<bool> IndexExistsAsync(string indexName)
        {
            try
            {
                LogOperation("IndexExists", new { IndexName = indexName });

                var response = await ElasticClient.Indices.ExistsAsync(indexName);

                if (response.IsValid)
                {
                    LogOperation("IndexExists - Sucesso", new { IndexName = indexName, Exists = response.Exists });
                    return response.Exists;
                }

                LogError("IndexExists", new Exception(response.OriginalException?.Message ?? response.ServerError?.ToString()), 
                    new { IndexName = indexName });
                return false;
            }
            catch (Exception ex)
            {
                LogError("IndexExists", ex, new { IndexName = indexName });
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetIndexInfoAsync(string indexName)
        {
            try
            {
                LogOperation("GetIndexInfo", new { IndexName = indexName });

                var statsResponse = await ElasticClient.Indices.StatsAsync(indexName);
                var settingsResponse = await ElasticClient.Indices.GetSettingsAsync(indexName);

                if (statsResponse.IsValid && settingsResponse.IsValid)
                {
                    var indexStats = statsResponse.Indices.FirstOrDefault().Value;
                    var indexSettings = settingsResponse.Indices.FirstOrDefault().Value;

                    var info = new Dictionary<string, object>
                    {
                        ["name"] = indexName,
                        ["status"] = "healthy",
                        ["documentsCount"] = indexStats?.Total?.Documents?.Count ?? 0,
                        ["storeSize"] = indexStats?.Total?.Store?.SizeInBytes ?? 0,
                        ["settings"] = new
                        {
                            numberOfShards = indexSettings?.Settings?.NumberOfShards,
                            numberOfReplicas = indexSettings?.Settings?.NumberOfReplicas
                        }
                    };

                    LogOperation("GetIndexInfo - Sucesso", new { IndexName = indexName });
                    return info;
                }

                LogError("GetIndexInfo", new Exception("Erro ao obter informações do índice"), new { IndexName = indexName });
                return new Dictionary<string, object>
                {
                    ["name"] = indexName,
                    ["status"] = "error"
                };
            }
            catch (Exception ex)
            {
                LogError("GetIndexInfo", ex, new { IndexName = indexName });
                return new Dictionary<string, object>
                {
                    ["name"] = indexName,
                    ["status"] = "error",
                    ["error"] = ex.Message
                };
            }
        }

        public async Task<Dictionary<string, object>> GetAllIndicesInfoAsync()
        {
            try
            {
                LogOperation("GetAllIndicesInfo");

                var jogosInfo = await GetIndexInfoAsync(DefaultIndex);
                var userSearchInfo = await GetIndexInfoAsync(UserSearchIndex);

                var healthyCount = 0;
                if (jogosInfo.ContainsKey("status") && jogosInfo["status"].ToString() == "healthy") healthyCount++;
                if (userSearchInfo.ContainsKey("status") && userSearchInfo["status"].ToString() == "healthy") healthyCount++;

                var allIndicesInfo = new Dictionary<string, object>
                {
                    ["jogos"] = jogosInfo,
                    ["userSearchHistory"] = userSearchInfo,
                    ["totalIndices"] = 2,
                    ["healthyIndices"] = healthyCount
                };

                LogOperation("GetAllIndicesInfo - Sucesso");
                return allIndicesInfo;
            }
            catch (Exception ex)
            {
                LogError("GetAllIndicesInfo", ex);
                return new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["totalIndices"] = 0,
                    ["healthyIndices"] = 0
                };
            }
        }
    }
}
