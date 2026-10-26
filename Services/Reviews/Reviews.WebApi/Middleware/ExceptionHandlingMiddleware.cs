using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Reviews.Application.Common.Exceptions;
using Reviews.Domain.Exceptions;

namespace Reviews.WebApi.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException exception)
        {
            var errors = exception.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).Distinct().ToArray());
            _logger.LogWarning("Валідація не пройшла для {Path}: {@Errors}", context.Request.Path, errors);
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest,
                "Помилка валідації", "Перевірте передані дані.", errors);
        }
        catch (ReviewNotFoundException exception)
        {
            _logger.LogWarning("Не знайдено на {Path}: {Message}", context.Request.Path, exception.Message);
            await WriteProblemAsync(context, StatusCodes.Status404NotFound,
                "Відгук не знайдено", exception.Message);
        }
        catch (ConcurrencyConflictException exception)
        {
            _logger.LogWarning("Конфлікт версій на {Path}: {Message}", context.Request.Path, exception.Message);
            await WriteProblemAsync(context, StatusCodes.Status409Conflict,
                "Конфлікт версій", exception.Message);
        }
        catch (MongoWriteException exception) when (exception.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning("Дублікат ключа на {Path}", context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status409Conflict,
                "Дублікат", "Автор уже залишив відгук на цей товар.");
        }
        catch (MongoWriteException exception)
        {
            _logger.LogWarning(exception, "Помилка запису в MongoDB на {Path}", context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest,
                "Помилка запису", "Документ не пройшов перевірку бази даних.");
        }
        catch (DomainException exception)
        {
            _logger.LogWarning("Порушено правило домену на {Path}: {Message}", context.Request.Path, exception.Message);
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest,
                "Помилка предметної області", exception.Message);
        }
        catch (Exception exception) when (exception is MongoException or TimeoutException)
        {
            _logger.LogError(exception, "MongoDB недоступна під час обробки {Path}", context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status503ServiceUnavailable,
                "MongoDB недоступна", "Не вдалося виконати операцію з базою даних.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Необроблена помилка під час обробки {Path}", context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError,
                "Внутрішня помилка", "Сталася неочікувана помилка.");
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context, int status, string title, string detail, object? errors = null)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;
        if (errors is not null)
            problem.Extensions["errors"] = errors;

        await context.Response.WriteAsJsonAsync(problem, cancellationToken: context.RequestAborted);
    }
}