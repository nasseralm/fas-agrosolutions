using FCG.API.Extensions;
using FCG.Application.DTOs;
using FCG.Application.Interfaces;
using FCG.Domain.DTOs;
using FCG.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System.Diagnostics;
using System.Security.Claims;

namespace FCG.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class JogoController : BaseController
    {
        private readonly IJogoService _jogoService;
        private readonly IElasticsearchService _elasticsearchService;
        private readonly ILogger<JogoController> _logger;

        public JogoController(IJogoService jogoService, IElasticsearchService elasticsearchService, ILogger<JogoController> logger)
        {
            _jogoService = jogoService;
            _elasticsearchService = elasticsearchService;
            _logger = logger;
        }

        [HttpPost("Incluir")]
        public async Task<IActionResult> Incluir(JogoDTO jogoDTO)
        {
            using var activity = OpenTelemetryExtensions.StartActivity("JogoController.Incluir", ActivityKind.Server);
            
            var userId = User.FindFirst("id")?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            activity?.EnrichWithBusinessContext("Jogo", "Create");
            activity?.EnrichWithUserContext(userId, userRole);
            activity?.SetTag("jogo.nome", jogoDTO.Nome);

            _logger.LogInformation("Iniciando inclusão do jogo. Nome: {Nome}", jogoDTO.Nome);
            
            try
            {
                var result = await _jogoService.Incluir(jogoDTO);
                
                if (result.Result != null)
                {
                    activity?.SetTag("jogo.id", result.Result.Id);
                    activity?.SetTag("operation.success", true);
                    _logger.LogInformation("Jogo incluído com sucesso! Id: {Id}", result.Result.Id);
                }
                else
                {
                    activity?.SetTag("operation.success", false);
                    activity?.SetTag("operation.errors", string.Join(", ", result.Notifications));
                }

                return CreateIActionResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao incluir jogo. Nome: {Nome}", jogoDTO.Nome);
                activity?.RecordException(ex);
                throw;
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("Alterar")]
        public async Task<IActionResult> Alterar(JogoDTO jogoDTO)
        {
            using var activity = OpenTelemetryExtensions.StartActivity("JogoController.Alterar", ActivityKind.Server);
            
            var userId = User.FindFirst("id")?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            activity?.EnrichWithBusinessContext("Jogo", "Update", jogoDTO.Id);
            activity?.EnrichWithUserContext(userId, userRole);
            activity?.SetTag("jogo.nome", jogoDTO.Nome);

            _logger.LogInformation("Tentativa de alteração de jogo. Id: {Id}, Nome: {Nome}", jogoDTO.Id, jogoDTO.Nome);
            
            try
            {
                var result = await _jogoService.Alterar(jogoDTO);
                
                if (result.Result != null)
                {
                    activity?.SetTag("operation.success", true);
                    _logger.LogInformation("Jogo alterado com sucesso. Id: {Id}, Nome: {Nome}", jogoDTO.Id, jogoDTO.Nome);
                }
                else
                {
                    activity?.SetTag("operation.success", false);
                    activity?.SetTag("operation.errors", string.Join(", ", result.Notifications));
                }

                return CreateIActionResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar jogo. Id: {Id}, Nome: {Nome}", jogoDTO.Id, jogoDTO.Nome);
                activity?.RecordException(ex);
                throw;
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("Excluir")]
        public async Task<IActionResult> Excluir(int id)
        {
            using var activity = OpenTelemetryExtensions.StartActivity("JogoController.Excluir", ActivityKind.Server);
            
            var userId = User.FindFirst("id")?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            activity?.EnrichWithBusinessContext("Jogo", "Delete", id);
            activity?.EnrichWithUserContext(userId, userRole);

            _logger.LogInformation("Tentativa de exclusão de jogo. Id: {Id}", id);
            
            try
            {
                var result = await _jogoService.Excluir(id);
                
                if (result.Result != null)
                {
                    activity?.SetTag("operation.success", true);
                    _logger.LogInformation("Jogo excluído com sucesso. Id: {Id}", id);
                }
                else
                {
                    activity?.SetTag("operation.success", false);
                    activity?.SetTag("operation.errors", string.Join(", ", result.Notifications));
                }

                return CreateIActionResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir jogo. Id: {Id}", id);
                activity?.RecordException(ex);
                throw;
            }
        }

        [HttpGet("SelecionarPorId")]
        public async Task<IActionResult> Selecionar(int id)
        {
            using var activity = OpenTelemetryExtensions.StartActivity("JogoController.Selecionar", ActivityKind.Server);
            
            var userId = User.FindFirst("id")?.Value;
            
            activity?.EnrichWithBusinessContext("Jogo", "Read", id);
            activity?.EnrichWithUserContext(userId);

            _logger.LogInformation("Consulta de jogo por Id. Id: {Id}", id);
            
            try
            {
                var result = await _jogoService.Selecionar(id);
                
                if (result.Result != null)
                {
                    activity?.SetTag("operation.success", true);
                    activity?.SetTag("jogo.nome", result.Result.Nome);
                }
                else
                {
                    activity?.SetTag("operation.success", false);
                    activity?.SetTag("operation.errors", string.Join(", ", result.Notifications));
                }

                _logger.LogInformation("Consulta de jogo por Id finalizada. Id: {Id}", id);
                return CreateIActionResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar jogo por Id. Id: {Id}", id);
                activity?.RecordException(ex);
                throw;
            }
        }

        [HttpGet("SelecionarPorNome")]
        public async Task<IActionResult> SelecionarPorNome([FromQuery] string nome)
        {
            using var activity = OpenTelemetryExtensions.StartActivity("JogoController.SelecionarPorNome", ActivityKind.Server);
            
            var userId = User.FindFirst("id")?.Value;
            
            activity?.EnrichWithBusinessContext("Jogo", "Read");
            activity?.EnrichWithUserContext(userId);
            activity?.SetTag("jogo.nome", nome);

            _logger.LogInformation("Consulta de jogo por nome. Nome: {Nome}", nome);
            
            try
            {
                var result = await _jogoService.SelecionarPorNome(nome);
                
                if (result.Result != null)
                {
                    activity?.SetTag("operation.success", true);
                    activity?.SetTag("jogo.id", result.Result.Id);
                }
                else
                {
                    activity?.SetTag("operation.success", false);
                    activity?.SetTag("operation.errors", string.Join(", ", result.Notifications));
                }

                _logger.LogInformation("Consulta de jogo por nome finalizada. Nome: {Nome}", nome);
                return CreateIActionResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar jogo por nome. Nome: {Nome}", nome);
                activity?.RecordException(ex);
                throw;
            }
        }
 
        [HttpGet("SelecionarTodos")]
        public async Task<IActionResult> SelecionarTodos()
        {
            _logger.LogInformation("Obtendo todos os jogos - {Hora}", DateTime.UtcNow.Hour.ToString());
            var result = await _jogoService.SelecionarTodos();   
            return CreateIActionResult(result);
        }
    }
}
