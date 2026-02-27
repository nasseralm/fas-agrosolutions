using FCG.Domain.Entities;
using FCG.Domain.DTOs;
using FCG.Domain.Interfaces;
using FCG.Infra.Data.Elasticsearch.Components.Base;
using FCG.Infra.Data.Elasticsearch.Configuration;
using FCG.Infra.Data.Elasticsearch.Interfaces;
using FCG.Infra.Data.Elasticsearch.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

namespace FCG.Infra.Data.Elasticsearch.Components
{
    public class JogoCrudComponent : ElasticsearchComponentBase, IJogoCrudComponent
    {
        public override string ComponentName => "JogoCrud";

        public JogoCrudComponent(
            IElasticClient elasticClient,
            ILogger<JogoCrudComponent> logger,
            IOptions<ElasticsearchSettings> settings)
            : base(elasticClient, logger, settings)
        {
        }

        public async Task<bool> IndexJogoAsync(Jogo jogo)
        {
            try
            {
                LogOperation("IndexJogo", new { JogoId = jogo.Id, JogoNome = jogo.Nome });

                var jogoDocument = MapToDocument(jogo);
                
                var response = await ElasticClient.IndexAsync(jogoDocument, idx => idx
                    .Index(DefaultIndex)
                    .Id(jogo.Id));

                if (response.IsValid)
                {
                    LogOperation("IndexJogo - Sucesso", new { JogoId = jogo.Id });
                    return true;
                }

                LogError("IndexJogo", new Exception(response.OriginalException?.Message ?? response.ServerError?.ToString()), 
                    new { JogoId = jogo.Id });
                return false;
            }
            catch (Exception ex)
            {
                LogError("IndexJogo", ex, new { JogoId = jogo.Id });
                return false;
            }
        }

        public async Task<bool> UpdateJogoAsync(Jogo jogo)
        {
            try
            {
                LogOperation("UpdateJogo", new { JogoId = jogo.Id, JogoNome = jogo.Nome });

                var jogoDocument = MapToDocument(jogo);
                
                var response = await ElasticClient.UpdateAsync<JogoDocument>(jogo.Id, u => u
                    .Index(DefaultIndex)
                    .Doc(jogoDocument));

                if (response.IsValid)
                {
                    LogOperation("UpdateJogo - Sucesso", new { JogoId = jogo.Id });
                    return true;
                }

                LogError("UpdateJogo", new Exception(response.OriginalException?.Message ?? response.ServerError?.ToString()), 
                    new { JogoId = jogo.Id });
                return false;
            }
            catch (Exception ex)
            {
                LogError("UpdateJogo", ex, new { JogoId = jogo.Id });
                return false;
            }
        }

        public async Task<bool> DeleteJogoAsync(int jogoId)
        {
            try
            {
                LogOperation("DeleteJogo", new { JogoId = jogoId });

                var response = await ElasticClient.DeleteAsync<JogoDocument>(jogoId, d => d
                    .Index(DefaultIndex));

                if (response.IsValid)
                {
                    LogOperation("DeleteJogo - Sucesso", new { JogoId = jogoId });
                    return true;
                }

                LogError("DeleteJogo", new Exception(response.OriginalException?.Message ?? response.ServerError?.ToString()), 
                    new { JogoId = jogoId });
                return false;
            }
            catch (Exception ex)
            {
                LogError("DeleteJogo", ex, new { JogoId = jogoId });
                return false;
            }
        }

        public async Task<bool> BulkIndexJogosAsync(IEnumerable<Jogo> jogos)
        {
            try
            {
                var jogosList = jogos.ToList();
                LogOperation("BulkIndexJogos", new { Count = jogosList.Count });

                var jogoDocuments = jogosList.Select(MapToDocument);
                
                var response = await ElasticClient.BulkAsync(b => b
                    .Index(DefaultIndex)
                    .IndexMany(jogoDocuments, (bd, jogo) => bd.Id(jogo.Id)));

                if (response.IsValid && !response.Errors)
                {
                    LogOperation("BulkIndexJogos - Sucesso", new { Count = jogosList.Count });
                    return true;
                }

                LogError("BulkIndexJogos", new Exception($"Erros: {response.Errors}, Detalhes: {response.OriginalException?.Message ?? response.ServerError?.ToString()}"), 
                    new { Count = jogosList.Count });
                return false;
            }
            catch (Exception ex)
            {
                LogError("BulkIndexJogos", ex, new { Count = jogos.Count() });
                return false;
            }
        }

        public async Task<SyncReportResult> SyncJogosWithDetailedReportAsync(IEnumerable<Jogo> jogos)
        {
            var report = new SyncReportResult();
            var jogosList = jogos.ToList();
            report.TotalJogos = jogosList.Count;

            LogOperation("SyncJogosWithDetailedReport", new { Count = jogosList.Count });

            try
            {
                foreach (var jogo in jogosList)
                {
                    var itemResult = new SyncItemDetail
                    {
                        JogoId = jogo.Id,
                        JogoNome = jogo.Nome
                    };

                    try
                    {
                        var validationError = ValidateJogoForIndexing(jogo);
                        if (!string.IsNullOrEmpty(validationError))
                        {
                            itemResult.Sucesso = false;
                            itemResult.MensagemErro = $"Validação falhou: {validationError}";
                            report.JogosFalha++;
                            report.DetalhesItens.Add(itemResult);
                            continue;
                        }

                        var jogoDocument = MapToDocument(jogo);
                        
                        var response = await ElasticClient.IndexAsync(jogoDocument, idx => idx
                            .Index(DefaultIndex)
                            .Id(jogo.Id));

                        if (response.IsValid)
                        {
                            itemResult.Sucesso = true;
                            report.JogosSucesso++;
                        }
                        else
                        {
                            itemResult.Sucesso = false;
                            itemResult.MensagemErro = response.OriginalException?.Message ?? 
                                                     response.ServerError?.ToString() ?? 
                                                     "Erro desconhecido na indexação";
                            report.JogosFalha++;
                        }
                    }
                    catch (Exception ex)
                    {
                        itemResult.Sucesso = false;
                        itemResult.MensagemErro = $"Exceção: {ex.Message}";
                        report.JogosFalha++;
                    }

                    report.DetalhesItens.Add(itemResult);
                    await Task.Delay(50);
                }
            }
            catch (Exception ex)
            {
                report.ErrosGerais.Add($"Erro geral na sincronização: {ex.Message}");
                LogError("SyncJogosWithDetailedReport", ex, new { Count = jogosList.Count });
            }

            LogOperation("SyncJogosWithDetailedReport - Concluído", 
                new { Sucessos = report.JogosSucesso, Falhas = report.JogosFalha, Total = report.TotalJogos });

            return report;
        }

        private string ValidateJogoForIndexing(Jogo jogo)
        {
            if (jogo == null) return "Jogo é nulo";
            if (jogo.Id <= 0) return "ID do jogo deve ser maior que zero";
            if (string.IsNullOrWhiteSpace(jogo.Nome)) return "Nome do jogo é obrigatório";
            if (string.IsNullOrWhiteSpace(jogo.Descricao)) return "Descrição do jogo é obrigatória";
            if (string.IsNullOrWhiteSpace(jogo.Genero)) return "Gênero do jogo é obrigatório";
            if (jogo.Preco < 0) return "Preço não pode ser negativo";
            if (string.IsNullOrWhiteSpace(jogo.Desenvolvedor)) return "Desenvolvedor é obrigatório";
            if (string.IsNullOrWhiteSpace(jogo.Distribuidora)) return "Distribuidora é obrigatória";
            if (string.IsNullOrWhiteSpace(jogo.ClassificacaoIndicativa)) return "Classificação indicativa é obrigatória";
            if (string.IsNullOrWhiteSpace(jogo.Plataforma)) return "Plataforma é obrigatória";
            if (jogo.DataLancamento == default) return "Data de lançamento é obrigatória";

            return null;
        }

        private JogoDocument MapToDocument(Jogo jogo)
        {
            return new JogoDocument
            {
                Id = jogo.Id,
                Nome = jogo.Nome,
                Descricao = jogo.Descricao,
                Genero = jogo.Genero,
                Preco = jogo.Preco,
                DataLancamento = jogo.DataLancamento,
                Desenvolvedor = jogo.Desenvolvedor,
                Distribuidora = jogo.Distribuidora,
                ClassificacaoIndicativa = jogo.ClassificacaoIndicativa,
                Plataforma = jogo.Plataforma,
                IndexedAt = DateTime.UtcNow
            };
        }
    }
}