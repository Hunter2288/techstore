namespace Reviews.Application.Common.Models;

public sealed class PagedResult<T>
{
    public int PageNumber { get; }
    public int PageSize { get; }
    public long TotalCount { get; }
    public IReadOnlyList<T> Items { get; }

    public PagedResult(int pageNumber, int pageSize, long totalCount, IReadOnlyList<T> items)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        Items = items;
    }
}
