using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Orders.Dal.Interfaces;

namespace Orders.Dal.Repositories;

public sealed class ProductSnapshotRepository(SqlConnection connection, Func<SqlTransaction?> transaction)
    : IProductSnapshotRepository
{
    public async Task UpsertAsync(long productId, string name, decimal price, bool isActive, DateTime occurredAt,
        CancellationToken ct)
    {
        const string sql = """
            MERGE dbo.ProductRefs WITH (HOLDLOCK) AS target
            USING (SELECT @ProductId AS ProductId, @Name AS Name, @Price AS Price,
                          @IsActive AS IsActive, @OccurredAt AS OccurredAt) AS source
               ON target.ProductId = source.ProductId
            WHEN MATCHED AND target.UpdatedAt < source.OccurredAt THEN
                UPDATE SET Name = source.Name, Price = source.Price, IsActive = source.IsActive,
                           UpdatedAt = source.OccurredAt
            WHEN NOT MATCHED THEN
                INSERT (ProductId, Name, Price, IsActive, UpdatedAt)
                VALUES (source.ProductId, source.Name, source.Price, source.IsActive, source.OccurredAt);
            """;
        await connection.ExecuteAsync(new CommandDefinition(sql,
            new { ProductId = productId, Name = name, Price = price, IsActive = isActive, OccurredAt = occurredAt },
            transaction(), cancellationToken: ct));
    }
}

public sealed class ProcessedEventRepository(SqlConnection connection, Func<SqlTransaction?> transaction)
    : IProcessedEventRepository
{
    public async Task<bool> TryAddAsync(Guid eventId, CancellationToken ct)
    {
        try
        {
            await connection.ExecuteAsync(new CommandDefinition(
                "INSERT dbo.ProcessedEvents (EventId) VALUES (@EventId)", new { EventId = eventId },
                transaction(), cancellationToken: ct));
            return true;
        }
        catch (SqlException exception) when (exception.Number is 2601 or 2627)
        {
            return false;
        }
    }
}
