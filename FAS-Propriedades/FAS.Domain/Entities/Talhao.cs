using FAS.Domain.Validation;
using NetTopologySuite.Geometries;

namespace FAS.Domain.Entities
{
    public class Talhao
    {
        public int Id { get; private set; }
        public int PropriedadeId { get; private set; }
        public int ProducerId { get; private set; }
        public string Nome { get; private set; }
        public string Codigo { get; private set; }
        public string Cultura { get; private set; }
        public string Variedade { get; private set; }
        public string Safra { get; private set; }
        public decimal? AreaHectares { get; private set; }
        public bool Ativo { get; private set; }

        public Geometry Delimitacao { get; private set; }
        public string DelimitacaoGeoJson { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }

        public Propriedade Propriedade { get; private set; }

        public Talhao() { }

        public Talhao(
            int propriedadeId,
            int producerId,
            string nome,
            string codigo,
            string cultura,
            string variedade,
            string safra,
            decimal? areaHectares,
            Geometry delimitacao,
            string delimitacaoGeoJson)
        {
            DomainExceptionValidation.When(propriedadeId <= 0, "PropriedadeId inválido.");
            DomainExceptionValidation.When(producerId <= 0, "ProducerId inválido.");
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(nome), "Nome é obrigatório.");
            DomainExceptionValidation.When(nome.Length > 200, "Nome deve ter no máximo 200 caracteres.");
            DomainExceptionValidation.When(!string.IsNullOrWhiteSpace(codigo) && codigo.Length > 50, "Código deve ter no máximo 50 caracteres.");
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(cultura), "Cultura é obrigatória.");
            DomainExceptionValidation.When(cultura.Length > 100, "Cultura deve ter no máximo 100 caracteres.");
            DomainExceptionValidation.When(!string.IsNullOrWhiteSpace(variedade) && variedade.Length > 100, "Variedade deve ter no máximo 100 caracteres.");
            DomainExceptionValidation.When(!string.IsNullOrWhiteSpace(safra) && safra.Length > 20, "Safra deve ter no máximo 20 caracteres.");
            DomainExceptionValidation.When(areaHectares.HasValue && areaHectares.Value <= 0, "Área (ha) deve ser maior que zero.");
            DomainExceptionValidation.When(delimitacao == null, "Delimitação é obrigatória.");

            PropriedadeId = propriedadeId;
            ProducerId = producerId;
            Nome = nome.Trim();
            Codigo = codigo?.Trim();
            Cultura = cultura.Trim();
            Variedade = variedade?.Trim();
            Safra = safra?.Trim();
            AreaHectares = areaHectares;
            Ativo = true;
            Delimitacao = delimitacao;
            DelimitacaoGeoJson = delimitacaoGeoJson;
            CreatedAtUtc = DateTime.UtcNow;
        }

        public void Atualizar(
            string nome,
            string codigo,
            string cultura,
            string variedade,
            string safra,
            decimal? areaHectares,
            Geometry delimitacao,
            string delimitacaoGeoJson)
        {
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(nome), "Nome é obrigatório.");
            DomainExceptionValidation.When(nome.Length > 200, "Nome deve ter no máximo 200 caracteres.");
            DomainExceptionValidation.When(!string.IsNullOrWhiteSpace(codigo) && codigo.Length > 50, "Código deve ter no máximo 50 caracteres.");
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(cultura), "Cultura é obrigatória.");
            DomainExceptionValidation.When(cultura.Length > 100, "Cultura deve ter no máximo 100 caracteres.");
            DomainExceptionValidation.When(!string.IsNullOrWhiteSpace(variedade) && variedade.Length > 100, "Variedade deve ter no máximo 100 caracteres.");
            DomainExceptionValidation.When(!string.IsNullOrWhiteSpace(safra) && safra.Length > 20, "Safra deve ter no máximo 20 caracteres.");
            DomainExceptionValidation.When(areaHectares.HasValue && areaHectares.Value <= 0, "Área (ha) deve ser maior que zero.");
            DomainExceptionValidation.When(delimitacao == null, "Delimitação é obrigatória.");

            Nome = nome.Trim();
            Codigo = codigo?.Trim();
            Cultura = cultura.Trim();
            Variedade = variedade?.Trim();
            Safra = safra?.Trim();
            AreaHectares = areaHectares;
            Delimitacao = delimitacao;
            DelimitacaoGeoJson = delimitacaoGeoJson;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Inativar()
        {
            Ativo = false;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Ativar()
        {
            Ativo = true;
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
