namespace FCG.Application.IntegrationEvents.Pagamentos
{
    public record PagamentoAprovadoV1(
        Guid EventId,
        DateTimeOffset OccurredAt,
        Guid CorrelationId,
        int CompraId,
        string PaymentId
    );
}
