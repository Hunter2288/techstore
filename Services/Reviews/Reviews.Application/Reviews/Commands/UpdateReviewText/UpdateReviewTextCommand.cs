using MediatR;

namespace Reviews.Application.Reviews.Commands.UpdateReviewText;

public sealed record UpdateReviewTextCommand(string Id, int ExpectedVersion, string Text) : IRequest<int>;
