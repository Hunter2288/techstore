using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using Reviews.Infrastructure.Persistence;

namespace Reviews.Infrastructure.Seeding;

public sealed class IndexCreationService : IHostedService
{
    private readonly MongoDbContext _context;

    public IndexCreationService(MongoDbContext context)
    {
        _context = context;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var reviewIndexes = new[]
        {
            new CreateIndexModel<ReviewDocument>(
                Builders<ReviewDocument>.IndexKeys.Ascending(x => x.ProductId).Descending(x => x.CreatedAt),
                new CreateIndexOptions { Name = "productId_1_createdAt_-1" }),
            new CreateIndexModel<ReviewDocument>(
                Builders<ReviewDocument>.IndexKeys.Ascending("author.userId"),
                new CreateIndexOptions { Name = "author.userId_1" }),
            new CreateIndexModel<ReviewDocument>(
                Builders<ReviewDocument>.IndexKeys.Ascending(x => x.ProductId).Ascending("author.userId"),
                new CreateIndexOptions { Unique = true, Name = "productId_1_author.userId_1" })
        };
        await _context.Reviews.Indexes.CreateManyAsync(reviewIndexes, cancellationToken);

        var discussionIndexes = new[]
        {
            new CreateIndexModel<BsonDocument>(
                new BsonDocument { ["productId"] = 1, ["createdAt"] = -1 },
                new CreateIndexOptions { Name = "productId_1_createdAt_-1" })
        };
        await _context.Discussions.Indexes.CreateManyAsync(discussionIndexes, cancellationToken);

        var commentIndexes = new[]
        {
            new CreateIndexModel<BsonDocument>(
                new BsonDocument { ["discussionId"] = 1, ["createdAt"] = 1 },
                new CreateIndexOptions { Name = "discussionId_1_createdAt_1" }),
            new CreateIndexModel<BsonDocument>(
                new BsonDocument("parentId", 1),
                new CreateIndexOptions { Name = "parentId_1" })
        };
        await _context.Comments.Indexes.CreateManyAsync(commentIndexes, cancellationToken);

        var ratingIndexes = new[]
        {
            new CreateIndexModel<BsonDocument>(
                new BsonDocument("productId", 1),
                new CreateIndexOptions { Unique = true, Name = "productId_1" })
        };
        await _context.Ratings.Indexes.CreateManyAsync(ratingIndexes, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
