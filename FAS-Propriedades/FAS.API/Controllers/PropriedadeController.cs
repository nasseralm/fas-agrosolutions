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
    public class PropriedadeController : BaseController
    {
        private readonly IPropriedadeService _propriedadeService;
        private readonly ILogger<PropriedadeController> _logger;

        public PropriedadeController(IPropriedadeService propriedadeService, ILogger<PropriedadeController> logger)
        {
            _propriedadeService = propriedadeService;
            _logger = logger;
        }

        [HttpPost("Incluir")]
        public async Task<IActionResult> Incluir(PropriedadeDTO dto)
        {
            var producerId = User.GetProducerId();
            _logger.LogInformation("Incluir propriedade solicitado. ProducerId: {ProducerId}", producerId);
            var result = await _propriedadeService.Incluir(producerId, dto);
            return CreateIActionResult(result);
        }

        [HttpPut("Alterar")]
        public async Task<IActionResult> Alterar(PropriedadeDTO dto)
        {
            var producerId = User.GetProducerId();
            var isAdmin = User.IsInRole("Admin");

            _logger.LogInformation("Alterar propriedade solicitado. Id: {Id} ProducerId: {ProducerId}", dto?.Id, producerId);
            var result = await _propriedadeService.Alterar(producerId, isAdmin, dto);

            if (result.HasNotifications && result.Notifications.Contains("Acesso negado."))
                return Forbid();

            return CreateIActionResult(result);
        }

        [HttpDelete("Excluir")]
        public async Task<IActionResult> Excluir(int id)
        {
            var producerId = User.GetProducerId();
            var isAdmin = User.IsInRole("Admin");

            _logger.LogInformation("Excluir propriedade solicitado. Id: {Id} ProducerId: {ProducerId}", id, producerId);
            var result = await _propriedadeService.Excluir(producerId, isAdmin, id);

            if (result.HasNotifications && result.Notifications.Contains("Acesso negado."))
                return Forbid();

            return CreateIActionResult(result);
        }

        [HttpGet("SelecionarPorId")]
        public async Task<IActionResult> Selecionar(int id)
        {
            var producerId = User.GetProducerId();
            var isAdmin = User.IsInRole("Admin");

            _logger.LogInformation("Selecionar propriedade solicitado. Id: {Id} ProducerId: {ProducerId}", id, producerId);
            var result = await _propriedadeService.Selecionar(id);

            if (!result.HasNotifications && result.Result != null && !isAdmin && result.Result.ProducerId != producerId)
            {
                return Forbid();
            }

            return CreateIActionResult(result);
        }

        [HttpGet("Listar")]
        public async Task<IActionResult> Listar([FromQuery] PaginationParams paginationParams)
        {
            var producerId = User.GetProducerId();
            _logger.LogInformation("Listar propriedades solicitado. ProducerId: {ProducerId}", producerId);

            var result = await _propriedadeService.Listar(producerId, paginationParams.PageNumber, paginationParams.PageSize);

            if (!result.HasNotifications && result.Result != null)
            {
                Response.AddPaginationHeader(new PaginationHeader(result.Result.CurrentPage, result.Result.PageSize, result.Result.TotalCount, result.Result.TotalPages));
            }

            return CreateIActionResult(result);
        }
    }
}

