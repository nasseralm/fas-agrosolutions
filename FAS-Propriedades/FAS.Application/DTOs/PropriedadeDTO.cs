using FAS.Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace FAS.Application.DTOs
{
    public class PropriedadeDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = DataAnnotationsMessages.REQUIRED)]
        [StringLength(200, ErrorMessage = DataAnnotationsMessages.MAXLENGHT)]
        public string Nome { get; set; }

        [StringLength(50, ErrorMessage = DataAnnotationsMessages.MAXLENGHT)]
        public string Codigo { get; set; }

        [StringLength(500, ErrorMessage = DataAnnotationsMessages.MAXLENGHT)]
        public string DescricaoLocalizacao { get; set; }

        [StringLength(120, ErrorMessage = DataAnnotationsMessages.MAXLENGHT)]
        public string Municipio { get; set; }

        [StringLength(2, ErrorMessage = DataAnnotationsMessages.STRINGLENGHTFIX, MinimumLength = 2)]
        public string Uf { get; set; }

        [Range(0.0001, double.MaxValue, ErrorMessage = DataAnnotationsMessages.RANGE)]
        public decimal? AreaTotalHectares { get; set; }

        /// <summary>GeoJSON Geometry (Point/Polygon/MultiPolygon).</summary>
        public JsonElement? Localizacao { get; set; }
    }
}
