namespace FCG.Application.IntegrationEvents.Pagamentos
{
    public record PagamentoRecusadoV1(
        Guid EventId,
        DateTimeOffset OccurredAt,
        Guid CorrelationId,
        int CompraId,
        string Motivo
    );
}
