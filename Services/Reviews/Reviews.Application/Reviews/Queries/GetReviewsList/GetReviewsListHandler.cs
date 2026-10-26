using AutoMapper;
using MediatR;
using Reviews.Application.Common.Interfaces;
using Reviews.Application.Common.Models;

namespace Reviews.Application.Reviews.Queries.GetReviewsList;

public sealed class GetReviewsListHandler : IRequestHandler<GetReviewsListQuery, PagedResult<ReviewDto>>
{
    private readonly IReviewQueries _queries;
    private readonly IMapper _mapper;

    public GetReviewsListHandler(IReviewQueries queries, IMapper mapper)
    {
        _queries = queries;
        _mapper = mapper;
    }

    public async Task<PagedResult<ReviewDto>> Handle(GetReviewsListQuery request, CancellationToken cancellationToken)
    {
        var result = await _queries.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.ProductId,
            cancellationToken);

        var items = _mapper.Map<IReadOnlyList<ReviewDto>>(result.Items);
        return new PagedResult<ReviewDto>(result.PageNumber, result.PageSize, result.TotalCount, items);
    }
}
