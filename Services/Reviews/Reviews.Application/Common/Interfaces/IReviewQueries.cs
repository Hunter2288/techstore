using Reviews.Application.Common.Models;
using Reviews.Domain.Entities;

namespace Reviews.Application.Common.Interfaces;

public interface IReviewQueries
{
    Task<PagedResult<Review>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        int? productId = null,
        CancellationToken cancellationToken = default);

    Task<Review?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
}
