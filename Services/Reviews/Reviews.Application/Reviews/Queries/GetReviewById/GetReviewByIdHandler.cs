using MediatR;
using Reviews.Application.Common.Interfaces;
using Reviews.Application.Reviews.Queries.GetReviewsList;

namespace Reviews.Application.Reviews.Queries.GetReviewById;

public sealed class GetReviewByIdHandler : IRequestHandler<GetReviewByIdQuery, ReviewDto?>
{
    private readonly IReviewQueries _queries;
    public GetReviewByIdHandler(IReviewQueries queries)
    {
        _queries = queries;
    }

    public async Task<ReviewDto?> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
    {
        return await _queries.GetByIdAsync(request.Id, cancellationToken);
    }
}
