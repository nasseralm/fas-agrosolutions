using FCG.Application.DTOs;
using FCG.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FCG.API.Extensions;
using System.Diagnostics;
using System.Security.Claims;

namespace FCG.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CompraController : BaseController
    {
        private readonly ICompraService _compraService;
        private readonly ILogger<CompraController> _logger;

        public CompraController(ICompraService compraService, ILogger<CompraController> logger)
        {
            _compraService = compraService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> EfetuarCompra(CompraDTO compraDTO)
        {
            using var activity = OpenTelemetryExtensions.StartActivity("CompraController.EfetuarCompra", ActivityKind.Server);
            
            var userId = User.FindFirst("id")?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            activity?.EnrichWithBusinessContext("Compra", "Create");
            activity?.EnrichWithUserContext(userId, userRole);
            activity?.SetTag("compra.jogo_id", compraDTO.JogoId);
            activity?.SetTag("compra.usuario_id", compraDTO.UsuarioId);
            activity?.SetTag("compra.quantidade", compraDTO.Quantidade);

            _logger.LogInformation("Iniciando compra. JogoId: {JogoId}, UsuarioId: {UsuarioId}, Quantidade: {Quantidade}", compraDTO.JogoId, compraDTO.UsuarioId, compraDTO.Quantidade);
            
            try
            {
                var result = await _compraService.EfetuarCompra(compraDTO);
                
                if (result.Result == true)
                {
                    activity?.SetTag("operation.success", true);
                    _logger.LogInformation("Compra efetuada com sucesso. JogoId: {JogoId}, UsuarioId: {UsuarioId}", compraDTO.JogoId, compraDTO.UsuarioId);
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
                _logger.LogError(ex, "Erro ao efetuar compra. JogoId: {JogoId}, UsuarioId: {UsuarioId}", compraDTO.JogoId, compraDTO.UsuarioId);
                activity?.RecordException(ex);
                throw;
            }
        }
    }
}