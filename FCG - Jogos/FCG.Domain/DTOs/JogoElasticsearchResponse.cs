namespace FCG.Domain.DTOs
{
    /// <summary>
    /// DTO específico para respostas do Elasticsearch
    /// Não inclui o campo Estoque pois é um dado transacional
    /// </summary>
    public class JogoElasticsearchResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string Genero { get; set; }
        public decimal Preco { get; set; }
        public DateTime DataLancamento { get; set; }
        public string Desenvolvedor { get; set; }
        public string Distribuidora { get; set; }
        public string ClassificacaoIndicativa { get; set; }
        public string Plataforma { get; set; }
    }
}