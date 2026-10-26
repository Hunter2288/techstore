using AutoMapper;
using Reviews.Domain.Entities;
using Reviews.Domain.ValueObjects;

namespace Reviews.Application.Reviews.Queries.GetReviewsList;

public sealed class ReviewProfile : Profile
{
    public ReviewProfile()
    {
        CreateMap<Author, ReviewAuthorDto>();
        CreateMap<ReviewMedia, ReviewMediaDto>();
        CreateMap<ReviewDetails, ReviewDetailsDto>();
        CreateMap<ReviewReply, ReviewReplyDto>();
        CreateMap<Review, ReviewDto>();
    }
}
