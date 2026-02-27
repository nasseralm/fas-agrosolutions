using FAS.API.Extensions;
using FAS.API.Models;
using FAS.Application.DTOs;
using FAS.Application.Interfaces;
using FAS.Infra.Ioc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TalhaoController : BaseController
    {
        private readonly ITalhaoService _talhaoService;
        private readonly ILogger<TalhaoController> _logger;

        public TalhaoController(ITalhaoService talhaoService, ILogger<TalhaoController> logger)
        {
            _talhaoService = talhaoService;
            _logger = logger;
        }

        [HttpPost("Incluir")]
        public async Task<IActionResult> Incluir(TalhaoDTO dto)
        {
            var producerId = User.GetProducerId();
            var isAdmin = User.IsInRole("Admin");

            _logger.LogInformation("Incluir talhão solicitado. ProducerId: {ProducerId} PropriedadeId: {PropriedadeId}", producerId, dto?.PropriedadeId);
            var result = await _talhaoService.Incluir(producerId, isAdmin, dto);

            if (result.HasNotifications && result.Notifications.Contains("Acesso negado."))
                return Forbid();

            return CreateIActionResult(result);
        }

        [HttpPut("Alterar")]
        public async Task<IActionResult> Alterar(TalhaoDTO dto)
        {
            var producerId = User.GetProducerId();
            var isAdmin = User.IsInRole("Admin");

            _logger.LogInformation("Alterar talhão solicitado. Id: {Id} ProducerId: {ProducerId}", dto?.Id, producerId);
            var result = await _talhaoService.Alterar(producerId, isAdmin, dto);

            if (result.HasNotifications && result.Notifications.Contains("Acesso negado."))
                return Forbid();

            return CreateIActionResult(result);
        }

        [HttpDelete("Excluir")]
        public async Task<IActionResult> Excluir(int id)
        {
            var producerId = User.GetProducerId();
            var isAdmin = User.IsInRole("Admin");

            _logger.LogInformation("Excluir talhão solicitado. Id: {Id} ProducerId: {ProducerId}", id, producerId);
            var result = await _talhaoService.Excluir(producerId, isAdmin, id);

            if (result.HasNotifications && result.Notifications.Contains("Acesso negado."))
                return Forbid();

            return CreateIActionResult(result);
        }

        [HttpGet("SelecionarPorId")]
        public async Task<IActionResult> Selecionar(int id)
        {
            var producerId = User.GetProducerId();
            var isAdmin = User.IsInRole("Admin");

            _logger.LogInformation("Selecionar talhão solicitado. Id: {Id} ProducerId: {ProducerId}", id, producerId);
            var result = await _talhaoService.Selecionar(id);

            if (!result.HasNotifications && result.Result != null && !isAdmin && result.Result.ProducerId != producerId)
            {
                return Forbid();
            }

            return CreateIActionResult(result);
        }

        [HttpGet("ListarPorPropriedade")]
        public async Task<IActionResult> ListarPorPropriedade(int propriedadeId, [FromQuery] PaginationParams paginationParams)
        {
            var producerId = User.GetProducerId();
            var isAdmin = User.IsInRole("Admin");

            _logger.LogInformation("Listar talhões solicitado. PropriedadeId: {PropriedadeId} ProducerId: {ProducerId}", propriedadeId, producerId);
            var result = await _talhaoService.ListarPorPropriedade(producerId, isAdmin, propriedadeId, paginationParams.PageNumber, paginationParams.PageSize);

            if (result.HasNotifications && result.Notifications.Contains("Acesso negado."))
                return Forbid();

            if (!result.HasNotifications && result.Result != null)
            {
                Response.AddPaginationHeader(new PaginationHeader(result.Result.CurrentPage, result.Result.PageSize, result.Result.TotalCount, result.Result.TotalPages));
            }

            return CreateIActionResult(result);
        }
    }
}

