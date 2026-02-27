using FCG.Domain.Enums;

namespace FCG.Domain.Entities
{
    public class Compra
    {
        public int Id { get; private set; }
        public int JogoId { get; private set; }
        public int UsuarioId { get; private set; }
        public int Quantidade { get; private set; }
        public decimal ValorTotal { get; private set; }
        public DateTime DataCompra { get; private set; }
        public int FormaPagamentoId { get; private set; }
        public StatusCompra Status { get; private set; }
        public string? PaymentId { get; private set; }
        public string? MotivoRecusa { get; private set; }
        public DateTime DataStatus { get; private set; }

        public Compra(int jogoId, int usuarioId, int quantidade, decimal valorTotal, int formaPagamentoId)
        {
            JogoId = jogoId;
            UsuarioId = usuarioId;
            Quantidade = quantidade;
            ValorTotal = valorTotal;
            FormaPagamentoId = formaPagamentoId;

            DataCompra = DateTime.UtcNow;

            Status = StatusCompra.Pendente;
            DataStatus = DateTime.UtcNow;
        }

        protected Compra() { }

        public void MarcarComoPendente()
        {
            Status = StatusCompra.Pendente;
            PaymentId = null;
            MotivoRecusa = null;
            DataStatus = DateTime.UtcNow;
        }

        public void AprovarPagamento(string paymentId)
        {
            Status = StatusCompra.Aprovada;
            PaymentId = paymentId;
            MotivoRecusa = null;
            DataStatus = DateTime.UtcNow;
        }

        public void MarcarComoRecusada(string motivo)
        {
            Status = StatusCompra.Recusada;
            MotivoRecusa = motivo;
            DataStatus = DateTime.UtcNow;
        }
    }
}
