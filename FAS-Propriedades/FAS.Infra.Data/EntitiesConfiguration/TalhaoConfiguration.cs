using FAS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FAS.Infra.Data.EntitiesConfiguration
{
    public class TalhaoConfiguration
    {
        private static ModelBuilder _builder;

        public TalhaoConfiguration(ModelBuilder builder)
        {
            _builder = builder;
            Config();
        }

        private static void Config()
        {
            _builder.Entity<Talhao>()
                .HasKey(x => x.Id);

            _builder.Entity<Talhao>()
                .Property(x => x.PropriedadeId)
                .IsRequired();

            _builder.Entity<Talhao>()
                .Property(x => x.ProducerId)
                .IsRequired();

            _builder.Entity<Talhao>()
                .Property(x => x.Nome)
                .HasMaxLength(200)
                .IsRequired();

            _builder.Entity<Talhao>()
                .Property(x => x.Codigo)
                .HasMaxLength(50)
                .IsRequired(false);

            _builder.Entity<Talhao>()
                .Property(x => x.Cultura)
                .HasMaxLength(100)
                .IsRequired();

            _builder.Entity<Talhao>()
                .Property(x => x.Variedade)
                .HasMaxLength(100)
                .IsRequired(false);

            _builder.Entity<Talhao>()
                .Property(x => x.Safra)
                .HasMaxLength(20)
                .IsRequired(false);

            _builder.Entity<Talhao>()
                .Property(x => x.AreaHectares)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            _builder.Entity<Talhao>()
                .Property(x => x.Ativo)
                .IsRequired();

            _builder.Entity<Talhao>()
                .Property(x => x.Delimitacao)
                .IsRequired();

            _builder.Entity<Talhao>()
                .Property(x => x.DelimitacaoGeoJson)
                .IsRequired(false);

            _builder.Entity<Talhao>()
                .Property(x => x.CreatedAtUtc)
                .IsRequired();

            _builder.Entity<Talhao>()
                .Property(x => x.UpdatedAtUtc)
                .IsRequired(false);

            _builder.Entity<Talhao>()
                .HasOne(x => x.Propriedade)
                .WithMany()
                .HasForeignKey(x => x.PropriedadeId)
                .OnDelete(DeleteBehavior.Cascade);

            _builder.Entity<Talhao>()
                .HasIndex(x => new { x.PropriedadeId, x.Nome })
                .IsUnique(false);

            _builder.Entity<Talhao>()
                .HasIndex(x => new { x.PropriedadeId, x.Codigo })
                .IsUnique(false);
        }
    }
}
