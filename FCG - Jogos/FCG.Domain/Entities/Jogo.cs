namespace FCG.Domain.Entities
{
    public class Jogo
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

        public Jogo() { }

        public Jogo(
            int id,
            string nome,
            string descricao,
            string genero,
            decimal preco,
            DateTime dataLancamento,
            string desenvolvedor,
            string distribuidora,
            string classificacaoIndicativa,
            int estoque,
            string plataforma)
        {
            Id = id;
            Nome = nome;
            Descricao = descricao;
            Genero = genero;
            Preco = preco;
            DataLancamento = dataLancamento;
            Desenvolvedor = desenvolvedor;
            Distribuidora = distribuidora;
            ClassificacaoIndicativa = classificacaoIndicativa;
            Estoque = estoque;
            Plataforma = plataforma;
        }

        public void BaixarEstoque(int quantidade)
        {
            if (quantidade < 0)
                throw new ArgumentException("Quantidade não pode ser negativa.");
            if (quantidade > Estoque)
                throw new InvalidOperationException("Estoque insuficiente.");
            Estoque -= quantidade;
        }
    }
}
