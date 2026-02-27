using FCG.Application.IntegrationEvents.Pagamentos;
using FCG.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FCG.Infra.IoC.Messaging.Consumers;

public class PagamentoRecusadoConsumer : IConsumer<PagamentoRecusadoV1>
{
    private readonly ILogger<PagamentoRecusadoConsumer> _logger;
    private readonly ICompraRepository _compraRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PagamentoRecusadoConsumer(
        ILogger<PagamentoRecusadoConsumer> logger,
        ICompraRepository compraRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _compraRepository = compraRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<PagamentoRecusadoV1> context)
    {
        var msg = context.Message;

        _logger.LogInformation("Pagamento recusado recebido. CompraId={CompraId} Motivo={Motivo}", msg.CompraId, msg.Motivo);

        var compra = await _compraRepository.Selecionar(msg.CompraId);
        if (compra == null)
        {
            _logger.LogWarning("Compra não encontrada para finalizar (recusado). CompraId={CompraId}", msg.CompraId);
            return;
        }

        compra.MarcarComoRecusada(msg.Motivo);

        _compraRepository.Alterar(compra);
        await _unitOfWork.Commit();

        _logger.LogInformation("Compra marcada como recusada/cancelada. CompraId={CompraId}", msg.CompraId);
    }
}
