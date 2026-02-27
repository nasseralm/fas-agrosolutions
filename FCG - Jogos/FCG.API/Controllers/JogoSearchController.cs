using FCG.API.Controllers;
using FCG.Application.Interfaces;
using FCG.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class JogoSearchController : BaseController
    {
        private readonly IJogoEnhancedService _jogoEnhancedService;
        private readonly IElasticsearchService _elasticsearchService;
        private readonly ILogger<JogoSearchController> _logger;

        public JogoSearchController(IJogoEnhancedService jogoEnhancedService, IElasticsearchService elasticsearchService, ILogger<JogoSearchController> logger)
        {
            _jogoEnhancedService = jogoEnhancedService;
            _elasticsearchService = elasticsearchService;
            _logger = logger;
        }

        [HttpGet("Search")]
        public async Task<IActionResult> SearchJogos([FromQuery] string searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? usuarioId = null)
        {
            _logger.LogInformation("Busca de jogos solicitada: {SearchTerm}, Página: {Page}", searchTerm, page);
            
            try
            {
                var currentUserId = usuarioId ?? GetCurrentUserId();
                _logger.LogInformation("Usuário identificado: {UsuarioId}", currentUserId);
                
                var result = await _jogoEnhancedService.SearchJogosAsync(searchTerm, page, pageSize, currentUserId);

                if (currentUserId.HasValue && result.HasResult && !string.IsNullOrWhiteSpace(searchTerm))
                {
                    _logger.LogInformation("Iniciando tracking para usuário {UsuarioId} - termo: {SearchTerm}", currentUserId.Value, searchTerm);
                    
                    var sessionId = GetSessionId();
                    var resultsList = result.Result.ToList();
                    var resultCount = resultsList.Count;
                    
                    var foundGenres = resultsList.Select(j => j.Genero).Where(g => !string.IsNullOrEmpty(g)).Distinct().ToList();
                    var foundDevelopers = resultsList.Select(j => j.Desenvolvedor).Where(d => !string.IsNullOrEmpty(d)).Distinct().ToList();
                    var foundPlatforms = resultsList.Select(j => j.Plataforma).Where(p => !string.IsNullOrEmpty(p)).Distinct().ToList();
                    var foundGameNames = resultsList.Select(j => j.Nome).Where(n => !string.IsNullOrEmpty(n)).Distinct().ToList();

                    _logger.LogInformation("Dados para tracking: Resultados={Count}, Gêneros={Genres}, Desenvolvedores={Developers}, Jogos={Games}, SessionId={SessionId}", 
                        resultCount, string.Join(",", foundGenres), string.Join(",", foundDevelopers), string.Join(",", foundGameNames), sessionId);

                    try
                    {
                        var trackingResult = await _elasticsearchService.TrackUserSearchAsync(
                            currentUserId.Value, 
                            searchTerm, 
                            sessionId, 
                            resultCount, 
                            foundGenres, 
                            foundDevelopers, 
                            foundPlatforms,
                            foundGameNames);

                        _logger.LogInformation("Tracking concluído: {Success} para usuário {UsuarioId} - busca: {SearchTerm}", 
                            trackingResult, currentUserId.Value, searchTerm);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro no tracking para usuário {UsuarioId} - busca: {SearchTerm}", currentUserId.Value, searchTerm);
                    }
                }
                else
                {
                    _logger.LogWarning("Tracking não executado - UsuarioId: {UserId}, HasResult: {HasResult}, SearchTerm: {SearchTerm}", 
                        currentUserId, result.HasResult, searchTerm);
                }

                _logger.LogInformation("Busca de jogos concluída: {SearchTerm} - {Count} resultados", searchTerm, 
                    result.HasResult ? result.Result.Count() : 0);
                return CreateIActionResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na busca de jogos: {SearchTerm}", searchTerm);
                throw;
            }
        }

        [HttpGet("AdvancedSearch")]
        public async Task<IActionResult> AdvancedSearch([FromQuery] int limit = 5)
        {
            _logger.LogInformation("Solicitação de busca avançada (gêneros populares). Limite: {Limit}", limit);
            
            try
            {
                if (limit <= 0 || limit > 10)
                {
                    limit = 5;
                    _logger.LogWarning("Limite ajustado para o valor padrão: {Limit}", limit);
                }

                var popularGames = await _elasticsearchService.GetTopPopularGamesAsync(limit);
                
                var result = new
                {
                    success = true,
                    data = popularGames.TopGenres,
                    totalSearches = popularGames.TotalSearches,
                    uniqueUsers = popularGames.UniqueUsers,
                    lastUpdated = popularGames.LastUpdated,
                    timestamp = DateTime.UtcNow
                };
                
                _logger.LogInformation("Busca avançada concluída com sucesso. Count: {Count}", popularGames.TopGenres.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar busca avançada");
                return StatusCode(500, new { 
                    success = false,
                    message = "Erro interno do servidor ao executar busca avançada",
                    error = ex.Message
                });
            }
        }

        [HttpGet("UserPreferences")]
        public async Task<IActionResult> GetUserPreferences([FromQuery] int? usuarioId = null)
        {
            try
            {
                var currentUserId = usuarioId ?? GetCurrentUserId();
                
                if (!currentUserId.HasValue)
                {
                    return BadRequest("ID do usuário é obrigatório. Forneça via parâmetro ou certifique-se de estar autenticado.");
                }

                _logger.LogInformation("Preferências do usuário solicitadas: {UsuarioId}", currentUserId.Value);
                
                var result = await _jogoEnhancedService.GetUserPreferencesAsync(currentUserId.Value);
                _logger.LogInformation("Preferências do usuário obtidas: {UsuarioId} - Total searches: {Total}", 
                    currentUserId.Value, result.Result?.TotalSearches ?? 0);
                return CreateIActionResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter preferências do usuário: {UsuarioId}", usuarioId);
                throw;
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }

        private string GetSessionId()
        {
            try
            {
                var jti = User.FindFirst("jti")?.Value;
                if (!string.IsNullOrEmpty(jti))
                {
                    return jti;
                }
                
                return HttpContext.Session?.Id ?? Guid.NewGuid().ToString();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao extrair session ID");
                return Guid.NewGuid().ToString();
            }
        }
    }
}