using FluentValidation;

namespace Reviews.Application.Reviews.Commands.CreateReview;

public sealed class CreateReviewValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OrderId).Must(x => x is null or > 0)
            .WithMessage("OrderId має бути більшим за 0 або null.");
        RuleFor(x => x).Must(x => !x.IsVerifiedPurchase || x.OrderId.HasValue)
            .WithMessage("Для підтвердженої покупки потрібен OrderId.");

        RuleFor(x => x.Author).NotNull();
        When(x => x.Author is not null, () =>
        {
            RuleFor(x => x.Author!.UserId).GreaterThan(0);
            RuleFor(x => x.Author!.DisplayName).NotEmpty().MaximumLength(100);
        });

        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Text).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5)
            .WithMessage("Оцінка має бути від 1 до 5.");

        RuleForEach(x => x.Pros).NotEmpty().MaximumLength(200);
        RuleForEach(x => x.Cons).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Pros).Must(x => x is null || x.Count <= 20)
            .WithMessage("Максимум 20 переваг.");
        RuleFor(x => x.Cons).Must(x => x is null || x.Count <= 20)
            .WithMessage("Максимум 20 недоліків.");
        RuleFor(x => x.Media).Must(x => x is null || x.Count <= 10)
            .WithMessage("Максимум 10 медіафайлів.");
        RuleForEach(x => x.Media).ChildRules(media =>
        {
            media.RuleFor(x => x.Type).Must(x => x is "photo" or "video")
                .WithMessage("Тип медіа має бути photo або video.");
            media.RuleFor(x => x.Url).NotEmpty().MaximumLength(2048);
        });

        When(x => x.Details is not null, () =>
        {
            RuleFor(x => x.Details!.BatteryLifeHours).Must(x => x is null or > 0);
            RuleFor(x => x.Details!.CameraScore).Must(x => x is null or >= 0 and <= 10);
            RuleFor(x => x.Details!.KeyboardRating).Must(x => x is null or >= 1 and <= 5);
            RuleFor(x => x.Details!.Thermals).MaximumLength(100);
            RuleFor(x => x.Details!.UsedFor).Must(x => x is null || x.Count <= 20);
        });
    }
}
