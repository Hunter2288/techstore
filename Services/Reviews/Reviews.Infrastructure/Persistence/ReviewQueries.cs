using MongoDB.Bson;
using MongoDB.Driver;
using Reviews.Application.Common.Interfaces;
using Reviews.Application.Common.Models;
using Reviews.Domain.Entities;

namespace Reviews.Infrastructure.Persistence;

public sealed class ReviewQueries : IReviewQueries
{
    private readonly MongoDbContext _context;

    public ReviewQueries(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Review>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        int? productId = null,
        CancellationToken cancellationToken = default)
    {
        var filter = productId.HasValue
            ? Builders<ReviewDocument>.Filter.Eq(x => x.ProductId, productId.Value)
            : Builders<ReviewDocument>.Filter.Empty;

        var totalCount = await _context.Reviews.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var skip = (int)Math.Min((long)(pageNumber - 1) * pageSize, int.MaxValue);

        var documents = await _context.Reviews.Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Review>(
            pageNumber,
            pageSize,
            totalCount,
            documents.Select(x => x.ToDomain()).ToList());
    }

    public async Task<Review?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        var document = await _context.Reviews
            .Find(Builders<ReviewDocument>.Filter.Eq(x => x.Id, id))
            .FirstOrDefaultAsync(cancellationToken);

        return document?.ToDomain();
    }
}
