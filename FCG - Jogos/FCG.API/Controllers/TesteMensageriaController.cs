using FCG.Application.IntegrationEvents.Pagamentos;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace FCG.API.Controllers
{
    [ApiController]
    [Route("api/test/mensageria")]
    public class TesteMensageriaController : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public TesteMensageriaController(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost("pagamento/aprovado/{compraId:int}")]
        public async Task<IActionResult> PublicarPagamentoAprovado(int compraId)
        {
            var evt = new PagamentoAprovadoV1(
                EventId: Guid.NewGuid(),
                OccurredAt: DateTimeOffset.UtcNow,
                CorrelationId: Guid.NewGuid(),
                CompraId: compraId,
                PaymentId: "pay_test_123"
            );

            await _publishEndpoint.Publish(evt);

            return Ok(new
            {
                compraId,
                status = "PagamentoAprovadoV1 publicado com sucesso"
            });
        }

        [HttpPost("pagamento/recusado/{compraId:int}")]
        public async Task<IActionResult> PublicarPagamentoRecusado(int compraId)
        {
            var evt = new PagamentoRecusadoV1(
                EventId: Guid.NewGuid(),
                OccurredAt: DateTimeOffset.UtcNow,
                CorrelationId: Guid.NewGuid(),
                CompraId: compraId, Motivo: "motivo recusado"
            );

            await _publishEndpoint.Publish(evt);

            return Ok(new
            {
                compraId,
                status = "PagamentoRecusadoV1 publicado com sucesso"
            });
        }
    }
}
