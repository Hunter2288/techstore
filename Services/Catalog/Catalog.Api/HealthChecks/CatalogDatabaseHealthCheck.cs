using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Catalog.Api;

public sealed class CatalogDatabaseHealthCheck(IConfiguration configuration) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(configuration.GetConnectionString("CatalogDb"));
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync(cancellationToken);
            return HealthCheckResult.Healthy("Catalog SQL Server is reachable");
        }
        catch (Exception exception) { return HealthCheckResult.Unhealthy("Catalog SQL Server is unavailable", exception); }
    }
}
