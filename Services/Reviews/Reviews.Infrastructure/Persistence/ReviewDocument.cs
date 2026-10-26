using Reviews.Domain.Entities;
using Reviews.Domain.ValueObjects;

namespace Reviews.Infrastructure.Persistence;

public sealed class ReviewDocument
{
    public string Id { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public AuthorDocument Author { get; set; } = new();
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public List<string> Pros { get; set; } = [];
    public List<string> Cons { get; set; } = [];
    public List<ReviewMediaDocument> Media { get; set; } = [];
    public ReviewDetailsDocument? Details { get; set; }
    public List<ReviewReplyDocument> Replies { get; set; } = [];
    public int HelpfulVotes { get; set; }
    public int SchemaVersion { get; set; } = 2;
    public int Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static ReviewDocument FromDomain(Review review) => new()
    {
        Id = review.Id,
        ProductId = review.ProductId,
        ProductName = review.ProductName,
        OrderId = review.OrderId,
        IsVerifiedPurchase = review.IsVerifiedPurchase,
        Author = new AuthorDocument { UserId = review.Author.UserId, DisplayName = review.Author.DisplayName },
        Rating = review.Rating,
        Title = review.Title,
        Text = review.Text,
        Pros = review.Pros.ToList(),
        Cons = review.Cons.ToList(),
        Media = review.Media.Select(x => new ReviewMediaDocument { Type = x.Type, Url = x.Url }).ToList(),
        Details = review.Details is null ? null : new ReviewDetailsDocument
        {
            BatteryLifeHours = review.Details.BatteryLifeHours,
            CameraScore = review.Details.CameraScore,
            KeyboardRating = review.Details.KeyboardRating,
            Thermals = review.Details.Thermals,
            UsedFor = review.Details.UsedFor?.ToList()
        },
        Replies = review.Replies.Select(x => new ReviewReplyDocument
        {
            Author = new AuthorDocument { UserId = x.Author.UserId, DisplayName = x.Author.DisplayName },
            Text = x.Text,
            CreatedAt = x.CreatedAt
        }).ToList(),
        HelpfulVotes = review.HelpfulVotes,
        SchemaVersion = review.SchemaVersion,
        Version = review.Version,
        CreatedAt = review.CreatedAt
    };

    public Review ToDomain() => Review.Rehydrate(
        Id,
        ProductId,
        ProductName,
        OrderId,
        IsVerifiedPurchase,
        new Author(Author.UserId, Author.DisplayName),
        Rating,
        Title,
        Text,
        Pros,
        Cons,
        Media.Select(x => new ReviewMedia(x.Type, x.Url)),
        Details is null ? null : new ReviewDetails(
            Details.BatteryLifeHours,
            Details.CameraScore,
            Details.KeyboardRating,
            Details.Thermals,
            Details.UsedFor?.ToArray()),
        Replies.Select(x => new ReviewReply(
            new Author(x.Author.UserId, x.Author.DisplayName), x.Text, x.CreatedAt)),
        HelpfulVotes,
        SchemaVersion,
        Version,
        CreatedAt);
}

public sealed class AuthorDocument
{
    public long UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}

public sealed class ReviewMediaDocument
{
    public string Type { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public sealed class ReviewDetailsDocument
{
    public int? BatteryLifeHours { get; set; }
    public int? CameraScore { get; set; }
    public int? KeyboardRating { get; set; }
    public string? Thermals { get; set; }
    public List<string>? UsedFor { get; set; }
}

public sealed class ReviewReplyDocument
{
    public AuthorDocument Author { get; set; } = new();
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
