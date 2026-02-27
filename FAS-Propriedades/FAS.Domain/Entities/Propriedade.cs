using FAS.Domain.Validation;
using NetTopologySuite.Geometries;

namespace FAS.Domain.Entities
{
    public class Propriedade
    {
        public int Id { get; private set; }
        public int ProducerId { get; private set; }
        public string Nome { get; private set; }
        public string Codigo { get; private set; }
        public string DescricaoLocalizacao { get; private set; }
        public string Municipio { get; private set; }
        public string Uf { get; private set; }
        public decimal? AreaTotalHectares { get; private set; }
        public bool Ativa { get; private set; }

        public Geometry Localizacao { get; private set; }
        public string LocalizacaoGeoJson { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }

        public Propriedade() { }

        public Propriedade(
            int producerId,
            string nome,
            string codigo,
            string descricaoLocalizacao,
            string municipio,
            string uf,
            decimal? areaTotalHectares,
            Geometry localizacao,
            string localizacaoGeoJson)
        {
            DomainExceptionValidation.When(producerId <= 0, "ProducerId inválido.");
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(nome), "Nome é obrigatório.");
            DomainExceptionValidation.When(nome.Length > 200, "Nome deve ter no máximo 200 caracteres.");
            DomainExceptionValidation.When(!string.IsNullOrWhiteSpace(codigo) && codigo.Length > 50, "Código deve ter no máximo 50 caracteres.");
            DomainExceptionValidation.When(!string.IsNullOrWhiteSpace(municipio) && municipio.Length > 120, "Município deve ter no máximo 120 caracteres.");
            DomainExceptionValidation.When(!string.IsNullOrWhiteSpace(uf) && uf.Length != 2, "UF deve ter 2 caracteres (ex: SP).");
            DomainExceptionValidation.When(areaTotalHectares.HasValue && areaTotalHectares.Value <= 0, "Área total (ha) deve ser maior que zero.");

            ProducerId = producerId;
            Nome = nome.Trim();
            Codigo = codigo?.Trim();
            DescricaoLocalizacao = descricaoLocalizacao?.Trim();
            Municipio = municipio?.Trim();
            Uf = uf?.Trim()?.ToUpperInvariant();
            AreaTotalHectares = areaTotalHectares;
            Ativa = true;
            Localizacao = localizacao;
            LocalizacaoGeoJson = localizacaoGeoJson;
            CreatedAtUtc = DateTime.UtcNow;
        }

        public void Atualizar(
            string nome,
            string codigo,
            string descricaoLocalizacao,
            string municipio,
            string uf,
            decimal? areaTotalHectares,
            Geometry localizacao,
            string localizacaoGeoJson)
        {
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(nome), "Nome é obrigatório.");
            DomainExceptionValidation.When(nome.Length > 200, "Nome deve ter no máximo 200 caracteres.");
            DomainExceptionValidation.When(!string.IsNullOrWhiteSpace(codigo) && codigo.Length > 50, "Código deve ter no máximo 50 caracteres.");
            DomainExceptionValidation.When(!string.IsNullOrWhiteSpace(municipio) && municipio.Length > 120, "Município deve ter no máximo 120 caracteres.");
            DomainExceptionValidation.When(!string.IsNullOrWhiteSpace(uf) && uf.Length != 2, "UF deve ter 2 caracteres (ex: SP).");
            DomainExceptionValidation.When(areaTotalHectares.HasValue && areaTotalHectares.Value <= 0, "Área total (ha) deve ser maior que zero.");

            Nome = nome.Trim();
            Codigo = codigo?.Trim();
            DescricaoLocalizacao = descricaoLocalizacao?.Trim();
            Municipio = municipio?.Trim();
            Uf = uf?.Trim()?.ToUpperInvariant();
            AreaTotalHectares = areaTotalHectares;
            Localizacao = localizacao;
            LocalizacaoGeoJson = localizacaoGeoJson;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Inativar()
        {
            Ativa = false;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void Ativar()
        {
            Ativa = true;
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
