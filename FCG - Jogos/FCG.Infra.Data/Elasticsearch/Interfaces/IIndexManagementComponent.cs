namespace FCG.Infra.Data.Elasticsearch.Interfaces
{
    /// <summary>
    /// Interface para componente de gerenciamento de índices
    /// </summary>
    public interface IIndexManagementComponent : IElasticsearchComponent
    {
        Task<bool> CreateJogosIndexAsync();
        Task<bool> CreateUserSearchIndexAsync();
        Task<bool> CreatePopularGamesIndexAsync();
        Task<bool> CreateAllIndicesAsync();
        Task<bool> DeleteJogosIndexAsync();
        Task<bool> DeleteUserSearchIndexAsync();
        Task<bool> DeletePopularGamesIndexAsync();
        Task<bool> IndexExistsAsync(string indexName);
        Task<Dictionary<string, object>> GetIndexInfoAsync(string indexName);
        Task<Dictionary<string, object>> GetAllIndicesInfoAsync();
    }
}