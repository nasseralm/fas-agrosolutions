using FAS.Domain.Entities;
using FAS.Infra.Data.EntitiesConfiguration;
using Microsoft.EntityFrameworkCore;

namespace FAS.Infra.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Propriedade> Propriedade { get; set; }
        public DbSet<Talhao> Talhao { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            _ = new PropriedadeConfiguration(builder);
            _ = new TalhaoConfiguration(builder);

            // SQLite dev: sem SpatiaLite, então não mapeamos colunas Geometry (persistimos GeoJSON em texto).
            if (Database.ProviderName != null &&
                Database.ProviderName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                builder.Entity<Propriedade>().Ignore(p => p.Localizacao);
                builder.Entity<Talhao>().Ignore(t => t.Delimitacao);
            }
        }
    }
}
