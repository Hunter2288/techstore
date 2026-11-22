using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using Platform.EventBus;
using System.Text.Json;

namespace Reviews.Infrastructure.Persistence;

public sealed class ProductSnapshotHandler(MongoDbContext context, ILogger<ProductSnapshotHandler> logger)
    : IIntegrationEventHandler
{
    public async Task HandleAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        if (integrationEvent is not IProductSnapshotEvent snapshot)
            throw new JsonException($"Unsupported event {integrationEvent.GetType().Name}");
        var marker = new BsonDocument("_id", integrationEvent.EventId.ToString());
        if (await context.ProcessedEvents.Find(marker).AnyAsync(cancellationToken))
        {
            logger.LogInformation("Duplicate {EventType} {EventId} skipped for product {ProductId}",
                integrationEvent.GetType().Name, integrationEvent.EventId, snapshot.ProductId);
            return;
        }

        if (snapshot.ProductId <= int.MaxValue)
        {
            var productId = (int)snapshot.ProductId;
            var newerOrSame = Builders<ReviewDocument>.Filter.Or(
                Builders<ReviewDocument>.Filter.Exists(x => x.ProductNameUpdatedAt, false),
                Builders<ReviewDocument>.Filter.Lt(x => x.ProductNameUpdatedAt, integrationEvent.OccurredAt));
            var filter = Builders<ReviewDocument>.Filter.Eq(x => x.ProductId, productId) & newerOrSame;
            var update = Builders<ReviewDocument>.Update
                .Set(x => x.ProductName, snapshot.ProductName)
                .Set(x => x.ProductNameUpdatedAt, integrationEvent.OccurredAt);
            var result = await context.Reviews.UpdateManyAsync(filter, update, cancellationToken: cancellationToken);
            logger.LogInformation("Updated {ReviewCount} review product names for product {ProductId}",
                result.ModifiedCount, snapshot.ProductId);
        }

        try
        {
            await context.ProcessedEvents.InsertOneAsync(
                new BsonDocument { ["_id"] = integrationEvent.EventId.ToString(), ["processedAt"] = DateTime.UtcNow },
                cancellationToken: cancellationToken);
        }
        catch (MongoWriteException exception) when (exception.WriteError?.Code == 11000)
        {
            logger.LogInformation("Concurrent duplicate event {EventId} recorded", integrationEvent.EventId);
        }
    }
}
