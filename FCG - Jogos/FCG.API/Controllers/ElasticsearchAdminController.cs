using FCG.Infra.Data.Elasticsearch.Interfaces;
using FCG.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class ElasticsearchAdminController : ControllerBase
    {
        private readonly IIndexManagementComponent _indexManagement;
        private readonly IUserTrackingComponent _userTracking;
        private readonly IJogoEnhancedService _jogoEnhancedService;
        private readonly ILogger<ElasticsearchAdminController> _logger;

        public ElasticsearchAdminController(
            IIndexManagementComponent indexManagement,
            IUserTrackingComponent userTracking,
            IJogoEnhancedService jogoEnhancedService,
            ILogger<ElasticsearchAdminController> logger)
        {
            _indexManagement = indexManagement;
            _userTracking = userTracking;
            _jogoEnhancedService = jogoEnhancedService;
            _logger = logger;
        }

        [HttpPost("SyncElasticSearchWithBase")]
        public async Task<IActionResult> SyncElasticSearchWithBase()
        {
            _logger.LogInformation("Sincronização do Elasticsearch com a base de dados solicitada");
            
            try
            {
                var result = await _jogoEnhancedService.SyncElasticSearchWithBaseAsync();
                _logger.LogInformation("Sincronização do Elasticsearch com a base de dados concluída");
                
                return Ok(new { 
                    success = result.HasResult,
                    data = result.Result,
                    notifications = result.Notifications
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na sincronização do Elasticsearch com a base de dados");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Erro na sincronização do Elasticsearch com a base de dados",
                    error = ex.Message
                });
            }
        }

        [HttpPost("CreateAllIndices")]
        public async Task<IActionResult> CreateAllIndices()
        {
            _logger.LogInformation("Criação de todos os índices solicitada");
            
            try
            {
                var success = await _indexManagement.CreateAllIndicesAsync();
                
                if (success)
                {
                    _logger.LogInformation("Todos os índices foram criados com sucesso");
                    return Ok(new { 
                        success = true,
                        message = "Todos os índices foram criados com sucesso",
                        indices = new[] { "fcg-games", "fcg-games-user-search-history" },
                        timestamp = DateTime.UtcNow
                    });
                }
                
                _logger.LogWarning("Falha na criação de alguns índices");
                return BadRequest(new { 
                    success = false,
                    message = "Falha na criação de alguns índices"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na criação dos índices");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Erro na criação dos índices",
                    error = ex.Message
                });
            }
        }

        [HttpGet("HealthCheck")]
        public async Task<IActionResult> HealthCheck()
        {
            _logger.LogInformation("Health check dos índices Elasticsearch solicitado");
            
            try
            {
                var jogosExists = await _indexManagement.IndexExistsAsync("fcg-games");
                var userSearchExists = await _indexManagement.IndexExistsAsync("fcg-games-user-search-history");
                
                var health = new
                {
                    elasticsearch = "connected",
                    indices = new
                    {
                        jogos = jogosExists ? "healthy" : "missing",
                        userSearchHistory = userSearchExists ? "healthy" : "missing"
                    },
                    status = (jogosExists && userSearchExists) ? "healthy" : "degraded",
                    timestamp = DateTime.UtcNow
                };
                
                _logger.LogInformation("Health check concluído - Status: {Status}", health.status);
                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no health check");
                return StatusCode(500, new { 
                    elasticsearch = "error", 
                    status = "unhealthy", 
                    error = ex.Message,
                    timestamp = DateTime.UtcNow 
                });
            }
        }

        [HttpDelete("DeleteAllIndices")]
        public async Task<IActionResult> DeleteAllIndices()
        {
            _logger.LogWarning("?? LIMPEZA COMPLETA - Deletando TODOS os índices e dados!");
            
            try
            {
                var deleteResults = new List<string>();
                
                _logger.LogInformation("Deletando índice de jogos...");
                var jogosDeleted = await _indexManagement.DeleteJogosIndexAsync();
                deleteResults.Add($"Jogos: {(jogosDeleted ? "Deletado" : "Falha")}");
                
                _logger.LogInformation("Deletando índice de tracking de usuários...");
                var userSearchDeleted = await _indexManagement.DeleteUserSearchIndexAsync();
                deleteResults.Add($"UserTracking: {(userSearchDeleted ? "Deletado" : "Falha")}");
                
                _logger.LogInformation("Aguardando 3 segundos para garantir remoção completa...");
                await Task.Delay(3000);
                
                _logger.LogInformation("Recriando todos os índices...");
                var allRecreated = await _indexManagement.CreateAllIndicesAsync();
                deleteResults.Add($"Todos os índices recriados: {(allRecreated ? "Sucesso" : "Falha")}");
                
                var allSuccess = jogosDeleted && userSearchDeleted && allRecreated;
                
                if (allSuccess)
                {
                    _logger.LogInformation("LIMPEZA COMPLETA realizada com sucesso!");
                    return Ok(new { 
                        success = true, 
                        message = "LIMPEZA COMPLETA realizada com sucesso! Todos os dados foram removidos e índices recriados com mapeamento correto.",
                        results = deleteResults,
                        warning = "TODOS OS DADOS FORAM PERDIDOS - índices recriados do zero",
                        nextSteps = new[] { 
                            "Execute SyncElasticSearchWithBase para repovoar os dados de jogos"
                        }
                    });
                }
                
                _logger.LogWarning("Limpeza concluída com alguns problemas");
                return BadRequest(new { 
                    success = false, 
                    message = "Limpeza concluída com alguns problemas",
                    results = deleteResults
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante limpeza completa");
                return StatusCode(500, new { success = false, message = "Erro durante limpeza completa", error = ex.Message });
            }
        }

        [HttpGet("IndexStatus")]
        public async Task<IActionResult> GetIndexStatus()
        {
            _logger.LogInformation("Solicitação de status dos índices");
            
            try
            {
                var info = await _indexManagement.GetAllIndicesInfoAsync();
                
                _logger.LogInformation("Status dos índices obtido com sucesso");
                return Ok(new { success = true, data = info });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter status dos índices");
                return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
            }
        }
    }
}