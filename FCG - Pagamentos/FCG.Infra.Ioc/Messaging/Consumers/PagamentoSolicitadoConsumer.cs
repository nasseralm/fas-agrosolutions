using FCG.API.Models;
using FCG.Application.DTOs;
using FCG.Application.IntegrationEvents.Pagamentos;
using FCG.Application.Interfaces;
using FCG.Domain.Notifications;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FCG.Infra.IoC.Messaging.Consumers
{
    public class PagamentoSolicitadoConsumer : IConsumer<PagamentoSolicitadoV1>
    {
        private readonly ILogger<PagamentoSolicitadoConsumer> _logger;
        private readonly IPagamentoService _pagamentoService;
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public PagamentoSolicitadoConsumer(
            ILogger<PagamentoSolicitadoConsumer> logger,
            IPagamentoService pagamentoService,
            ISendEndpointProvider sendEndpointProvider)
        {
            _logger = logger;
            _pagamentoService = pagamentoService;
            _sendEndpointProvider = sendEndpointProvider;
        }

        public async Task Consume(ConsumeContext<PagamentoSolicitadoV1> context)
        {
            var message = context.Message;

            _logger.LogInformation("Pagamento solicitado recebido. CompraId={CompraId}, UsuarioId={UsuarioId}, JogoId={JogoId}, Valor={Valor}, CorrelationId={CorrelationId}",
                message.CompraId, message.UsuarioId, message.JogoId, message.Valor, message.CorrelationId);

            //1️ - Monta o DTO para reaproveitar o Application Service
            var pagamentoDTO = new PagamentoDTO
            {
                UsuarioId = message.UsuarioId,
                JogoId = message.JogoId,
                FormaPagamentoId = message.FormaPagamentoId,
                Valor = message.Valor,
                Quantidade = message.Quantidade
            };

            DomainNotificationsResult<PagamentoViewModel> pagamentoResult;

            try
            {
                //2️ - Executa o fluxo de pagamento
                pagamentoResult = await _pagamentoService.Efetuar(pagamentoDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao processar pagamento solicitado. CompraId={CompraId}", message.CompraId);

                await EnviarPagamentoRecusadoAsync(message, "Falha inesperada ao processar pagamento.", context.CancellationToken);

                return;
            }

            //3️ - Regra de sucesso x falha
            var pagamentoFalhou =
                pagamentoResult.Result == null ||
                pagamentoResult.Notifications.Any(n => n.Contains("Erro ao efetuar pagamento", StringComparison.OrdinalIgnoreCase));

            if (pagamentoFalhou)
            {
                var motivo = pagamentoResult.Notifications.Any() ? string.Join(" | ", pagamentoResult.Notifications) : "Pagamento não aprovado.";

                await EnviarPagamentoRecusadoAsync(message, motivo, context.CancellationToken);

                return;
            }

            //4️ - Pagamento aprovado → envia para a API Jogos
            var paymentId = pagamentoResult.Result.Id.ToString();

            await EnviarPagamentoAprovadoAsync(message,paymentId,context.CancellationToken);

            _logger.LogInformation("Pagamento aprovado e evento enviado. CompraId={CompraId}, PaymentId={PaymentId}", message.CompraId, paymentId);
        }

        //ENVIO DE EVENTOS PARA A API JOGOS

        private async Task EnviarPagamentoAprovadoAsync(
            PagamentoSolicitadoV1 solicitado,
            string paymentId,
            CancellationToken cancellationToken)
        {
            var evento = new PagamentoAprovadoV1(
                EventId: Guid.NewGuid(),
                OccurredAt: DateTimeOffset.UtcNow,
                CorrelationId: solicitado.CorrelationId,
                CompraId: solicitado.CompraId,
                PaymentId: paymentId
            );

            var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:fcg.jogos.pagamentos.aprovado"));

            await endpoint.Send(evento, cancellationToken);
        }

        private async Task EnviarPagamentoRecusadoAsync(
            PagamentoSolicitadoV1 solicitado,
            string motivo,
            CancellationToken cancellationToken)
        {
            var evento = new PagamentoRecusadoV1(
                EventId: Guid.NewGuid(),
                OccurredAt: DateTimeOffset.UtcNow,
                CorrelationId: solicitado.CorrelationId,
                CompraId: solicitado.CompraId,
                Motivo: motivo
            );

            var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:fcg.jogos.pagamentos.recusado"));

            await endpoint.Send(evento, cancellationToken);
        }
    }
}
