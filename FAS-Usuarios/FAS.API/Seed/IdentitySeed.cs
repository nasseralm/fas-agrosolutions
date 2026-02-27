using System.Text;
using FAS.Domain.Entities;
using FAS.Domain.ValueObjects;
using FAS.Infra.Data.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace FAS.API.Seed
{
    /// <summary>
    /// Seed do produtor rural para testes/demonstração (T-006).
    /// Credenciais: produtor@demo.com / Senha123!
    /// </summary>
    public static class IdentitySeed
    {
        public const string SeedEmail = "produtor@demo.com";
        public const string SeedPassword = "Senha123!";

        public static IHost SeedIdentityIfEmpty(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("FAS.API.Seed.IdentitySeed");
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                var existe = db.Usuario.Any(u => u.EmailUsuario.EmailAddress == SeedEmail);
                if (existe)
                {
                    logger.LogInformation("Usuário seed já existe: {Email}", SeedEmail);
                    return host;
                }

                var email = new Email(SeedEmail);
                var usuario = new Usuario(0, "Produtor Demo", email, StatusProdutor.Ativo);

                using var hmac = new HMACSHA512();
                usuario.AlterarSenha(
                    hmac.ComputeHash(Encoding.UTF8.GetBytes(SeedPassword)),
                    hmac.Key
                );

                db.Usuario.Add(usuario);
                db.SaveChanges();
                logger.LogInformation("Usuário seed criado: {Email}", SeedEmail);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao executar seed do Identity.");
            }

            return host;
        }
    }
}
