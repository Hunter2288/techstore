using Reviews.Domain.Entities;

namespace Reviews.Domain.Interfaces;

public enum ReviewUpdateStatus
{
    Updated,
    NotFound,
    VersionConflict
}

public enum ReviewDeleteStatus
{
    Deleted,
    NotFound,
    VersionConflict
}

public interface IReviewRepository
{
    Task AddAsync(
        Review review,
        CancellationToken cancellationToken = default);

    Task<ReviewUpdateStatus> UpdateTextAsync(
        string id,
        int expectedVersion,
        string text,
        CancellationToken cancellationToken = default);

    Task<ReviewDeleteStatus> DeleteAsync(
        string id,
        int expectedVersion,
        CancellationToken cancellationToken = default);
}