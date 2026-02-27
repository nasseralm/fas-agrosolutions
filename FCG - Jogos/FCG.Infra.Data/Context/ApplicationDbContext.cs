using FCG.Domain.Entities;
using FCG.Infra.Data.EntitiesConfiguration;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infra.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }
        public DbSet<Jogo> Jogo { get; set; }
        public DbSet<Compra> Compra { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            _ = new JogoConfiguration(builder);
        }    
    }
}
