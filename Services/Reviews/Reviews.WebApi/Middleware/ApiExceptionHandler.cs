using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Core.Servers;
using Reviews.Application.Common.Exceptions;
using Reviews.Domain.Exceptions;

namespace Reviews.WebApi.Middleware;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            ValidationException => (400, "Validation failed", "One or more validation errors occurred."),
            ReviewNotFoundException => (404, "Review not found", exception.Message),
            ConcurrencyConflictException => (409, "Version conflict", exception.Message),
            DomainException => (400, "Domain rule violation", exception.Message),
            MongoWriteException { WriteError.Category: ServerErrorCategory.DuplicateKey } => (409, "Duplicate review", "A review already exists for this product and author."),
            MongoWriteException => (400, "Database write rejected", "The document was rejected by the database."),
            MongoConnectionException or TimeoutException => (503, "Database unavailable", "The database could not be reached."),
            _ => (500, "Unexpected error", "An unexpected error occurred.")
        };

        if (status >= 500) _logger.LogError(exception, "Request failed with {StatusCode}", status);
        else _logger.LogWarning(exception, "Request failed with {StatusCode}", status);

        context.Response.StatusCode = status;
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;
        if (exception is ValidationException validationException)
        {
            problem.Extensions["errors"] = validationException.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).Distinct().ToArray());
        }

        await context.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
