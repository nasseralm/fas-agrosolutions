using FAS.Application.DTOs;
using FAS.Infra.Ioc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FAS.Application.Interfaces;

namespace FAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // T-005/T-009: endpoints protegidos; só acessa/alterar dados do próprio produtor
    public class UsuarioController : BaseController
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(IUsuarioService usuarioService, ILogger<UsuarioController> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("Incluir")]
        public async Task<IActionResult> Incluir(UsuarioDTO usuarioDTO)
        {
            _logger.LogInformation("Iniciando inclusão de usuário. Email: {Email}, Nome: {Nome}", usuarioDTO.Email, usuarioDTO.Nome);
            try
            {
                var result = await _usuarioService.Incluir(usuarioDTO);
                _logger.LogInformation("Usuário incluído com sucesso. Email: {Email}, Id: {Id}", usuarioDTO.Email, result.Result?.Id);
                return CreateIActionResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao incluir usuário. Email: {Email}", usuarioDTO.Email);
                throw;
            }
        }

        [HttpPut("Alterar")]
        public async Task<IActionResult> Alterar(UsuarioDTO usuarioDTO)
        {
            _logger.LogInformation("Tentativa de alteração de usuário. Id: {Id}, Email: {Email}", usuarioDTO.Id, usuarioDTO.Email);
            try
            {
                var producerId = User.GetProducerId();
                // T-009: só alterar dados do próprio produtor (ou Admin pode alterar qualquer um)
                if (!User.IsInRole("Admin") && usuarioDTO.Id != producerId)
                {
                    _logger.LogWarning("Produtor {ProducerId} tentou alterar dados do produtor {IdAlterar}", producerId, usuarioDTO.Id);
                    return Forbid();
                }

                var result = await _usuarioService.Alterar(usuarioDTO);
                _logger.LogInformation("Usuário alterado com sucesso. Id: {Id}, Email: {Email}", usuarioDTO.Id, usuarioDTO.Email);
                return CreateIActionResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar usuário. Id: {Id}, Email: {Email}", usuarioDTO.Id, usuarioDTO.Email);
                throw;
            }
        }

        [HttpDelete("Excluir")]
        public async Task<IActionResult> Excluir(int id)
        {
            _logger.LogInformation("Tentativa de exclusão de usuário. Id: {Id}", id);
            try
            {
                var producerId = User.GetProducerId();
                // T-009: só excluir próprio cadastro (ou Admin pode excluir qualquer um)
                if (!User.IsInRole("Admin") && id != producerId)
                {
                    _logger.LogWarning("Produtor {ProducerId} tentou excluir produtor {Id}", producerId, id);
                    return Forbid();
                }

                var result = await _usuarioService.Excluir(id);
                _logger.LogInformation("Usuário excluído com sucesso. Id: {Id}", id);
                return CreateIActionResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir usuário. Id: {Id}", id);
                throw;
            }
        }

        [HttpGet("SelecionarPorId")]
        public async Task<IActionResult> Selecionar(int id)
        {
            _logger.LogInformation("Consulta de usuário por Id. Id: {Id}", id);
            try
            {
                // T-009: usuário A não acessa dados do usuário B — só próprio produtor ou Admin
                var producerId = User.GetProducerId();
                if (!User.IsInRole("Admin") && id != producerId)
                {
                    _logger.LogWarning("Produtor {ProducerId} tentou acessar dados do produtor {Id}", producerId, id);
                    return Forbid();
                }

                var result = await _usuarioService.Selecionar(id);
                _logger.LogInformation("Consulta de usuário por Id finalizada. Id: {Id}", id);
                return CreateIActionResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar usuário por Id. Id: {Id}", id);
                throw;
            }
        }

        [HttpGet("SelecionarPorNomeEmail")]
        public async Task<IActionResult> SelecionarPorNomeEmail([FromQuery] string email, [FromQuery] string nome)
        {
            _logger.LogInformation("Consulta de usuário por nome/email. Email: {Email}, Nome: {Nome}", email, nome);
            try
            {
                var result = await _usuarioService.SelecionarPorNomeEmail(email, nome);
                // T-009: se não for Admin, só pode ver o próprio registro
                var producerId = User.GetProducerId();
                if (!User.IsInRole("Admin") && result.HasResult && result.Result.Id != producerId)
                {
                    _logger.LogWarning("Produtor {ProducerId} tentou acessar dados de outro produtor", producerId);
                    return Forbid();
                }
                _logger.LogInformation("Consulta de usuário por nome/email finalizada. Email: {Email}, Nome: {Nome}", email, nome);
                return CreateIActionResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar usuário por nome/email. Email: {Email}, Nome: {Nome}", email, nome);
                throw;
            }
        }
    }
}
