using FluentValidation;

namespace Reviews.Application.Reviews.Queries.GetReviewsList;

public sealed class GetReviewsListValidator : AbstractValidator<GetReviewsListQuery>
{
    public GetReviewsListValidator()
    {
        RuleFor(x => x.PageNumber).InclusiveBetween(1, 1_000_000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.ProductId).Must(x => x is null or > 0)
            .WithMessage("ProductId має бути більшим за 0 або не заданим.");
    }
}
