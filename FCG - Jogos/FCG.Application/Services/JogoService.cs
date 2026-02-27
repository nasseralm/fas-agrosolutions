using AutoMapper;
using FCG.API.Models;
using FCG.Application.DTOs;
using FCG.Application.Interfaces;
using FCG.Domain.Interfaces;
using FCG.Domain.Notifications;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FCG.Application.Services
{
    public class JogoService : IJogoService
    {
        private readonly IJogoRepository _jogoRepository;
        private readonly ILogger<JogoService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IElasticsearchService _elasticsearchService;
        private static readonly ActivitySource ActivitySource = new("FCG.Application");

        public JogoService(IJogoRepository jogoRepository, ILogger<JogoService> logger,
            IUnitOfWork unitOfWork, IMapper mapper, IElasticsearchService elasticsearchService)
        {
            _jogoRepository = jogoRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _elasticsearchService = elasticsearchService;
        }

        public async Task<DomainNotificationsResult<JogoViewModel>> Incluir(JogoDTO jogoDTO)
        {
            using var activity = ActivitySource.StartActivity("JogoService.Incluir");
            activity?.SetTag("jogo.nome", jogoDTO.Nome);
            activity?.SetTag("operation.type", "create");

            var resultNotifications = new DomainNotificationsResult<JogoViewModel>();

            _logger.LogInformation("Iniciando inclusão de jogo: {nome}", jogoDTO.Nome);

            try
            {
                var jogoRecuperado = await _jogoRepository.SelecionarPorNome(jogoDTO.Nome);

                if (jogoRecuperado != null)
                {
                    _logger.LogWarning("Tentativa de inclusão de jogo já existente: {nome}", jogoDTO.Nome);
                    activity?.SetTag("result", "duplicate");
                    resultNotifications.Notifications.Add("O jogo já existe no banco de dados!");
                    return resultNotifications;
                }

                var jogo = _mapper.Map<Domain.Entities.Jogo>(jogoDTO);

                await _jogoRepository.Incluir(jogo);
                await _unitOfWork.Commit();

                activity?.SetTag("jogo.id", jogo.Id);
                activity?.SetTag("result", "success");

                var indexSuccess = await _elasticsearchService.IndexJogoAsync(jogo);
                if (!indexSuccess)
                {
                    _logger.LogWarning("Falha ao indexar jogo no Elasticsearch: {nome}", jogoDTO.Nome);
                    activity?.SetTag("elasticsearch.indexed", false);
                }
                else
                {
                    activity?.SetTag("elasticsearch.indexed", true);
                }

                resultNotifications.Result = _mapper.Map<JogoViewModel>(jogo);

                _logger.LogInformation("Jogo incluído com sucesso: {nome}", jogoDTO.Nome);
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao incluir jogo: {nome}", jogoDTO.Nome);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("exception.type", ex.GetType().Name);
                resultNotifications.Notifications.Add("Erro ao incluir o jogo.");
            }

            return resultNotifications;
        }

        public async Task<DomainNotificationsResult<JogoViewModel>> Alterar(JogoDTO jogoDTO)
        {
            using var activity = ActivitySource.StartActivity("JogoService.Alterar");
            activity?.SetTag("jogo.id", jogoDTO.Id);
            activity?.SetTag("jogo.nome", jogoDTO.Nome);
            activity?.SetTag("operation.type", "update");

            var resultNotifications = new DomainNotificationsResult<JogoViewModel>();

            _logger.LogInformation("Iniciando alteração de jogo: {id} - {nome}", jogoDTO.Id, jogoDTO.Nome);

            try
            {
                var jogo = await _jogoRepository.Selecionar(jogoDTO.Id);

                if (jogo == null)
                {
                    _logger.LogWarning("Jogo não encontrado para alteração: {id}", jogoDTO.Id);
                    activity?.SetTag("result", "not_found");
                    resultNotifications.Notifications.Add("Jogo não encontrado.");
                    return resultNotifications;
                }

                _mapper.Map(jogoDTO, jogo);

                _jogoRepository.Alterar(jogo);
                await _unitOfWork.Commit();

                resultNotifications.Result = _mapper.Map<JogoViewModel>(jogo);
                activity?.SetTag("result", "success");

                _logger.LogInformation("Jogo alterado com sucesso: {id} - {nome}", jogoDTO.Id, jogoDTO.Nome);
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar jogo: {id} - {nome}", jogoDTO.Id, jogoDTO.Nome);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("exception.type", ex.GetType().Name);
                resultNotifications.Notifications.Add("Erro ao alterar o jogo.");
            }

            return resultNotifications;
        }

        public async Task<DomainNotificationsResult<JogoViewModel>> Excluir(int id)
        {
            using var activity = ActivitySource.StartActivity("JogoService.Excluir");
            activity?.SetTag("jogo.id", id);
            activity?.SetTag("operation.type", "delete");

            var resultNotifications = new DomainNotificationsResult<JogoViewModel>();

            _logger.LogInformation("Iniciando exclusão de jogo: {id}", id);

            try
            {
                var jogo = await _jogoRepository.Excluir(id);
                if (jogo == null)
                {
                    _logger.LogWarning("Jogo não encontrado para exclusão: {id}", id);
                    activity?.SetTag("result", "not_found");
                    resultNotifications.Notifications.Add("Jogo não encontrado.");
                    return resultNotifications;
                }

                await _unitOfWork.Commit();

                resultNotifications.Result = _mapper.Map<JogoViewModel>(jogo);
                activity?.SetTag("result", "success");

                _logger.LogInformation("Jogo excluído com sucesso: {id}", id);
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir jogo: {id}", id);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("exception.type", ex.GetType().Name);
                resultNotifications.Add("Erro ao excluir o jogo.");
            }

            return resultNotifications;
        }

        public async Task<DomainNotificationsResult<JogoViewModel>> Selecionar(int id)
        {
            using var activity = ActivitySource.StartActivity("JogoService.Selecionar");
            activity?.SetTag("jogo.id", id);
            activity?.SetTag("operation.type", "read");

            var resultNotifications = new DomainNotificationsResult<JogoViewModel>();

            _logger.LogInformation("Selecionando jogo por ID: {id}", id);

            try
            {
                var jogo = await _jogoRepository.Selecionar(id);

                if (jogo == null)
                {
                    _logger.LogWarning("Jogo não encontrado ao selecionar por ID: {id}", id);
                    activity?.SetTag("result", "not_found");
                    resultNotifications.Notifications.Add($"Jogo com o ID {id} não encontrado.");
                    return resultNotifications;
                }

                resultNotifications.Result = _mapper.Map<JogoViewModel>(jogo);
                activity?.SetTag("result", "success");
                activity?.SetTag("jogo.nome", jogo.Nome);

                _logger.LogInformation("Jogo selecionado com sucesso: {id}", id);
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao selecionar jogo por ID: {id}", id);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("exception.type", ex.GetType().Name);
                resultNotifications.Notifications.Add("Erro ao buscar o jogo.");
            }

            return resultNotifications;
        }

        public async Task<DomainNotificationsResult<JogoViewModel>> SelecionarPorNome(string nome)
        {
            using var activity = ActivitySource.StartActivity("JogoService.SelecionarPorNome");
            activity?.SetTag("jogo.nome", nome);
            activity?.SetTag("operation.type", "read");

            var resultNotifications = new DomainNotificationsResult<JogoViewModel>();

            _logger.LogInformation("Selecionando jogo por nome: {nome}", nome);

            try
            {
                if (!string.IsNullOrWhiteSpace(nome))
                {
                    var jogoPorNome = await _jogoRepository.SelecionarPorNome(nome);
                    if (jogoPorNome != null)
                    {
                        _logger.LogInformation("Jogo encontrado por nome: {nome}", nome);
                        activity?.SetTag("result", "success");
                        activity?.SetTag("jogo.id", jogoPorNome.Id);
                        resultNotifications.Result = _mapper.Map<JogoViewModel>(jogoPorNome);
                        activity?.SetStatus(ActivityStatusCode.Ok);
                        return resultNotifications;
                    }
                }

                _logger.LogWarning("Jogo não encontrado com o nome fornecido: {nome}", nome);
                activity?.SetTag("result", "not_found");
                resultNotifications.Notifications.Add("Jogo não encontrado com o nome fornecido.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao selecionar jogo por nome: {nome}", nome);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("exception.type", ex.GetType().Name);
                resultNotifications.Notifications.Add("Erro ao buscar o jogo.");
            }

            return resultNotifications;
        }

        public async Task<DomainNotificationsResult<IEnumerable<JogoViewModel>>> SelecionarTodos()
        {
            var resultNotifications = new DomainNotificationsResult<IEnumerable<JogoViewModel>>();

            try
            {
                var jogos = await _jogoRepository.SelecionarTodosAsync();

                if (!jogos.Any())
                {
                    _logger.LogInformation("Não há jogos disponíveis no momento");
                    resultNotifications.Notifications.Add("Não há jogos disponíveis no momento");
                    
                    resultNotifications.Result = Enumerable.Empty<JogoViewModel>();
                    
                    return resultNotifications;
                }

                var listaJogos = _mapper.Map<IEnumerable<JogoViewModel>>(jogos);

                resultNotifications.Result = listaJogos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao selecionar todos os jogos");
                resultNotifications.Notifications.Add("Erro ao buscar todos os jogos.");
            }

            return resultNotifications;
        }

    }
}
