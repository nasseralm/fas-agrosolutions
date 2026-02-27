namespace FCG.Domain.DTOs
{
    public class SyncReportResult
    {
        public int TotalJogos { get; set; }
        public int JogosSucesso { get; set; }
        public int JogosFalha { get; set; }
        public bool Sucesso => JogosFalha == 0 && TotalJogos > 0;
        public List<string> ErrosGerais { get; set; } = new List<string>();
        public List<SyncItemDetail> DetalhesItens { get; set; } = new List<SyncItemDetail>();
    }

    public class SyncItemDetail
    {
        public int JogoId { get; set; }
        public string JogoNome { get; set; }
        public bool Sucesso { get; set; }
        public string MensagemErro { get; set; }
        public DateTime ProcessadoEm { get; set; } = DateTime.UtcNow;
    }
}