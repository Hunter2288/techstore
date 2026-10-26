using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Reviews.Infrastructure.Persistence;

namespace Reviews.Infrastructure.Seeding;

public sealed class SchemaMigrationService : IHostedService
{
    private readonly MongoDbContext _context;
    private readonly ILogger<SchemaMigrationService> _logger;

    public SchemaMigrationService(MongoDbContext context, ILogger<SchemaMigrationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var collections = await _context.Database.ListCollectionNames().ToListAsync(cancellationToken);
        if (!collections.Contains("reviews", StringComparer.OrdinalIgnoreCase))
            return;

        var filter = Builders<ReviewDocument>.Filter.Lt(x => x.SchemaVersion, 2);
        var update = Builders<ReviewDocument>.Update
            .Set(x => x.HelpfulVotes, 0)
            .Set(x => x.SchemaVersion, 2);

        var migrated = await _context.Reviews.UpdateManyAsync(filter, update, cancellationToken: cancellationToken);

        var missingSchemaVersion = Builders<ReviewDocument>.Filter.Exists(x => x.SchemaVersion, false);
        var missingFieldsUpdate = Builders<ReviewDocument>.Update
            .Set(x => x.HelpfulVotes, 0)
            .Set(x => x.SchemaVersion, 2)
            .Set(x => x.Version, 1);
        var missing = await _context.Reviews.UpdateManyAsync(
            missingSchemaVersion,
            missingFieldsUpdate,
            cancellationToken: cancellationToken);

        var missingHelpfulVotes = Builders<ReviewDocument>.Filter.Exists(x => x.HelpfulVotes, false);
        await _context.Reviews.UpdateManyAsync(
            missingHelpfulVotes,
            Builders<ReviewDocument>.Update.Set(x => x.HelpfulVotes, 0),
            cancellationToken: cancellationToken);

        if (migrated.ModifiedCount + missing.ModifiedCount > 0)
        {
            _logger.LogInformation(
                "Міграцію Reviews schemaVersion=2 виконано. Оновлено документів: {Count}",
                migrated.ModifiedCount + missing.ModifiedCount);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
