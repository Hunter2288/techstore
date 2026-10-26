using AutoMapper;
using MediatR;
using Reviews.Application.Common.Interfaces;
using Reviews.Application.Reviews.Queries.GetReviewsList;

namespace Reviews.Application.Reviews.Queries.GetReviewById;

public sealed class GetReviewByIdHandler : IRequestHandler<GetReviewByIdQuery, ReviewDto?>
{
    private readonly IReviewQueries _queries;
    private readonly IMapper _mapper;

    public GetReviewByIdHandler(IReviewQueries queries, IMapper mapper)
    {
        _queries = queries;
        _mapper = mapper;
    }

    public async Task<ReviewDto?> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
    {
        var review = await _queries.GetByIdAsync(request.Id, cancellationToken);
        return review is null ? null : _mapper.Map<ReviewDto>(review);
    }
}
