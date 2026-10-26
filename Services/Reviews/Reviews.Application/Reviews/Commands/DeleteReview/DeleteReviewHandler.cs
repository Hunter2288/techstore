using System;
using System.Collections.Generic;
using System.Text;

using MediatR;
using Reviews.Application.Common.Exceptions;
using Reviews.Domain.Interfaces;

namespace Reviews.Application.Reviews.Commands.DeleteReview;

public sealed class DeleteReviewHandler
    : IRequestHandler<DeleteReviewCommand>
{
    private readonly IReviewRepository _repository;

    public DeleteReviewHandler(IReviewRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(
        DeleteReviewCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _repository.DeleteAsync(
            request.Id,
            request.ExpectedVersion,
            cancellationToken);

        switch (result)
        {
            case ReviewDeleteStatus.Deleted:
                return;

            case ReviewDeleteStatus.NotFound:
                throw new ReviewNotFoundException(
                    $"Відгук {request.Id} не знайдено.");

            case ReviewDeleteStatus.VersionConflict:
                throw new ConcurrencyConflictException(
                    "Відгук уже змінено. Перечитайте його й повторіть видалення.");

            default:
                throw new InvalidOperationException(
                    "Невідомий результат видалення відгуку.");
        }
    }
}