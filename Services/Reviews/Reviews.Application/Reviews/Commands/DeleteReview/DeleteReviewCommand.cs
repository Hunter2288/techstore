using MediatR;

namespace Reviews.Application.Reviews.Commands.DeleteReview;

public sealed record DeleteReviewCommand(
    string Id,
    int ExpectedVersion) : IRequest;