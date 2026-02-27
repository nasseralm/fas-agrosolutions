namespace FCG.Application.IntegrationEvents.Pagamentos
{
    public record PagamentoSolicitadoV1(
        Guid EventId,
        DateTimeOffset OccurredAt,
        Guid CorrelationId,

        int CompraId,
        int JogoId,
        int UsuarioId,
        int Quantidade,
        decimal Valor,
        int FormaPagamentoId
    );
}
