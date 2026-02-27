using FCG.Application.IntegrationEvents.Pagamentos;
using FCG.Domain.Enums;
using FCG.Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FCG.Infra.IoC.Messaging.Consumers
{
    public class PagamentoAprovadoConsumer : IConsumer<PagamentoAprovadoV1>
    {
        private readonly ILogger<PagamentoAprovadoConsumer> _logger;
        private readonly ICompraRepository _compraRepository;
        private readonly IJogoRepository _jogoRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PagamentoAprovadoConsumer(
            ILogger<PagamentoAprovadoConsumer> logger,
            ICompraRepository compraRepository,
            IJogoRepository jogoRepository,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _compraRepository = compraRepository;
            _jogoRepository = jogoRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Consume(ConsumeContext<PagamentoAprovadoV1> context)
        {
            var msg = context.Message;

            _logger.LogInformation(
                "Pagamento aprovado recebido. CompraId={CompraId} PaymentId={PaymentId} CorrelationId={CorrelationId}",
                msg.CompraId, msg.PaymentId, msg.CorrelationId);

            // 1) Buscar compra
            var compra = await _compraRepository.Selecionar(msg.CompraId);
            if (compra == null)
            {
                _logger.LogWarning("Compra não encontrada ao processar pagamento aprovado. CompraId={CompraId}", msg.CompraId);
                return;
            }

            // 2) Idempotência: se já não está pendente, não reprocessa
            if (compra.Status != StatusCompra.Pendente)
            {
                _logger.LogInformation(
                    "Compra já processada (status={Status}). Ignorando mensagem duplicada. CompraId={CompraId}",
                    compra.Status, compra.Id);
                return;
            }

            // 3) Buscar jogo
            var jogo = await _jogoRepository.Selecionar(compra.JogoId);
            if (jogo == null)
            {
                _logger.LogWarning(
                    "Jogo não encontrado ao finalizar compra aprovada. CompraId={CompraId} JogoId={JogoId}",
                    compra.Id, compra.JogoId);
                return;
            }

            // 4) Baixar estoque
            if (jogo.Estoque < compra.Quantidade)
            {
                _logger.LogWarning(
                    "Estoque insuficiente no momento da finalização. CompraId={CompraId} JogoId={JogoId} Estoque={Estoque} Solicitado={Quantidade}",
                    compra.Id, compra.JogoId, jogo.Estoque, compra.Quantidade);

                compra.MarcarComoRecusada("Estoque insuficiente na finalização da compra.");
                _compraRepository.Alterar(compra);
                await _unitOfWork.Commit();
                return;
            }

            jogo.BaixarEstoque(compra.Quantidade);
            _jogoRepository.Alterar(jogo);

            // 5) Aprovar compra + salvar PaymentId
            compra.AprovarPagamento(msg.PaymentId);
            _compraRepository.Alterar(compra);

            await _unitOfWork.Commit();

            _logger.LogInformation(
                "Compra finalizada com sucesso (pagamento aprovado). CompraId={CompraId} JogoId={JogoId}",
                compra.Id, compra.JogoId);
        }
    }
}
