using FAS.Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace FAS.Application.DTOs
{
    public class TalhaoDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = DataAnnotationsMessages.REQUIRED)]
        [Range(1, int.MaxValue, ErrorMessage = DataAnnotationsMessages.RANGE)]
        public int PropriedadeId { get; set; }

        [Required(ErrorMessage = DataAnnotationsMessages.REQUIRED)]
        [StringLength(200, ErrorMessage = DataAnnotationsMessages.MAXLENGHT)]
        public string Nome { get; set; }

        [StringLength(50, ErrorMessage = DataAnnotationsMessages.MAXLENGHT)]
        public string Codigo { get; set; }

        [Required(ErrorMessage = DataAnnotationsMessages.REQUIRED)]
        [StringLength(100, ErrorMessage = DataAnnotationsMessages.MAXLENGHT)]
        public string Cultura { get; set; }

        [StringLength(100, ErrorMessage = DataAnnotationsMessages.MAXLENGHT)]
        public string Variedade { get; set; }

        [StringLength(20, ErrorMessage = DataAnnotationsMessages.MAXLENGHT)]
        public string Safra { get; set; }

        [Range(0.0001, double.MaxValue, ErrorMessage = DataAnnotationsMessages.RANGE)]
        public decimal? AreaHectares { get; set; }

        /// <summary>GeoJSON Geometry (Polygon/MultiPolygon).</summary>
        public JsonElement? Delimitacao { get; set; }
    }
}
