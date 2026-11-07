using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace Orders.Api.Infrastructure;

public sealed class OrdersDatabaseBootstrapper(
    string connectionString,
    ILogger<OrdersDatabaseBootstrapper> logger) : IHostedService
{
    private static readonly string[] Scripts = ["schema.sql", "procedures.sql", "seed.sql"];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var scriptsPath = Path.Combine(AppContext.BaseDirectory, "db", "p1");
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        foreach (var file in Scripts)
        {
            var path = Path.Combine(scriptsPath, file);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Orders database bootstrap script was not found: {path}");

            var content = await File.ReadAllTextAsync(path, cancellationToken);
            foreach (var batch in Regex.Split(content, @"(?im)^\s*GO\s*(?:--.*)?$")
                         .Where(batch => !string.IsNullOrWhiteSpace(batch)))
            {
                await using var command = connection.CreateCommand();
                command.CommandText = batch;
                command.CommandTimeout = 120;
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        logger.LogInformation("Orders database schema and seed data are ready");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
