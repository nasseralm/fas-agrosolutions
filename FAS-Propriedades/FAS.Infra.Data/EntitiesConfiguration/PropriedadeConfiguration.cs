using FAS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FAS.Infra.Data.EntitiesConfiguration
{
    public class PropriedadeConfiguration
    {
        private static ModelBuilder _builder;

        public PropriedadeConfiguration(ModelBuilder builder)
        {
            _builder = builder;
            Config();
        }

        private static void Config()
        {
            _builder.Entity<Propriedade>()
                .HasKey(x => x.Id);

            _builder.Entity<Propriedade>()
                .Property(x => x.ProducerId)
                .IsRequired();

            _builder.Entity<Propriedade>()
                .Property(x => x.Nome)
                .HasMaxLength(200)
                .IsRequired();

            _builder.Entity<Propriedade>()
                .Property(x => x.Codigo)
                .HasMaxLength(50)
                .IsRequired(false);

            _builder.Entity<Propriedade>()
                .Property(x => x.DescricaoLocalizacao)
                .HasMaxLength(500)
                .IsRequired(false);

            _builder.Entity<Propriedade>()
                .Property(x => x.Municipio)
                .HasMaxLength(120)
                .IsRequired(false);

            _builder.Entity<Propriedade>()
                .Property(x => x.Uf)
                .HasMaxLength(2)
                .IsRequired(false);

            _builder.Entity<Propriedade>()
                .Property(x => x.AreaTotalHectares)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            _builder.Entity<Propriedade>()
                .Property(x => x.Ativa)
                .IsRequired();

            _builder.Entity<Propriedade>()
                .Property(x => x.Localizacao)
                .IsRequired(false);

            _builder.Entity<Propriedade>()
                .Property(x => x.LocalizacaoGeoJson)
                .IsRequired(false);

            _builder.Entity<Propriedade>()
                .Property(x => x.CreatedAtUtc)
                .IsRequired();

            _builder.Entity<Propriedade>()
                .Property(x => x.UpdatedAtUtc)
                .IsRequired(false);

            _builder.Entity<Propriedade>()
                .HasIndex(x => new { x.ProducerId, x.Nome })
                .IsUnique(false);

            _builder.Entity<Propriedade>()
                .HasIndex(x => new { x.ProducerId, x.Codigo })
                .IsUnique(false);
        }
    }
}
