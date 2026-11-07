using MediatR;
using Reviews.Application.Common.Interfaces;
using Reviews.Application.Common.Models;

namespace Reviews.Application.Reviews.Queries.GetReviewsList;

public sealed class GetReviewsListHandler : IRequestHandler<GetReviewsListQuery, PagedResult<ReviewDto>>
{
    private readonly IReviewQueries _queries;
    public GetReviewsListHandler(IReviewQueries queries)
    {
        _queries = queries;
    }

    public async Task<PagedResult<ReviewDto>> Handle(GetReviewsListQuery request, CancellationToken cancellationToken)
    {
        var result = await _queries.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.ProductId,
            cancellationToken);

        return result;
    }
}
