using System.Net;
using System.Text.Json;
using AspNet.MinimalApi.Movies.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace AspNet.MinimalApi.Movies.Middleware;

/// <summary>
/// Кастомний middleware для глобальної обробки помилок.
///
/// ПРИМІТКА: Цей клас залишено для демонстрації еволюції підходів.
/// В поточній реалізації використовується UseExceptionHandler замість цього middleware.
///
/// Переваги кастомного middleware:
/// - Повний контроль над логікою обробки
/// - Можливість додавання специфічної логіки
/// - Навчальна цінність для розуміння принципів роботи
///
/// Недоліки:
/// - Більше коду для підтримки
/// - Потреба в окремому тестуванні
/// - Дублювання функціональності, яка вже є в ASP.NET Core
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Необроблена помилка: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var problemDetails = exception switch
        {
            MovieNotFoundException movieNotFound => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Фільм не знайдено",
                Status = (int)HttpStatusCode.NotFound,
                Detail = movieNotFound.Message,
                Instance = context.Request.Path,
                Extensions = { ["imdbId"] = movieNotFound.ImdbId }
            },
            
            MovieNotFoundByIdException movieNotFoundById => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Фільм не знайдено",
                Status = (int)HttpStatusCode.NotFound,
                Detail = movieNotFoundById.Message,
                Instance = context.Request.Path,
                Extensions = { ["id"] = movieNotFoundById.Id }
            },
            
            MovieAlreadyExistsException movieExists => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                Title = "Конфлікт даних",
                Status = (int)HttpStatusCode.Conflict,
                Detail = movieExists.Message,
                Instance = context.Request.Path,
                Extensions = { ["imdbId"] = movieExists.ImdbId }
            },
            
            ValidationException validation => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Помилка валідації",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = validation.Message,
                Instance = context.Request.Path,
                Extensions = { ["errors"] = validation.Errors }
            },
            
            ArgumentException argument => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Некоректні параметри",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = argument.Message,
                Instance = context.Request.Path
            },
            
            _ => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Внутрішня помилка сервера",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = "Сталася неочікувана помилка. Спробуйте пізніше.",
                Instance = context.Request.Path
            }
        };

        context.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(problemDetails, options);
        await context.Response.WriteAsync(json);
    }
}
