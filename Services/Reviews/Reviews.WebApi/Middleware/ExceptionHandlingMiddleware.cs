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
                .ToDictionary(group => group.Key, group => group.Select(x => x.ErrorMessage).Distinct().ToArray());
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Помилка валідації", "Перевірте передані дані.", errors);
        }
        catch (ReviewNotFoundException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, "Відгук не знайдено", exception.Message);
        }
        catch (ConcurrencyConflictException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Конфлікт версій", exception.Message);
        }
        catch (MongoWriteException exception) when (exception.WriteError?.Code == 11000)
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Дублікат", "Автор уже залишив відгук на цей товар.");
        }
        catch (DomainException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Помилка предметної області", exception.Message);
        }
        catch (MongoException exception)
        {
            _logger.LogError(exception, "Помилка MongoDB під час обробки {Path}", context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status503ServiceUnavailable, "MongoDB недоступна", "Не вдалося виконати операцію з базою даних.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Необроблена помилка під час обробки {Path}", context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "Внутрішня помилка", "Сталася неочікувана помилка.");
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        int status,
        string title,
        string detail,
        object? errors = null)
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
        if (errors is not null)
            problem.Extensions["errors"] = errors;

        await context.Response.WriteAsJsonAsync(problem, cancellationToken: context.RequestAborted);
    }
}
