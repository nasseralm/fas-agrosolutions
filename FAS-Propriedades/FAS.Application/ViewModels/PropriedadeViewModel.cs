using System.Text.Json;
using System.Text.Json.Serialization;

namespace FAS.API.Models
{
    public class PropriedadeViewModel
    {
        public int Id { get; private set; }
        public string Nome { get; private set; }
        public string Codigo { get; private set; }
        public string DescricaoLocalizacao { get; private set; }
        public string Municipio { get; private set; }
        public string Uf { get; private set; }
        public decimal? AreaTotalHectares { get; private set; }
        public bool Ativa { get; private set; }
        public JsonElement? Localizacao { get; set; }

        [JsonIgnore]
        public int ProducerId { get; private set; }
    }
}
