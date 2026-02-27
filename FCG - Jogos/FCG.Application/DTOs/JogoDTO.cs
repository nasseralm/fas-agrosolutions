using FCG.Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FCG.Application.DTOs
{
    public class JogoDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = DataAnnotationsMessages.REQUIRED)]
        [StringLength(200, ErrorMessage = DataAnnotationsMessages.MAXLENGHT)]
        public string Nome { get; set; }

        [Required(ErrorMessage = DataAnnotationsMessages.REQUIRED)]
        [StringLength(1000, ErrorMessage = DataAnnotationsMessages.MAXLENGHT)]
        public string Descricao { get; set; }

        [Required(ErrorMessage = DataAnnotationsMessages.REQUIRED)]
        [StringLength(100, ErrorMessage = DataAnnotationsMessages.MAXLENGHT)]
        public string Genero { get; set; }

        [Required(ErrorMessage = DataAnnotationsMessages.REQUIRED)]
        public decimal Preco { get; set; }

        [Required(ErrorMessage = DataAnnotationsMessages.REQUIRED)]
        public DateTime DataLancamento { get; set; }

        [Required(ErrorMessage = DataAnnotationsMessages.REQUIRED)]
        [StringLength(200, ErrorMessage = DataAnnotationsMessages.MAXLENGHT)]
        public string Desenvolvedor { get; set; }

        [Required(ErrorMessage = DataAnnotationsMessages.REQUIRED)]
        [StringLength(200, ErrorMessage = DataAnnotationsMessages.MAXLENGHT)]
        public string Distribuidora { get; set; }

        [Required(ErrorMessage = DataAnnotationsMessages.REQUIRED)]
        [StringLength(20, ErrorMessage = DataAnnotationsMessages.MAXLENGHT)]
        public string ClassificacaoIndicativa { get; set; }

        [Required(ErrorMessage = DataAnnotationsMessages.REQUIRED)]
        public int Estoque { get; set; }

        [Required(ErrorMessage = DataAnnotationsMessages.REQUIRED)]
        [StringLength(100, ErrorMessage = DataAnnotationsMessages.MAXLENGHT)]
        public string Plataforma { get; set; }
    }
}
