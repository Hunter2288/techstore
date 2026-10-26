using MediatR;
using Reviews.Application.Common.Models;

namespace Reviews.Application.Reviews.Queries.GetReviewsList;

public sealed record GetReviewsListQuery(int PageNumber = 1, int PageSize = 10, int? ProductId = null)
    : IRequest<PagedResult<ReviewDto>>;
