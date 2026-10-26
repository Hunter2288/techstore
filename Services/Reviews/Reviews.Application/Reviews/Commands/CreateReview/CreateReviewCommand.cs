using MediatR;

namespace Reviews.Application.Reviews.Commands.CreateReview;

public sealed record ReviewAuthorInput(long UserId, string DisplayName);
public sealed record ReviewMediaInput(string Type, string Url);
public sealed record ReviewDetailsInput(
    int? BatteryLifeHours = null,
    int? CameraScore = null,
    int? KeyboardRating = null,
    string? Thermals = null,
    IReadOnlyList<string>? UsedFor = null);

public sealed record CreateReviewCommand(
    int ProductId,
    string ProductName,
    int? OrderId,
    bool IsVerifiedPurchase,
    ReviewAuthorInput Author,
    string Title,
    string Text,
    int Rating,
    IReadOnlyList<string>? Pros = null,
    IReadOnlyList<string>? Cons = null,
    IReadOnlyList<ReviewMediaInput>? Media = null,
    ReviewDetailsInput? Details = null) : IRequest<string>;
