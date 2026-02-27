using AutoMapper;
using FCG.API.Models;
using FCG.Application.DTOs;
using FCG.Application.Interfaces;
using FCG.Domain.Interfaces;
using FCG.Domain.Notifications;
using FCG.Domain.DTOs;
using Microsoft.Extensions.Logging;

namespace FCG.Application.Services
{
    public class JogoEnhancedService : JogoService, IJogoEnhancedService
    {
        private readonly IElasticsearchService _elasticsearchService;
        private readonly ILogger<JogoEnhancedService> _logger;
        private readonly IMapper _mapper;
        private readonly IJogoRepository _jogoRepository;

        public JogoEnhancedService(
            IJogoRepository jogoRepository,
            ILogger<JogoEnhancedService> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IElasticsearchService elasticsearchService)
            : base(jogoRepository, logger, unitOfWork, mapper, elasticsearchService)
        {
            _elasticsearchService = elasticsearchService;
            _logger = logger;
            _mapper = mapper;
            _jogoRepository = jogoRepository;
        }

        public async Task<DomainNotificationsResult<IEnumerable<JogoElasticsearchResponse>>> SearchJogosAsync(string searchTerm, int page = 1, int pageSize = 10, int? usuarioId = null)
        {
            var resultNotifications = new DomainNotificationsResult<IEnumerable<JogoElasticsearchResponse>>();

            try
            {
                _logger.LogInformation("Realizando busca de jogos: {SearchTerm}, Página: {Page}, Usuário: {UsuarioId}", searchTerm, page, usuarioId);

                var from = (page - 1) * pageSize;
                var jogos = await _elasticsearchService.SearchJogosAsync(searchTerm, from, pageSize);
                
                resultNotifications.Result = jogos;

                _logger.LogInformation("Busca de jogos concluída: {Count} resultados", jogos.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar jogos: {SearchTerm}", searchTerm);
                resultNotifications.Notifications.Add("Erro ao realizar busca de jogos.");
            }

            return resultNotifications;
        }

        public async Task<DomainNotificationsResult<UserPreferencesDto>> GetUserPreferencesAsync(int usuarioId)
        {
            var resultNotifications = new DomainNotificationsResult<UserPreferencesDto>();

            try
            {
                _logger.LogInformation("Obtendo preferências do usuário: {UsuarioId}", usuarioId);

                var preferences = await _elasticsearchService.GetUserPreferencesAsync(usuarioId);
                resultNotifications.Result = preferences;

                _logger.LogInformation("Preferências do usuário obtidas - Total de buscas: {TotalSearches}, Top termos: {TopTerms}", 
                    preferences.TotalSearches, string.Join(", ", preferences.TopSearchTerms));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter preferências do usuário: {UsuarioId}", usuarioId);
                resultNotifications.Notifications.Add("Erro ao obter preferências do usuário.");
            }

            return resultNotifications;
        }

        public async Task<DomainNotificationsResult<SyncReportResult>> SyncElasticSearchWithBaseAsync()
        {
            var resultNotifications = new DomainNotificationsResult<SyncReportResult>();

            try
            {
                _logger.LogInformation("Iniciando sincronização do Elasticsearch com a base de dados");

                var allJogos = await _jogoRepository.SelecionarTodos();
                var jogosList = allJogos.ToList();

                if (!jogosList.Any())
                {
                    _logger.LogInformation("Nenhum jogo encontrado na base de dados para sincronização");
                    
                    var emptyReport = new SyncReportResult
                    {
                        TotalJogos = 0,
                        JogosSucesso = 0,
                        JogosFalha = 0
                    };
                    emptyReport.ErrosGerais.Add("Nenhum jogo encontrado na base de dados para sincronização.");
                    
                    resultNotifications.Result = emptyReport;
                    resultNotifications.Notifications.Add("Nenhum jogo encontrado na base de dados para sincronização.");
                    return resultNotifications;
                }

                _logger.LogInformation("Encontrados {Count} jogos para sincronização com o Elasticsearch", jogosList.Count);

                var syncReport = await _elasticsearchService.SyncJogosWithDetailedReportAsync(jogosList);

                resultNotifications.Result = syncReport;

                if (syncReport.Sucesso)
                {
                    _logger.LogInformation("Sincronização com Elasticsearch concluída com sucesso: {Sucessos}/{Total} jogos indexados", 
                        syncReport.JogosSucesso, syncReport.TotalJogos);
                    resultNotifications.Notifications.Add($"Sincronização concluída com sucesso: {syncReport.JogosSucesso}/{syncReport.TotalJogos} jogos indexados no Elasticsearch.");
                }
                else
                {
                    _logger.LogWarning("Sincronização com Elasticsearch concluída com falhas: {Sucessos}/{Total} jogos indexados, {Falhas} falhas", 
                        syncReport.JogosSucesso, syncReport.TotalJogos, syncReport.JogosFalha);
                    resultNotifications.Notifications.Add($"Sincronização concluída com falhas: {syncReport.JogosSucesso}/{syncReport.TotalJogos} jogos indexados, {syncReport.JogosFalha} falhas.");
                    
                    var primeiroErros = syncReport.DetalhesItens
                        .Where(item => !item.Sucesso)
                        .Take(5)
                        .Select(item => $"Jogo {item.JogoId} ({item.JogoNome}): {item.MensagemErro}");
                    
                    foreach (var erro in primeiroErros)
                    {
                        resultNotifications.Notifications.Add(erro);
                    }

                    if (syncReport.JogosFalha > 5)
                    {
                        resultNotifications.Notifications.Add($"E mais {syncReport.JogosFalha - 5} erros... Verifique o relatório completo.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante sincronização com Elasticsearch");
                
                var errorReport = new SyncReportResult
                {
                    TotalJogos = 0,
                    JogosSucesso = 0,
                    JogosFalha = 0
                };
                errorReport.ErrosGerais.Add($"Erro durante sincronização: {ex.Message}");
                
                resultNotifications.Result = errorReport;
                resultNotifications.Notifications.Add("Erro durante sincronização com Elasticsearch.");
            }

            return resultNotifications;
        }

        internal async Task<DomainNotificationsResult<bool>> TrackUserSearchAsync(int usuarioId, string searchTerm, string sessionId, IEnumerable<JogoElasticsearchResponse> searchResults)
        {
            var resultNotifications = new DomainNotificationsResult<bool>();

            try
            {
                _logger.LogInformation("Registrando busca do usuário: {UsuarioId} - {SearchTerm}", usuarioId, searchTerm);

                var resultsList = searchResults.ToList();
                var resultCount = resultsList.Count;
                
                var foundGenres = resultsList.Select(j => j.Genero).Where(g => !string.IsNullOrEmpty(g)).Distinct().ToList();
                var foundDevelopers = resultsList.Select(j => j.Desenvolvedor).Where(d => !string.IsNullOrEmpty(d)).Distinct().ToList();
                var foundPlatforms = resultsList.Select(j => j.Plataforma).Where(p => !string.IsNullOrEmpty(p)).Distinct().ToList();
                var foundGameNames = resultsList.Select(j => j.Nome).Where(n => !string.IsNullOrEmpty(n)).Distinct().ToList();

                var success = await _elasticsearchService.TrackUserSearchAsync(
                    usuarioId, 
                    searchTerm, 
                    sessionId, 
                    resultCount, 
                    foundGenres, 
                    foundDevelopers, 
                    foundPlatforms,
                    foundGameNames);

                resultNotifications.Result = success;

                if (success)
                {
                    _logger.LogInformation("Busca do usuário registrada com sucesso: {UsuarioId} - {SearchTerm}", usuarioId, searchTerm);
                }
                else
                {
                    _logger.LogWarning("Falha ao registrar busca do usuário: {UsuarioId} - {SearchTerm}", usuarioId, searchTerm);
                    resultNotifications.Notifications.Add("Falha ao registrar a busca do usuário.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar busca do usuário: {UsuarioId} - {SearchTerm}", usuarioId, searchTerm);
                resultNotifications.Notifications.Add("Erro ao registrar a busca do usuário.");
                resultNotifications.Result = false;
            }

            return resultNotifications;
        }
    }
}