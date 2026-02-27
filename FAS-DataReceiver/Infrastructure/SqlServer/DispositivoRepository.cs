using Agro.DataReceiver.Application.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Agro.DataReceiver.Infrastructure.SqlServer;

public sealed class DispositivoRepository : IDispositivoRepository
{
    private readonly string _connectionString;
    private readonly ILogger<DispositivoRepository> _logger;

    public DispositivoRepository(IConfiguration configuration, ILogger<DispositivoRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("SqlServer") 
            ?? throw new InvalidOperationException("ConnectionStrings:SqlServer not configured in appsettings.json");
        _logger = logger;
    }

    public async Task<string?> GetTalhaoIdByDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            
            const string sql = """
                SELECT TalhaoId 
                FROM Dispositivos 
                WHERE DeviceId = @DeviceId AND Ativo = 1
                """;

            var talhaoId = await connection.QueryFirstOrDefaultAsync<string>(sql, new { DeviceId = deviceId });
            
            _logger.LogDebug("Resolved device {DeviceId} to talhao {TalhaoId}", deviceId, talhaoId);
            
            return talhaoId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving talhao for device {DeviceId}", deviceId);
            throw;
        }
    }

    public async Task<IReadOnlyList<DeviceMappingEntry>> GetMappingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            const string sql = """
                SELECT TalhaoId, DeviceId
                FROM Dispositivos
                WHERE Ativo = 1
                ORDER BY TalhaoId
                """;
            var rows = await connection.QueryAsync<(string TalhaoId, string DeviceId)>(sql, cancellationToken);
            return rows.Select(r => new DeviceMappingEntry { TalhaoId = r.TalhaoId, DeviceId = r.DeviceId }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading device mapping");
            throw;
        }
    }
}
