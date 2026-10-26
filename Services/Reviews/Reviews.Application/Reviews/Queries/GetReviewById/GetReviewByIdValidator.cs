using System;
using System.Collections.Generic;
using System.Text;

using FluentValidation;

namespace Reviews.Application.Reviews.Queries.GetReviewById;

public sealed class GetReviewByIdValidator
    : AbstractValidator<GetReviewByIdQuery>
{
    public GetReviewByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .Length(24)
            .Matches("^[a-fA-F0-9]{24}$")
            .WithMessage(
                "Id має бути MongoDB ObjectId із 24 шістнадцяткових символів.");
    }
}