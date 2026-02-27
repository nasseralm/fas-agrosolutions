using FAS.Domain.Entities;
using FAS.Infra.Data.Context;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

namespace FAS.API.Seed
{
    /// <summary>
    /// Seed de propriedade e talhões para o produtor demo (produtor@demo.com).
    /// Obtém o Id do usuário na tabela Usuario (mesmo DB AgroSolutions) para garantir correspondência com o JWT.
    /// </summary>
    public static class PropertiesSeed
    {
        public const string DemoUserEmail = "produtor@demo.com";

        public static IHost SeedPropertiesIfEmpty(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("FAS.API.Seed.PropertiesSeed");
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            try
            {
                var producerId = ObterProducerIdDemo(configuration, logger);
                if (producerId <= 0)
                {
                    logger.LogWarning("Seed de propriedades ignorado: usuário demo ({Email}) não encontrado na tabela Usuario.", DemoUserEmail);
                    return host;
                }

                var jaTem = db.Propriedade.Any(p => p.ProducerId == producerId);
                if (jaTem)
                {
                    logger.LogInformation("Seed de propriedades já existe para ProducerId {ProducerId}.", producerId);
                    return host;
                }

                var propriedade = new Propriedade(
                    producerId: producerId,
                    nome: "Fazenda Demo",
                    codigo: "FD01",
                    descricaoLocalizacao: "Área de demonstração",
                    municipio: "Brasília",
                    uf: "DF",
                    areaTotalHectares: 50m,
                    localizacao: null,
                    localizacaoGeoJson: null
                );
                db.Propriedade.Add(propriedade);
                db.SaveChanges();

                var geom = CreateDemoPolygon();
                var talhoes = new[]
                {
                    new Talhao(propriedade.Id, producerId, "Talhão 01", "T01", "Soja", "Intacta", "2024/25", 12.5m, geom, null),
                    new Talhao(propriedade.Id, producerId, "Talhão 02", "T02", "Milho", "P30F53", "2024/25", 8.2m, geom, null),
                    new Talhao(propriedade.Id, producerId, "Talhão 03", "T03", "Soja", "Intacta", "2024/25", 15.0m, geom, null),
                    new Talhao(propriedade.Id, producerId, "Talhão 04", "T04", "Algodão", "FM 966", "2024/25", 6.8m, geom, null),
                };
                foreach (var t in talhoes)
                    db.Talhao.Add(t);
                db.SaveChanges();

                logger.LogInformation("Seed de propriedades criado: Fazenda Demo e {Count} talhões para ProducerId {ProducerId} ({Email}).", talhoes.Length, producerId, DemoUserEmail);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao executar seed de Propriedades/Talhões.");
            }

            return host;
        }

        /// <summary>
        /// Obtém o Id do usuário demo na tabela Usuario (mesmo DB AgroSolutions).
        /// Retorna 0 se a tabela não existir ou o usuário não for encontrado.
        /// </summary>
        private static int ObterProducerIdDemo(IConfiguration configuration, ILogger logger)
        {
            try
            {
                var connString = configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connString))
                    return 0;

                using var conn = new SqlConnection(connString);
                conn.Open();
                using var cmd = new SqlCommand("SELECT Id FROM Usuario WHERE Email = @email", conn);
                cmd.Parameters.AddWithValue("@email", DemoUserEmail);
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value && int.TryParse(result.ToString(), out var id))
                    return id;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Não foi possível obter Id do usuário demo na tabela Usuario (tabela pode não existir neste DB).");
            }

            return 0;
        }

        private static Geometry CreateDemoPolygon()
        {
            var factory = new GeometryFactory(new PrecisionModel(PrecisionModels.Floating), 4326);
            var coords = new[]
            {
                new Coordinate(-47.0, -15.0),
                new Coordinate(-46.99, -15.0),
                new Coordinate(-46.99, -14.99),
                new Coordinate(-47.0, -14.99),
                new Coordinate(-47.0, -15.0)
            };
            var ring = factory.CreateLinearRing(coords);
            var polygon = factory.CreatePolygon(ring);
            polygon.SRID = 4326;
            return polygon;
        }
    }
}
