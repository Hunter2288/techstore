using System;
using System.Collections.Generic;
using System.Text;

using FluentValidation;

namespace Reviews.Application.Reviews.Commands.DeleteReview;

public sealed class DeleteReviewValidator
    : AbstractValidator<DeleteReviewCommand>
{
    public DeleteReviewValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .Length(24)
            .Matches("^[a-fA-F0-9]{24}$")
            .WithMessage(
                "Id має бути MongoDB ObjectId із 24 шістнадцяткових символів.");

        RuleFor(x => x.ExpectedVersion)
            .GreaterThan(0)
            .WithMessage("ExpectedVersion має бути більшим за 0.");
    }
}