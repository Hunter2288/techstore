using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Reviews.Domain.Entities;
using Reviews.Domain.Interfaces;
using global::Reviews.Domain.Interfaces;

namespace Reviews.Infrastructure.Persistence;

public sealed class ReviewRepository : IReviewRepository
{
    private readonly MongoDbContext _context;

    public ReviewRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Review review, CancellationToken cancellationToken = default)
    {
        await _context.Reviews.InsertOneAsync(
            ReviewDocument.FromDomain(review),
            cancellationToken: cancellationToken);

        await RefreshRatingSummaryAsync(review.ProductId, cancellationToken);
    }
    public async Task<ReviewDeleteStatus> DeleteAsync(
    string id,
    int expectedVersion,
    CancellationToken cancellationToken = default)
    {
        if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
            return ReviewDeleteStatus.NotFound;

        var idFilter =
            Builders<ReviewDocument>.Filter.Eq(x => x.Id, id);

        var existing = await _context.Reviews
            .Find(idFilter)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is null)
            return ReviewDeleteStatus.NotFound;

        var deleteFilter = idFilter &
            Builders<ReviewDocument>.Filter.Eq(
                x => x.Version,
                expectedVersion);

        var result = await _context.Reviews.DeleteOneAsync(
            deleteFilter,
            cancellationToken);

        if (result.DeletedCount == 0)
        {
            var stillExists = await _context.Reviews
                .Find(idFilter)
                .AnyAsync(cancellationToken);

            return stillExists
                ? ReviewDeleteStatus.VersionConflict
                : ReviewDeleteStatus.NotFound;
        }

        await RefreshRatingSummaryAsync(
            existing.ProductId,
            cancellationToken);

        return ReviewDeleteStatus.Deleted;
    }

    public async Task<ReviewUpdateStatus> UpdateTextAsync(
        string id,
        int expectedVersion,
        string text,
        CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out _))
            return ReviewUpdateStatus.NotFound;

        var filter = Builders<ReviewDocument>.Filter.Eq(x => x.Id, id) &
                     Builders<ReviewDocument>.Filter.Eq(x => x.Version, expectedVersion);
        var update = Builders<ReviewDocument>.Update
            .Set(x => x.Text, text.Trim())
            .Inc(x => x.Version, 1);

        var result = await _context.Reviews.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        if (result.MatchedCount == 1)
            return ReviewUpdateStatus.Updated;

        var exists = await _context.Reviews
            .Find(Builders<ReviewDocument>.Filter.Eq(x => x.Id, id))
            .AnyAsync(cancellationToken);

        return exists ? ReviewUpdateStatus.VersionConflict : ReviewUpdateStatus.NotFound;
    }

    private async Task RefreshRatingSummaryAsync(int productId, CancellationToken cancellationToken)
    {
        var stats = await _context.Reviews.Aggregate()
            .Match(x => x.ProductId == productId)
            .Group(x => x.ProductId, group => new RatingAggregate
            {
                ProductId = group.Key,
                Count = group.Count(),
                Sum = group.Sum(x => x.Rating),
                Average = group.Average(x => (double)x.Rating)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (stats is null)
        {
            await _context.Ratings.DeleteOneAsync(
                new MongoDB.Bson.BsonDocument(
                    "productId", productId),
                cancellationToken: cancellationToken);

            return;
        }

        var distribution = new BsonDocument();
        for (var rating = 1; rating <= 5; rating++)
        {
            var filter = Builders<ReviewDocument>.Filter.Eq(x => x.ProductId, productId) &
                         Builders<ReviewDocument>.Filter.Eq(x => x.Rating, rating);
            var count = await _context.Reviews.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            if (count > 0)
                distribution[rating.ToString()] = new BsonInt32(checked((int)count));
        }

        var summary = new BsonDocument
        {
            ["productId"] = new BsonInt32(stats.ProductId),
            ["count"] = new BsonInt32(stats.Count),
            ["sum"] = new BsonInt32(stats.Sum),
            ["avg"] = new BsonDouble(stats.Average),
            ["distribution"] = distribution,
            ["updatedAt"] = new BsonDateTime(DateTime.UtcNow)
        };

        await _context.Ratings.ReplaceOneAsync(
            new BsonDocument("productId", productId),
            summary,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);
    }

    private sealed class RatingAggregate
    {
        public int ProductId { get; set; }
        public int Count { get; set; }
        public int Sum { get; set; }
        public double Average { get; set; }
    }
}
