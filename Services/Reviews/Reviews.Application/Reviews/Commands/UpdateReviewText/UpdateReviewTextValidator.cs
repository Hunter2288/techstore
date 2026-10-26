using FluentValidation;

namespace Reviews.Application.Reviews.Commands.UpdateReviewText;

public sealed class UpdateReviewTextValidator : AbstractValidator<UpdateReviewTextCommand>
{
    public UpdateReviewTextValidator()
    {
        RuleFor(x => x.Id).NotEmpty().Length(24).Matches("^[a-fA-F0-9]{24}$");
        RuleFor(x => x.ExpectedVersion).GreaterThan(0);
        RuleFor(x => x.Text).NotEmpty().MaximumLength(5000);
    }
}
