using AspNet.MinimalApi.Movies.Data;
using AspNet.MinimalApi.Movies.Dtos;
using AspNet.MinimalApi.Movies.Middleware;
using AspNet.MinimalApi.Movies.Models;
using AspNet.MinimalApi.Movies.Repositories;
using AspNet.MinimalApi.Movies.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Налаштування Serilog з конфігурації
builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

// Додаємо сервіси
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=movies.db"));

builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IMovieService, MovieService>();

// Додаємо CORS для фронтенду
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Створюємо базу даних при запуску
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        context.Database.EnsureCreated();
        logger.LogInformation("База даних успішно ініціалізована");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Помилка при ініціалізації бази даних");
        throw;
    }
}

// Глобальний обробник помилок - кастомний middleware (закоментовано для демонстрації еволюції)
// app.UseMiddleware<GlobalExceptionMiddleware>();

// Використовуємо вбудований UseExceptionHandler замість кастомного middleware
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();

        if (exceptionFeature?.Error is not null)
        {
            var exception = exceptionFeature.Error;
            logger.LogError(exception, "Необроблена помилка: {Message}", exception.Message);

            context.Response.ContentType = "application/problem+json";

            var problemDetails = exception switch
            {
                AspNet.MinimalApi.Movies.Exceptions.MovieNotFoundException movieNotFound => new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    Title = "Фільм не знайдено",
                    Status = 404,
                    Detail = movieNotFound.Message,
                    Instance = context.Request.Path,
                    Extensions = { ["imdbId"] = movieNotFound.ImdbId }
                },

                AspNet.MinimalApi.Movies.Exceptions.MovieNotFoundByIdException movieNotFoundById => new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    Title = "Фільм не знайдено",
                    Status = 404,
                    Detail = movieNotFoundById.Message,
                    Instance = context.Request.Path,
                    Extensions = { ["id"] = movieNotFoundById.Id }
                },

                AspNet.MinimalApi.Movies.Exceptions.MovieAlreadyExistsException movieExists => new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                    Title = "Конфлікт даних",
                    Status = 409,
                    Detail = movieExists.Message,
                    Instance = context.Request.Path,
                    Extensions = { ["imdbId"] = movieExists.ImdbId }
                },

                AspNet.MinimalApi.Movies.Exceptions.ValidationException validation => new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "Помилка валідації",
                    Status = 400,
                    Detail = validation.Message,
                    Instance = context.Request.Path,
                    Extensions = { ["errors"] = validation.Errors }
                },

                ArgumentException argument => new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "Некоректні параметри",
                    Status = 400,
                    Detail = argument.Message,
                    Instance = context.Request.Path
                },

                _ => new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Title = "Внутрішня помилка сервера",
                    Status = 500,
                    Detail = "Сталася неочікувана помилка. Спробуйте пізніше.",
                    Instance = context.Request.Path
                }
            };

            context.Response.StatusCode = problemDetails.Status ?? 500;

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var json = System.Text.Json.JsonSerializer.Serialize(problemDetails, options);
            await context.Response.WriteAsync(json);
        }
    });
});

app.UseCors();

// Налаштування статичних файлів залежно від середовища
if (app.Environment.IsProduction())
{
    // У Production використовуємо зібрані файли з dist папки
    var distPath = Path.Combine(app.Environment.WebRootPath, "dist");
    var distFileProvider = new PhysicalFileProvider(distPath);

    var distOptions = new DefaultFilesOptions
    {
        FileProvider = distFileProvider,
        RequestPath = ""
    };
    distOptions.DefaultFileNames.Clear();
    distOptions.DefaultFileNames.Add("index.html");

    app.UseDefaultFiles(distOptions);

    var staticFileOptions = new StaticFileOptions
    {
        FileProvider = distFileProvider,
        RequestPath = ""
    };
    app.UseStaticFiles(staticFileOptions);
}
else
{
    // У Development використовуємо файли з wwwroot папки
    app.UseDefaultFiles(); // Дозволяє використовувати index.html як default файл
    app.UseStaticFiles();
}

// API endpoints

/// <summary>
/// Пошук фільмів (якщо s порожній, повертає всі фільми)
/// </summary>
app.MapGet("/api/movies/search", async (IMovieService movieService, ILogger<Program> logger, string? s, int page = 1) =>
    {
        logger.LogInformation("API запит: пошук фільмів з параметрами s={SearchTerm}, page={Page}", s, page);

        // Якщо параметр пошуку порожній, повертаємо всі фільми
        var searchTerm = string.IsNullOrWhiteSpace(s) ? "" : s;
        var result = await movieService.SearchAsync(searchTerm, page);

        logger.LogInformation("API відповідь: знайдено {Count} фільмів", result.Search.Count);
        return Results.Ok(result);
    })
    .WithName("SearchMovies");

/// <summary>
/// Отримання деталей фільму за IMDB ID
/// </summary>
app.MapGet("/api/movies/{imdbId}", async (string imdbId, IMovieService movieService, ILogger<Program> logger) =>
    {
        logger.LogInformation("API запит: отримання деталей фільму з IMDB ID {ImdbId}", imdbId);

        var movie = await movieService.GetDetailsAsync(imdbId);

        logger.LogInformation("API відповідь: деталі фільму {Title} повернуто", movie?.Title ?? "не знайдено");
        return Results.Ok(movie);
    })
    .WithName("GetMovieDetails");

/// <summary>
/// Створення нового фільму
/// </summary>
app.MapPost("/api/movies", async (Movie movie, IMovieService movieService, ILogger<Program> logger) =>
    {
        logger.LogInformation("API запит: створення фільму {Title} (IMDB: {ImdbId})",
            movie.Title, movie.ImdbId);

        var createdMovie = await movieService.CreateAsync(movie);

        logger.LogInformation("API відповідь: фільм {Title} створено з ID {Id}",
            createdMovie.Title, createdMovie.Id);

        return Results.Created($"/api/movies/{createdMovie.ImdbId}", createdMovie);
    })
    .WithName("CreateMovie");

/// <summary>
/// Оновлення фільму
/// </summary>
app.MapPut("/api/movies/{id:int}", async (int id, Movie movie, IMovieService movieService, ILogger<Program> logger) =>
    {
        logger.LogInformation("API запит: оновлення фільму з ID {Id}", id);

        var updatedMovie = await movieService.UpdateAsync(id, movie);

        logger.LogInformation("API відповідь: фільм {Title} оновлено", updatedMovie?.Title ?? "не знайдено");
        return Results.Ok(updatedMovie);
    })
    .WithName("UpdateMovie");

/// <summary>
/// Видалення фільму
/// </summary>
app.MapDelete("/api/movies/{id:int}", async (int id, IMovieService movieService, ILogger<Program> logger) =>
    {
        logger.LogInformation("API запит: видалення фільму з ID {Id}", id);

        await movieService.DeleteAsync(id);

        logger.LogInformation("API відповідь: фільм з ID {Id} видалено", id);
        return Results.NoContent();
    })
    .WithName("DeleteMovie");


try
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Запуск Movies API додатку");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Критична помилка при запуску додатку");
    throw;
}
finally
{
    Log.Information("Завершення роботи Movies API додатку");
    await Log.CloseAndFlushAsync();
}