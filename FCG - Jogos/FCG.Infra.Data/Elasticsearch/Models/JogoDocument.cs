using Nest;

namespace FCG.Infra.Data.Elasticsearch.Models
{
    [ElasticsearchType(IdProperty = nameof(Id))]
    public class JogoDocument
    {
        public int Id { get; set; }
        
        [Text(Analyzer = "standard")]
        public string Nome { get; set; }
        
        [Text(Analyzer = "standard")]
        public string Descricao { get; set; }
        
        [Keyword]
        public string Genero { get; set; }
        
        [Number(NumberType.ScaledFloat, ScalingFactor = 100)]
        public decimal Preco { get; set; }
        
        [Date]
        public DateTime DataLancamento { get; set; }
        
        [Text(Analyzer = "standard")]
        public string Desenvolvedor { get; set; }
        
        [Text(Analyzer = "standard")]
        public string Distribuidora { get; set; }
        
        [Keyword]
        public string ClassificacaoIndicativa { get; set; }
        
        [Keyword]
        public string Plataforma { get; set; }
        
        [Date]
        public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    }
}