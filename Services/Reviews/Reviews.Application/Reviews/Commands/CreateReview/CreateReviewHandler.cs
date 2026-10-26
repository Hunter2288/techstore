using MediatR;
using Reviews.Domain.Entities;
using Reviews.Domain.Interfaces;
using Reviews.Domain.ValueObjects;

namespace Reviews.Application.Reviews.Commands.CreateReview;

public sealed class CreateReviewHandler : IRequestHandler<CreateReviewCommand, string>
{
    private readonly IReviewRepository _repository;

    public CreateReviewHandler(IReviewRepository repository)
    {
        _repository = repository;
    }

    public async Task<string> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var author = new Author(request.Author.UserId, request.Author.DisplayName);
        ReviewDetails? details = request.Details is null
            ? null
            : new ReviewDetails(
                request.Details.BatteryLifeHours,
                request.Details.CameraScore,
                request.Details.KeyboardRating,
                request.Details.Thermals,
                request.Details.UsedFor?.ToArray());

        var media = request.Media?.Select(x => new ReviewMedia(x.Type, x.Url));

        var review = Review.Create(
            request.ProductId,
            request.ProductName,
            request.OrderId,
            request.IsVerifiedPurchase,
            author,
            request.Title,
            request.Text,
            request.Rating,
            request.Pros,
            request.Cons,
            media,
            details);

        await _repository.AddAsync(review, cancellationToken);
        return review.Id;
    }
}
