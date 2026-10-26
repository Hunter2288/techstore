using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using Reviews.Infrastructure.Persistence;

namespace Reviews.Infrastructure.Seeding;

public sealed class ReviewSeeder : IHostedService
{
    private static readonly ObjectId ReviewId1 = ObjectId.Parse("66f0c2000000000000000001");
    private static readonly ObjectId ReviewId2 = ObjectId.Parse("66f0c2000000000000000002");
    private static readonly ObjectId DiscussionId = ObjectId.Parse("66f0c3000000000000000001");
    private static readonly ObjectId CommentId1 = ObjectId.Parse("66f0c4000000000000000001");
    private static readonly ObjectId CommentId2 = ObjectId.Parse("66f0c4000000000000000002");

    private readonly MongoDbContext _context;

    public ReviewSeeder(MongoDbContext context)
    {
        _context = context;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var t0 = new DateTime(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc);

        await InsertReviewIfMissingAsync(new ReviewDocument
        {
            Id = ReviewId1.ToString(),
            ProductId = 1,
            ProductName = "Смартфон Samsung Galaxy S24",
            OrderId = 1,
            IsVerifiedPurchase = true,
            Author = new AuthorDocument { UserId = 1, DisplayName = "Іван Петренко" },
            Rating = 5,
            Title = "Відмінний флагман",
            Text = "Швидкий, гарна камера, батарея тримає день.",
            Pros = ["камера", "екран"],
            Cons = ["ціна"],
            Media = [new ReviewMediaDocument { Type = "photo", Url = "/media/r1-1.jpg" }],
            Details = new ReviewDetailsDocument { BatteryLifeHours = 14, CameraScore = 9 },
            Replies = [new ReviewReplyDocument
            {
                Author = new AuthorDocument { UserId = 100, DisplayName = "Підтримка TechStore" },
                Text = "Дякуємо за відгук!",
                CreatedAt = t0.AddMinutes(5)
            }],
            HelpfulVotes = 0,
            SchemaVersion = 2,
            Version = 1,
            CreatedAt = t0
        }, cancellationToken);

        await InsertReviewIfMissingAsync(new ReviewDocument
        {
            Id = ReviewId2.ToString(),
            ProductId = 2,
            ProductName = "Ноутбук Lenovo IdeaPad 5",
            OrderId = null,
            IsVerifiedPurchase = false,
            Author = new AuthorDocument { UserId = 2, DisplayName = "Олена Коваль" },
            Rating = 4,
            Title = "Добра ціна/якість",
            Text = "Зручна клавіатура, трохи шумить кулер.",
            Pros = ["клавіатура"],
            Cons = ["шум кулера"],
            Details = new ReviewDetailsDocument
            {
                KeyboardRating = 5,
                Thermals = "average",
                UsedFor = ["навчання", "офіс"]
            },
            HelpfulVotes = 0,
            SchemaVersion = 2,
            Version = 1,
            CreatedAt = t0.AddMinutes(10)
        }, cancellationToken);

        await InsertBsonIfMissingAsync(_context.Discussions, new BsonDocument
        {
            ["_id"] = DiscussionId,
            ["productId"] = 3,
            ["productName"] = "Навушники Sony WH-1000XM5",
            ["title"] = "Чи підтримують мультиточкове підключення?",
            ["createdBy"] = new BsonDocument { ["userId"] = 1, ["displayName"] = "Іван Петренко" },
            ["status"] = "open",
            ["commentCount"] = 2,
            ["lastComments"] = new BsonArray(),
            ["createdAt"] = new BsonDateTime(t0.AddMinutes(20))
        }, cancellationToken);

        await InsertBsonIfMissingAsync(_context.Comments, new BsonDocument
        {
            ["_id"] = CommentId1,
            ["discussionId"] = DiscussionId,
            ["parentId"] = BsonNull.Value,
            ["author"] = new BsonDocument { ["userId"] = 2, ["displayName"] = "Олена Коваль" },
            ["text"] = "Так, до двох пристроїв одночасно.",
            ["createdAt"] = new BsonDateTime(t0.AddMinutes(25))
        }, cancellationToken);

        await InsertBsonIfMissingAsync(_context.Comments, new BsonDocument
        {
            ["_id"] = CommentId2,
            ["discussionId"] = DiscussionId,
            ["parentId"] = CommentId1,
            ["author"] = new BsonDocument { ["userId"] = 1, ["displayName"] = "Іван Петренко" },
            ["text"] = "Дякую!",
            ["createdAt"] = new BsonDateTime(t0.AddMinutes(30))
        }, cancellationToken);

        var lastComments = await _context.Comments
            .Find(new BsonDocument("discussionId", DiscussionId))
            .Sort(new BsonDocument("createdAt", -1))
            .Limit(3)
            .ToListAsync(cancellationToken);

        await _context.Discussions.UpdateOneAsync(
            new BsonDocument("_id", DiscussionId),
            Builders<BsonDocument>.Update.Set("lastComments", new BsonArray(lastComments)),
            cancellationToken: cancellationToken);

        await InsertRatingIfMissingAsync(new BsonDocument
        {
            ["productId"] = 1,
            ["count"] = 1,
            ["sum"] = 5,
            ["avg"] = 5.0,
            ["distribution"] = new BsonDocument("5", 1),
            ["updatedAt"] = new BsonDateTime(t0)
        }, cancellationToken);

        await InsertRatingIfMissingAsync(new BsonDocument
        {
            ["productId"] = 2,
            ["count"] = 1,
            ["sum"] = 4,
            ["avg"] = 4.0,
            ["distribution"] = new BsonDocument("4", 1),
            ["updatedAt"] = new BsonDateTime(t0)
        }, cancellationToken);
    }

    private async Task InsertReviewIfMissingAsync(ReviewDocument document, CancellationToken cancellationToken)
    {
        var exists = await _context.Reviews
            .Find(Builders<ReviewDocument>.Filter.Eq(x => x.Id, document.Id))
            .AnyAsync(cancellationToken);
        if (exists) return;

        try
        {
            await _context.Reviews.InsertOneAsync(document, cancellationToken: cancellationToken);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Code == 11000)
        {
            // Інший екземпляр застосунку міг вставити той самий seed паралельно.
        }
    }

    private static async Task InsertBsonIfMissingAsync(
        IMongoCollection<BsonDocument> collection,
        BsonDocument document,
        CancellationToken cancellationToken)
    {
        var exists = await collection.Find(new BsonDocument("_id", document["_id"])).AnyAsync(cancellationToken);
        if (exists) return;

        try
        {
            await collection.InsertOneAsync(document, cancellationToken: cancellationToken);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Code == 11000)
        {
            // Ідемпотентний seed: дублікат означає, що запис уже додано.
        }
    }

    private async Task InsertRatingIfMissingAsync(BsonDocument document, CancellationToken cancellationToken)
    {
        var exists = await _context.Ratings
            .Find(new BsonDocument("productId", document["productId"]))
            .AnyAsync(cancellationToken);
        if (exists) return;

        try
        {
            await _context.Ratings.InsertOneAsync(document, cancellationToken: cancellationToken);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Code == 11000)
        {
            // Унікальний індекс productId захищає від дубля за паралельного запуску.
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
