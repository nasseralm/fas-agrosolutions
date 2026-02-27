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
    public class JogoSearchComponent : ElasticsearchComponentBase, IJogoSearchComponent
    {
        public override string ComponentName => "JogoSearch";

        public JogoSearchComponent(
            IElasticClient elasticClient,
            ILogger<JogoSearchComponent> logger,
            IOptions<ElasticsearchSettings> settings)
            : base(elasticClient, logger, settings)
        {
        }

        public async Task<IEnumerable<JogoElasticsearchResponse>> SearchJogosAsync(string searchTerm, int from = 0, int size = 10)
        {
            try
            {
                LogOperation("SearchJogos", new { SearchTerm = searchTerm, From = from, Size = size });

                var response = await ElasticClient.SearchAsync<JogoDocument>(s => s
                    .Index(DefaultIndex)
                    .From(from)
                    .Size(size)
                    .Query(q => q
                        .MultiMatch(m => m
                            .Fields(f => f
                                .Field(p => p.Nome, 2.0)
                                .Field(p => p.Descricao, 1.0)
                                .Field(p => p.Desenvolvedor, 1.5)
                                .Field(p => p.Distribuidora, 1.0))
                            .Query(searchTerm)
                            .Type(TextQueryType.BestFields)
                            .Fuzziness(Fuzziness.Auto)))
                    .Sort(sort => sort
                        .Descending(SortSpecialField.Score)
                        .Descending(f => f.DataLancamento)));

                if (response.IsValid)
                {
                    LogOperation("SearchJogos - Sucesso", new { SearchTerm = searchTerm, ResultCount = response.Documents.Count });
                    return response.Documents.Select(MapToElasticsearchResponse);
                }

                LogError("SearchJogos", new Exception(response.OriginalException?.Message ?? response.ServerError?.ToString()), 
                    new { SearchTerm = searchTerm });
                return Enumerable.Empty<JogoElasticsearchResponse>();
            }
            catch (Exception ex)
            {
                LogError("SearchJogos", ex, new { SearchTerm = searchTerm });
                return Enumerable.Empty<JogoElasticsearchResponse>();
            }
        }

        private JogoElasticsearchResponse MapToElasticsearchResponse(JogoDocument document)
        {
            return new JogoElasticsearchResponse
            {
                Id = document.Id,
                Nome = document.Nome,
                Descricao = document.Descricao,
                Genero = document.Genero,
                Preco = document.Preco,
                DataLancamento = document.DataLancamento,
                Desenvolvedor = document.Desenvolvedor,
                Distribuidora = document.Distribuidora,
                ClassificacaoIndicativa = document.ClassificacaoIndicativa,
                Plataforma = document.Plataforma
            };
        }
    }
}