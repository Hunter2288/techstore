using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;
using Reviews.Application.Common.Interfaces;
using Reviews.Application.Common.Models;
using Reviews.Application.Reviews.Queries.GetReviewsList;

namespace Reviews.Infrastructure.Persistence;

public sealed class ReviewQueries : IReviewQueries
{
    private static readonly Expression<Func<ReviewDocument, ReviewDto>> Projection = x => new ReviewDto
    {
        Id = x.Id,
        ProductId = x.ProductId,
        ProductName = x.ProductName,
        OrderId = x.OrderId,
        IsVerifiedPurchase = x.IsVerifiedPurchase,
        Author = new ReviewAuthorDto { UserId = x.Author.UserId, DisplayName = x.Author.DisplayName },
        Rating = x.Rating,
        Title = x.Title,
        Text = x.Text,
        Pros = x.Pros,
        Cons = x.Cons,
        Media = x.Media.Select(m => new ReviewMediaDto { Type = m.Type, Url = m.Url }).ToList(),
        Details = x.Details == null ? null : new ReviewDetailsDto
        {
            BatteryLifeHours = x.Details.BatteryLifeHours,
            CameraScore = x.Details.CameraScore,
            KeyboardRating = x.Details.KeyboardRating,
            Thermals = x.Details.Thermals,
            UsedFor = x.Details.UsedFor
        },
        Replies = x.Replies.Select(r => new ReviewReplyDto
        {
            Author = new ReviewAuthorDto { UserId = r.Author.UserId, DisplayName = r.Author.DisplayName },
            Text = r.Text,
            CreatedAt = r.CreatedAt
        }).ToList(),
        HelpfulVotes = x.HelpfulVotes,
        SchemaVersion = x.SchemaVersion,
        Version = x.Version,
        CreatedAt = x.CreatedAt
    };

    private readonly MongoDbContext _context;

    public ReviewQueries(MongoDbContext context) => _context = context;

    public async Task<PagedResult<ReviewDto>> GetPagedAsync(
        int pageNumber, int pageSize, int? productId = null,
        CancellationToken cancellationToken = default)
    {
        var filter = productId.HasValue
            ? Builders<ReviewDocument>.Filter.Eq(x => x.ProductId, productId.Value)
            : Builders<ReviewDocument>.Filter.Empty;
        var totalCount = await _context.Reviews.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var skip = (int)Math.Min((long)(pageNumber - 1) * pageSize, int.MaxValue);
        var items = await _context.Reviews.Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Limit(pageSize)
            .Project(Projection)
            .ToListAsync(cancellationToken);

        return new PagedResult<ReviewDto>(pageNumber, pageSize, totalCount, items);
    }

    public async Task<ReviewDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out _)) return null;
        return await _context.Reviews
            .Find(Builders<ReviewDocument>.Filter.Eq(x => x.Id, id))
            .Project(Projection)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
