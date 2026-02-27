using System.Text.Json;
using System.Text.Json.Serialization;

namespace FAS.API.Models
{
    public class TalhaoViewModel
    {
        public int Id { get; private set; }
        public int PropriedadeId { get; private set; }
        public string Nome { get; private set; }
        public string Codigo { get; private set; }
        public string Cultura { get; private set; }
        public string Variedade { get; private set; }
        public string Safra { get; private set; }
        public decimal? AreaHectares { get; private set; }
        public bool Ativo { get; private set; }
        public JsonElement? Delimitacao { get; set; }

        [JsonIgnore]
        public int ProducerId { get; private set; }
    }
}
