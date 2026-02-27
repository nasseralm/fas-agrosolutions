using FCG.Domain.Entities;
using FCG.Domain.DTOs;
using FCG.Domain.Interfaces;

namespace FCG.Infra.Data.Elasticsearch.Interfaces
{
    /// <summary>
    /// Interface para componente de operações CRUD de jogos
    /// </summary>
    public interface IJogoCrudComponent : IElasticsearchComponent
    {
        Task<bool> IndexJogoAsync(Jogo jogo);
        Task<bool> UpdateJogoAsync(Jogo jogo);
        Task<bool> DeleteJogoAsync(int jogoId);
        Task<bool> BulkIndexJogosAsync(IEnumerable<Jogo> jogos);
        Task<SyncReportResult> SyncJogosWithDetailedReportAsync(IEnumerable<Jogo> jogos);
    }
}