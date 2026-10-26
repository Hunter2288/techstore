using MediatR;
using Reviews.Application.Common.Exceptions;
using Reviews.Domain.Interfaces;

namespace Reviews.Application.Reviews.Commands.UpdateReviewText;

public sealed class UpdateReviewTextHandler : IRequestHandler<UpdateReviewTextCommand, int>
{
    private readonly IReviewRepository _repository;

    public UpdateReviewTextHandler(IReviewRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(UpdateReviewTextCommand request, CancellationToken cancellationToken)
    {
        var result = await _repository.UpdateTextAsync(
            request.Id,
            request.ExpectedVersion,
            request.Text,
            cancellationToken);

        return result switch
        {
            ReviewUpdateStatus.Updated => request.ExpectedVersion + 1,
            ReviewUpdateStatus.NotFound => throw new ReviewNotFoundException($"Відгук {request.Id} не знайдено."),
            ReviewUpdateStatus.VersionConflict => throw new ConcurrencyConflictException(
                "Відгук уже змінено. Перечитайте його та повторіть оновлення з актуальною версією."),
            _ => throw new InvalidOperationException("Невідомий результат оновлення відгуку.")
        };
    }
}
