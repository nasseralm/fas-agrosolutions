using Agro.DataReceiver.Application.Interfaces;
using Agro.DataReceiver.Domain.Entities;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Agro.DataReceiver.Infrastructure.SqlServer;

public sealed class TalhaoRepository : ITalhaoRepository
{
    private readonly string _connectionString;
    private readonly ILogger<TalhaoRepository> _logger;

    public TalhaoRepository(IConfiguration configuration, ILogger<TalhaoRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("ConnectionStrings:SqlServer not configured in appsettings.json");
        _logger = logger;
    }

    public async Task<bool> ExistsAsync(string talhaoId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);

            const string sql = """
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Talhoes WHERE Id = @TalhaoId AND Ativo = 1
                ) THEN 1 ELSE 0 END
                """;

            var exists = await connection.ExecuteScalarAsync<bool>(sql, new { TalhaoId = talhaoId });

            _logger.LogDebug("Talhao {TalhaoId} exists: {Exists}", talhaoId, exists);

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of talhao {TalhaoId}", talhaoId);
            throw;
        }
    }

    public async Task<IReadOnlyList<Talhao>> GetActiveTalhoesWithGeoJsonAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);

            const string sql = """
                SELECT Id, Nome, Ativo, GeoJson, UpdatedAt 
                FROM Talhoes 
                WHERE Ativo = 1 AND GeoJson IS NOT NULL
                """;

            var talhoes = await connection.QueryAsync<Talhao>(sql);

            _logger.LogDebug("Loaded {Count} active talhoes with GeoJSON", talhoes.Count());

            return talhoes.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading talhoes with GeoJSON");
            throw;
        }
    }
}
