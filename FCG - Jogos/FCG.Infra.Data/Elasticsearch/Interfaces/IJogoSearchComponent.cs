using FCG.Domain.DTOs;

namespace FCG.Infra.Data.Elasticsearch.Interfaces
{
    /// <summary>
    /// Interface para componente de busca de jogos
    /// </summary>
    public interface IJogoSearchComponent : IElasticsearchComponent
    {
        Task<IEnumerable<JogoElasticsearchResponse>> SearchJogosAsync(string searchTerm, int from = 0, int size = 10);
    }
}