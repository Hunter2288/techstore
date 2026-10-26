using Reviews.Domain.Common;
using Reviews.Domain.Exceptions;
using Reviews.Domain.ValueObjects;

namespace Reviews.Domain.Entities;

public sealed class Review : BaseEntity
{
    private readonly List<string> _pros = [];
    private readonly List<string> _cons = [];
    private readonly List<ReviewMedia> _media = [];
    private readonly List<ReviewReply> _replies = [];

    public int ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int? OrderId { get; private set; }
    public bool IsVerifiedPurchase { get; private set; }
    public Author Author { get; private set; } = null!;
    public int Rating { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Text { get; private set; } = string.Empty;
    public IReadOnlyList<string> Pros => _pros.AsReadOnly();
    public IReadOnlyList<string> Cons => _cons.AsReadOnly();
    public IReadOnlyList<ReviewMedia> Media => _media.AsReadOnly();
    public ReviewDetails? Details { get; private set; }
    public IReadOnlyList<ReviewReply> Replies => _replies.AsReadOnly();
    public int HelpfulVotes { get; private set; }
    public int SchemaVersion { get; private set; }
    public int Version { get; private set; }

    private Review()
    {
    }

    private Review(string id, DateTime createdAt) : base(id, createdAt)
    {
    }

    public static Review Create(
        int productId,
        string productName,
        int? orderId,
        bool isVerifiedPurchase,
        Author author,
        string title,
        string text,
        int rating,
        IEnumerable<string>? pros = null,
        IEnumerable<string>? cons = null,
        IEnumerable<ReviewMedia>? media = null,
        ReviewDetails? details = null)
    {
        if (productId <= 0)
            throw new DomainException("ProductId має бути більшим за 0.");
        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("Назва товару є обов'язковою.");
        if (orderId is <= 0)
            throw new DomainException("OrderId має бути більшим за 0 або null.");
        if (isVerifiedPurchase && orderId is null)
            throw new DomainException("Для підтвердженої покупки потрібен OrderId.");
        if (author is null)
            throw new DomainException("Автор є обов'язковим.");
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 150)
            throw new DomainException("Заголовок має містити від 1 до 150 символів.");
        if (string.IsNullOrWhiteSpace(text))
            throw new DomainException("Текст відгуку є обов'язковим.");
        if (rating is < 1 or > 5)
            throw new DomainException("Оцінка має бути від 1 до 5.");

        var review = new Review
        {
            ProductId = productId,
            ProductName = productName.Trim(),
            OrderId = orderId,
            IsVerifiedPurchase = isVerifiedPurchase,
            Author = author,
            Rating = rating,
            Title = title.Trim(),
            Text = text.Trim(),
            Details = details,
            HelpfulVotes = 0,
            SchemaVersion = 2,
            Version = 1
        };

        if (pros is not null) review._pros.AddRange(pros.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()));
        if (cons is not null) review._cons.AddRange(cons.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()));
        if (media is not null) review._media.AddRange(media);

        return review;
    }

    public static Review Rehydrate(
        string id,
        int productId,
        string productName,
        int? orderId,
        bool isVerifiedPurchase,
        Author author,
        int rating,
        string title,
        string text,
        IEnumerable<string>? pros,
        IEnumerable<string>? cons,
        IEnumerable<ReviewMedia>? media,
        ReviewDetails? details,
        IEnumerable<ReviewReply>? replies,
        int helpfulVotes,
        int schemaVersion,
        int version,
        DateTime createdAt)
    {
        var review = new Review(id, createdAt)
        {
            ProductId = productId,
            ProductName = productName,
            OrderId = orderId,
            IsVerifiedPurchase = isVerifiedPurchase,
            Author = author,
            Rating = rating,
            Title = title,
            Text = text,
            Details = details,
            HelpfulVotes = Math.Max(0, helpfulVotes),
            SchemaVersion = schemaVersion,
            Version = Math.Max(1, version)
        };

        if (pros is not null) review._pros.AddRange(pros);
        if (cons is not null) review._cons.AddRange(cons);
        if (media is not null) review._media.AddRange(media);
        if (replies is not null) review._replies.AddRange(replies);
        return review;
    }
}
