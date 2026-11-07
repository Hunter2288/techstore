using MediatR;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Reviews.Domain.Entities;
using Reviews.Domain.Interfaces;
using Reviews.Domain.ValueObjects;

namespace Reviews.Application.Reviews.Commands.CreateReview;

public sealed class CreateReviewHandler : IRequestHandler<CreateReviewCommand, string>
{
    private static readonly ActivitySource ActivitySource = new("Reviews.WebApi");
    private readonly IReviewRepository _repository;
    private readonly ILogger<CreateReviewHandler> _logger;

    public CreateReviewHandler(IReviewRepository repository, ILogger<CreateReviewHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<string> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("reviews.create");
        activity?.SetTag("product.id", request.ProductId);
        activity?.SetTag("review.author_id", request.Author.UserId);
        activity?.SetTag("review.rating", request.Rating);
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
        activity?.SetTag("review.id", review.Id);
        _logger.LogInformation("Review {ReviewId} created for product {ProductId} by author {AuthorId} with rating {Rating}",
            review.Id, review.ProductId, request.Author.UserId, review.Rating);
        return review.Id;
    }
}
