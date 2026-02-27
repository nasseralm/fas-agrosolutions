using AutoMapper;
using FCG.API.Models;
using FCG.Application.DTOs;
using FCG.Application.Interfaces;
using FCG.Application.Interfaces.Messaging;
using FCG.Domain.Entities;
using FCG.Domain.Interfaces;
using FCG.Domain.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FCG.Application.Services
{
    public class CompraService : ICompraService
    {
        private readonly IJogoRepository _jogoRepository;
        private readonly ICompraRepository _compraRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CompraService> _logger;
        private readonly IOptions<FcgPagamentosAPI> _fcgPagamentosApi;
        private readonly HttpClient _httpClient;
        private static readonly ActivitySource ActivitySource = new("FCG.Application");
        private readonly IMessageBus _messageBus;

        public CompraService(
            IJogoRepository jogoRepository, ICompraRepository compraRepository,
            IUnitOfWork unitOfWork, ILogger<CompraService> logger, 
            IOptions<FcgPagamentosAPI> fcgPagamentosApi, HttpClient httpClient,
            IMessageBus messageBus)
        {
            _jogoRepository = jogoRepository;
            _compraRepository = compraRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _fcgPagamentosApi = fcgPagamentosApi;
            _httpClient = httpClient;
            _messageBus = messageBus;
        }

        public async Task<DomainNotificationsResult<bool>> EfetuarCompra(CompraDTO compraDTO)
        {
            using var activity = ActivitySource.StartActivity("CompraService.EfetuarCompra");
            activity?.SetTag("compra.jogo_id", compraDTO.JogoId);
            activity?.SetTag("compra.usuario_id", compraDTO.UsuarioId);
            activity?.SetTag("compra.quantidade", compraDTO.Quantidade);
            activity?.SetTag("compra.forma_pagamento_id", compraDTO.FormaPagamentoId);
            activity?.SetTag("operation.type", "create");

            var result = new DomainNotificationsResult<bool>();

            _logger.LogInformation(
                "Iniciando efetuação de compra. JogoId: {JogoId}, UsuarioId: {UsuarioId}, Quantidade: {Quantidade}",
                compraDTO.JogoId, compraDTO.UsuarioId, compraDTO.Quantidade);

            try
            {
                var jogo = await _jogoRepository.Selecionar(compraDTO.JogoId);
                if (jogo == null)
                {
                    _logger.LogWarning("Jogo não encontrado para compra. JogoId: {JogoId}", compraDTO.JogoId);
                    activity?.SetTag("result", "jogo_not_found");
                    result.Notifications.Add("Jogo não encontrado.");
                    return result;
                }

                activity?.SetTag("jogo.nome", jogo.Nome);
                activity?.SetTag("jogo.preco", jogo.Preco);
                activity?.SetTag("jogo.estoque", jogo.Estoque);

                if (jogo.Estoque < compraDTO.Quantidade)
                {
                    _logger.LogWarning(
                        "Estoque insuficiente para compra. JogoId: {JogoId}, Estoque: {Estoque}, Solicitado: {Quantidade}",
                        compraDTO.JogoId, jogo.Estoque, compraDTO.Quantidade);

                    activity?.SetTag("result", "insufficient_stock");
                    result.Notifications.Add("Estoque insuficiente.");
                    return result;
                }

                var valorTotal = jogo.Preco * compraDTO.Quantidade;
                activity?.SetTag("compra.valor_total", valorTotal);

                //1) Cria a compra como pendente
                var compra = new Compra(
                    jogo.Id,
                    compraDTO.UsuarioId,
                    compraDTO.Quantidade,
                    valorTotal,
                    compraDTO.FormaPagamentoId);

                compra.MarcarComoPendente();

                await _compraRepository.Incluir(compra);
                await _unitOfWork.Commit();

                activity?.SetTag("compra.id", compra.Id);
                
                var correlationId = Guid.NewGuid();
                var eventId = Guid.NewGuid();

                using var paymentActivity = ActivitySource.StartActivity("Payment.Request.Publish");
                paymentActivity?.SetTag("messaging.system", "rabbitmq");
                paymentActivity?.SetTag("messaging.operation", "publish");
                paymentActivity?.SetTag("messaging.message_id", eventId);
                paymentActivity?.SetTag("messaging.correlation_id", correlationId);
                paymentActivity?.SetTag("compra.id", compra.Id);
                paymentActivity?.SetTag("payment.valor", valorTotal);
                paymentActivity?.SetTag("payment.forma_pagamento_id", compraDTO.FormaPagamentoId);

                var evt = new IntegrationEvents.Pagamentos.PagamentoSolicitadoV1(
                    EventId: eventId,
                    OccurredAt: DateTimeOffset.UtcNow,
                    CorrelationId: correlationId,
                    CompraId: compra.Id,
                    JogoId: compraDTO.JogoId,
                    UsuarioId: compraDTO.UsuarioId,
                    Quantidade: compraDTO.Quantidade,
                    Valor: valorTotal,
                    FormaPagamentoId: compraDTO.FormaPagamentoId
                );

                try
                {
                    //2) Publica evento para Pagamentos processar de forma assíncrona - Event Driven Architecture
                    await _messageBus.PublishAsync(evt);
                    paymentActivity?.SetStatus(ActivityStatusCode.Ok);

                    activity?.SetTag("result", "payment_requested");
                    activity?.SetStatus(ActivityStatusCode.Ok);

                    result.Result = true;

                    _logger.LogInformation(
                        "Compra registrada e pagamento solicitado via mensageria. CompraId: {CompraId}, JogoId: {JogoId}, UsuarioId: {UsuarioId}, Quantidade: {Quantidade}, Valor: {Valor}",
                        compra.Id, compraDTO.JogoId, compraDTO.UsuarioId, compraDTO.Quantidade, valorTotal);
                }
                catch (Exception pubEx)
                {
                    //Compra está pendente, mas o evento não foi publicado
                    _logger.LogError(pubEx,
                        "Falha ao publicar evento de pagamento. CompraId={CompraId}. Compra ficará pendente.",
                        compra.Id);

                    paymentActivity?.SetStatus(ActivityStatusCode.Error, pubEx.Message);
                    activity?.SetTag("result", "publish_failed");
                    activity?.SetStatus(ActivityStatusCode.Error, pubEx.Message);

                    result.Notifications.Add("Compra registrada, mas houve falha ao solicitar o pagamento. Tente novamente.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao efetuar compra. JogoId: {JogoId}, UsuarioId: {UsuarioId}", compraDTO.JogoId, compraDTO.UsuarioId);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("exception.type", ex.GetType().Name);
                result.Notifications.Add("Erro ao efetuar compra.");
            }

            return result;
        }

        //Modelo antigo - Sem Mensageria
        //public async Task<DomainNotificationsResult<bool>> EfetuarCompra(CompraDTO compraDTO)
        //{
        //    using var activity = ActivitySource.StartActivity("CompraService.EfetuarCompra");
        //    activity?.SetTag("compra.jogo_id", compraDTO.JogoId);
        //    activity?.SetTag("compra.usuario_id", compraDTO.UsuarioId);
        //    activity?.SetTag("compra.quantidade", compraDTO.Quantidade);
        //    activity?.SetTag("compra.forma_pagamento_id", compraDTO.FormaPagamentoId);
        //    activity?.SetTag("operation.type", "create");

        //    var result = new DomainNotificationsResult<bool>();

        //    _logger.LogInformation("Iniciando efetuação de compra. JogoId: {JogoId}, UsuarioId: {UsuarioId}, Quantidade: {Quantidade}", compraDTO.JogoId, compraDTO.UsuarioId, compraDTO.Quantidade);

        //    try
        //    {
        //        var jogo = await _jogoRepository.Selecionar(compraDTO.JogoId);
        //        if (jogo == null)
        //        {
        //            _logger.LogWarning("Jogo não encontrado para compra. JogoId: {JogoId}", compraDTO.JogoId);
        //            activity?.SetTag("result", "jogo_not_found");
        //            result.Notifications.Add("Jogo não encontrado.");
        //            return result;
        //        }

        //        activity?.SetTag("jogo.nome", jogo.Nome);
        //        activity?.SetTag("jogo.preco", jogo.Preco);
        //        activity?.SetTag("jogo.estoque", jogo.Estoque);

        //        if (jogo.Estoque < compraDTO.Quantidade)
        //        {
        //            _logger.LogWarning("Estoque insuficiente para compra. JogoId: {JogoId}, Estoque: {Estoque}, Solicitado: {Quantidade}", compraDTO.JogoId, jogo.Estoque, compraDTO.Quantidade);
        //            activity?.SetTag("result", "insufficient_stock");
        //            result.Notifications.Add("Estoque insuficiente.");
        //            return result;
        //        }

        //        var valorTotal = jogo.Preco * compraDTO.Quantidade;
        //        activity?.SetTag("compra.valor_total", valorTotal);

        //        using var httpClient = new HttpClient();

        //        var pagamentoDTO = new
        //        {
        //            compraDTO.UsuarioId,
        //            compraDTO.JogoId,
        //            Valor = valorTotal,
        //            compraDTO.Quantidade,
        //            compraDTO.FormaPagamentoId
        //        };

        //        var url = $"{_fcgPagamentosApi.Value.UrlBase}/pagamento/efetuar";
        //        activity?.SetTag("payment.api.url", url);

        //        using var paymentActivity = ActivitySource.StartActivity("PaymentAPI.Call");
        //        paymentActivity?.SetTag("http.method", "POST");
        //        paymentActivity?.SetTag("http.url", url);
        //        paymentActivity?.SetTag("payment.valor", valorTotal);
        //        paymentActivity?.SetTag("payment.forma_pagamento_id", compraDTO.FormaPagamentoId);

        //        //Aqui é chamado o microservice Pagamento para efetuar o pagamento do jogo
        //        //Neste local quero implementar o RabbitMq

        //        var response = await _httpClient.PostAsJsonAsync(url, pagamentoDTO);


        //        paymentActivity?.SetTag("http.status_code", (int)response.StatusCode);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            _logger.LogWarning("Pagamento não autorizado para compra. JogoId: {JogoId}, UsuarioId: {UsuarioId}, StatusCode: {StatusCode}", compraDTO.JogoId, compraDTO.UsuarioId, response.StatusCode);
        //            activity?.SetTag("result", "payment_not_authorized");
        //            paymentActivity?.SetStatus(ActivityStatusCode.Error, "Payment not authorized");
        //            result.Notifications.Add("Pagamento não autorizado.");
        //            return result;
        //        }

        //        paymentActivity?.SetStatus(ActivityStatusCode.Ok);

        //        jogo.BaixarEstoque(compraDTO.Quantidade);
        //        _jogoRepository.Alterar(jogo);

        //        var compra = new Compra(jogo.Id, compraDTO.UsuarioId, compraDTO.Quantidade, valorTotal, compraDTO.FormaPagamentoId);
        //        await _compraRepository.Incluir(compra);

        //        await _unitOfWork.Commit();

        //        activity?.SetTag("compra.id", compra.Id);
        //        activity?.SetTag("result", "success");

        //        result.Result = true;
        //        _logger.LogInformation("Compra efetuada com sucesso. CompraId: {CompraId}, JogoId: {JogoId}, UsuarioId: {UsuarioId}, Quantidade: {Quantidade}", compra.Id, compraDTO.JogoId, compraDTO.UsuarioId, compraDTO.Quantidade);
        //        activity?.SetStatus(ActivityStatusCode.Ok);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Erro ao efetuar compra. JogoId: {JogoId}, UsuarioId: {UsuarioId}", compraDTO.JogoId, compraDTO.UsuarioId);
        //        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        //        activity?.SetTag("exception.type", ex.GetType().Name);
        //        result.Notifications.Add("Erro ao efetuar compra.");
        //    }

        //    return result;
        //}
    }
}