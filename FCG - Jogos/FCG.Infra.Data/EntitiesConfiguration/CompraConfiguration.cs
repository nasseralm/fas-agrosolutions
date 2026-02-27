using FCG.Domain.Entities;
using FCG.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infra.Data.EntitiesConfiguration
{
    public class CompraConfiguration
    {
        public CompraConfiguration(ModelBuilder builder)
        {
            builder.Entity<Compra>()
                .ToTable("Compra");

            builder.Entity<Compra>()
                .HasKey(x => x.Id);

            builder.Entity<Compra>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Entity<Compra>()
                .Property(x => x.JogoId)
                .IsRequired();

            builder.Entity<Compra>()
                .Property(x => x.UsuarioId)
                .IsRequired();

            builder.Entity<Compra>()
                .Property(x => x.Quantidade)
                .IsRequired();

            builder.Entity<Compra>()
                .Property(x => x.ValorTotal)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Entity<Compra>()
                .Property(x => x.DataCompra)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Entity<Compra>()
                .Property(x => x.FormaPagamentoId)
                .IsRequired();

            builder.Entity<Compra>()
                .Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired()
                .HasDefaultValue(StatusCompra.Pendente);

            builder.Entity<Compra>()
                .Property(x => x.PaymentId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired(false);

            builder.Entity<Compra>()
                .Property(x => x.MotivoRecusa)
                .HasMaxLength(255)
                .IsUnicode(false)
                .IsRequired(false);

            builder.Entity<Compra>()
                .Property(x => x.DataStatus)
                .HasColumnType("datetime2")
                .IsRequired()
                .HasDefaultValueSql("SYSDATETIME()");
        }
    }
}