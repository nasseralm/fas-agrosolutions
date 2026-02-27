using FAS.Domain.Entities;
using FAS.Infra.Data.EntitiesConfiguration;
using Microsoft.EntityFrameworkCore;

namespace FAS.Infra.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<StoredEvent> StoredEvent { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            _ = new UsuarioConfiguration(builder);
            _ = new StoredEventConfiguration(builder);
        }
    }
}
