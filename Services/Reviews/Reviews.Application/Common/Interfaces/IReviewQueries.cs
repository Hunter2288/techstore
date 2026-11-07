using Reviews.Application.Common.Models;
using Reviews.Application.Reviews.Queries.GetReviewsList;

namespace Reviews.Application.Common.Interfaces;

public interface IReviewQueries
{
    Task<PagedResult<ReviewDto>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        int? productId = null,
        CancellationToken cancellationToken = default);

    Task<ReviewDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
}
