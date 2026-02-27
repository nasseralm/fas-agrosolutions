using FCG.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infra.Data.EntitiesConfiguration
{
    public class JogoConfiguration
    {
        private static ModelBuilder _builder;
        public JogoConfiguration(ModelBuilder builder)
        {
            _builder = builder;
            Config();
        }

        private static void Config()
        {
            _builder.Entity<Jogo>()
                .HasKey(x => x.Id);

            _builder.Entity<Jogo>()
                .Property(x => x.Nome)
                .HasMaxLength(200)
                .IsRequired();

            _builder.Entity<Jogo>()
                .Property(x => x.Descricao)
                .HasMaxLength(1000)
                .IsRequired();

            _builder.Entity<Jogo>()
                .Property(x => x.Genero)
                .HasMaxLength(100)
                .IsRequired();

            _builder.Entity<Jogo>()
                .Property(x => x.Preco)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            _builder.Entity<Jogo>()
                .Property(x => x.DataLancamento)
                .IsRequired();

            _builder.Entity<Jogo>()
                .Property(x => x.Desenvolvedor)
                .HasMaxLength(200)
                .IsRequired();

            _builder.Entity<Jogo>()
                .Property(x => x.Distribuidora)
                .HasMaxLength(200)
                .IsRequired();

            _builder.Entity<Jogo>()
                .Property(x => x.ClassificacaoIndicativa)
                .HasMaxLength(20)
                .IsRequired();

            _builder.Entity<Jogo>()
                .Property(x => x.Estoque)
                .IsRequired();

            _builder.Entity<Jogo>()
                .Property(x => x.Plataforma)
                .HasMaxLength(100)
                .IsRequired();
        }
    }
}
