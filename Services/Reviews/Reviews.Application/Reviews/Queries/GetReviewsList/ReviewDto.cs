namespace Reviews.Application.Reviews.Queries.GetReviewsList;

public sealed class ReviewDto
{
    public string Id { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public ReviewAuthorDto Author { get; set; } = new();
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public List<string> Pros { get; set; } = [];
    public List<string> Cons { get; set; } = [];
    public List<ReviewMediaDto> Media { get; set; } = [];
    public ReviewDetailsDto? Details { get; set; }
    public List<ReviewReplyDto> Replies { get; set; } = [];
    public int HelpfulVotes { get; set; }
    public int SchemaVersion { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class ReviewAuthorDto
{
    public long UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}

public sealed class ReviewMediaDto
{
    public string Type { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public sealed class ReviewDetailsDto
{
    public int? BatteryLifeHours { get; set; }
    public int? CameraScore { get; set; }
    public int? KeyboardRating { get; set; }
    public string? Thermals { get; set; }
    public List<string>? UsedFor { get; set; }
}

public sealed class ReviewReplyDto
{
    public ReviewAuthorDto Author { get; set; } = new();
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
