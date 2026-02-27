namespace FCG.API.Models
{
    public class JogoViewModel
    {
        public int Id { get; private set; }
        public string Nome { get; private set; }
        public string Descricao { get; private set; }
        public string Genero { get; private set; }
        public decimal Preco { get; private set; }
        public DateTime DataLancamento { get; private set; }
        public string Desenvolvedor { get; private set; }
        public string Distribuidora { get; private set; }
        public string ClassificacaoIndicativa { get; private set; }
        public int Estoque { get; private set; }
        public string Plataforma { get; private set; }
    }
}
