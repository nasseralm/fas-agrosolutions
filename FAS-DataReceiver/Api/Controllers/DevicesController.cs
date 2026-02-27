using Agro.DataReceiver.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Agro.DataReceiver.Api.Controllers;

[ApiController]
[Route("v1/devices")]
public sealed class DevicesController : ControllerBase
{
    private readonly IDispositivoRepository _dispositivoRepository;

    public DevicesController(IDispositivoRepository dispositivoRepository)
    {
        _dispositivoRepository = dispositivoRepository;
    }

    /// <summary>
    /// Mapeamento talhão → sensor (para exibir na tela de propriedades/talhões).
    /// </summary>
    [HttpGet("mapping")]
    [ProducesResponseType(typeof(IReadOnlyList<DeviceMappingEntry>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMapping(CancellationToken cancellationToken)
    {
        var list = await _dispositivoRepository.GetMappingAsync(cancellationToken);
        return Ok(list);
    }
}
