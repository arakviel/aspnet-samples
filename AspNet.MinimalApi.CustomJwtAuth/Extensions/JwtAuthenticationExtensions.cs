using AspNet.MinimalApi.CustomJwtAuth.Authentication;
using AspNet.MinimalApi.CustomJwtAuth.Data;
using AspNet.MinimalApi.CustomJwtAuth.Middleware;
using AspNet.MinimalApi.CustomJwtAuth.Services;
using Microsoft.EntityFrameworkCore;

namespace AspNet.MinimalApi.CustomJwtAuth.Extensions;

/// <summary>
///     Розширення для налаштування кастомної JWT аутентифікації.
///     Містить методи для конфігурації DI контейнера та middleware pipeline.
/// </summary>
public static class JwtAuthenticationExtensions
{
    /// <summary>
    ///     Додає кастомну JWT аутентифікацію до DI контейнера.
    /// </summary>
    /// <param name="services">Колекція сервісів</param>
    /// <param name="configuration">Конфігурація додатку</param>
    /// <returns>Колекція сервісів для подальшого налаштування</returns>
    public static IServiceCollection AddCustomJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Налаштування JWT опцій з конфігурації
        var jwtSettings = configuration.GetSection("JwtSettings");
        var jwtOptions = new JwtAuthenticationOptions
        {
            SecretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is required"),
            Issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer is required"),
            Audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience is required"),
            ExpirationMinutes = jwtSettings.GetValue("ExpirationMinutes", 60),
            RefreshTokenExpirationDays = jwtSettings.GetValue("RefreshTokenExpirationDays", 7),
            Algorithm = jwtSettings.GetValue<string>("Algorithm", "HS256"),
            IncludeIssuedAt = jwtSettings.GetValue("IncludeIssuedAt", true),
            IncludeJwtId = jwtSettings.GetValue("IncludeJwtId", true),
            ClockSkewSeconds = jwtSettings.GetValue("ClockSkewSeconds", 300)
        };

        // Валідація JWT опцій
        ValidateJwtOptions(jwtOptions);

        // Реєстрація JWT опцій як singleton
        services.AddSingleton(jwtOptions);

        // Реєстрація JWT сервісів як Singleton (для middleware)
        services.AddSingleton<JwtTokenGenerator>();
        services.AddScoped<IJwtService, JwtService>();

        // Реєстрація сервісів користувачів
        services.AddScoped<IUserService, UserService>();

        // Налаштування бази даних
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Database connection string is required");

        services.AddDbContext<AuthDbContext>(options =>
            options.UseSqlite(connectionString));

        return services;
    }

    /// <summary>
    ///     Додає кастомний JWT middleware до pipeline.
    /// </summary>
    /// <param name="app">Будівельник додатку</param>
    /// <returns>Будівельник додатку для подальшого налаштування</returns>
    public static IApplicationBuilder UseCustomJwtAuthentication(this IApplicationBuilder app)
    {
        return app.UseMiddleware<JwtAuthenticationMiddleware>();
    }

    /// <summary>
    ///     Ініціалізує базу даних та застосовує міграції.
    /// </summary>
    /// <param name="app">Веб-додаток</param>
    /// <returns>Task для асинхронної операції</returns>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        try
        {
            await context.EnsureDatabaseCreatedAsync();
            app.Logger.LogInformation("Database initialized successfully");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Error initializing database");
            throw;
        }
    }

    /// <summary>
    ///     Валідує JWT опції на коректність.
    /// </summary>
    /// <param name="options">JWT опції для валідації</param>
    /// <exception cref="ArgumentException">Викидається при некоректних опціях</exception>
    private static void ValidateJwtOptions(JwtAuthenticationOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.SecretKey))
            throw new ArgumentException("JWT SecretKey cannot be empty");

        if (options.SecretKey.Length < 32)
            throw new ArgumentException("JWT SecretKey must be at least 32 characters long");

        if (string.IsNullOrWhiteSpace(options.Issuer))
            throw new ArgumentException("JWT Issuer cannot be empty");

        if (string.IsNullOrWhiteSpace(options.Audience))
            throw new ArgumentException("JWT Audience cannot be empty");

        if (options.ExpirationMinutes <= 0)
            throw new ArgumentException("JWT ExpirationMinutes must be positive");

        if (options.RefreshTokenExpirationDays <= 0)
            throw new ArgumentException("JWT RefreshTokenExpirationDays must be positive");

        if (options.ClockSkewSeconds < 0)
            throw new ArgumentException("JWT ClockSkewSeconds cannot be negative");
    }

    /// <summary>
    ///     Перевіряє, чи аутентифікований користувач у HTTP контексті.
    /// </summary>
    /// <param name="context">HTTP контекст</param>
    /// <returns>true, якщо користувач аутентифікований</returns>
    public static bool IsAuthenticated(this HttpContext context)
    {
        return JwtAuthenticationMiddleware.IsAuthenticated(context);
    }

    /// <summary>
    ///     Отримує ID аутентифікованого користувача.
    /// </summary>
    /// <param name="context">HTTP контекст</param>
    /// <returns>ID користувача або null</returns>
    public static string? GetUserId(this HttpContext context)
    {
        return JwtAuthenticationMiddleware.GetUserId(context);
    }

    /// <summary>
    ///     Отримує ім'я аутентифікованого користувача.
    /// </summary>
    /// <param name="context">HTTP контекст</param>
    /// <returns>Ім'я користувача або null</returns>
    public static string? GetUsername(this HttpContext context)
    {
        return JwtAuthenticationMiddleware.GetUsername(context);
    }

    /// <summary>
    ///     Отримує роль аутентифікованого користувача.
    /// </summary>
    /// <param name="context">HTTP контекст</param>
    /// <returns>Роль користувача або null</returns>
    public static string? GetUserRole(this HttpContext context)
    {
        return JwtAuthenticationMiddleware.GetUserRole(context);
    }

    /// <summary>
    ///     Перевіряє, чи має користувач певну роль.
    /// </summary>
    /// <param name="context">HTTP контекст</param>
    /// <param name="role">Роль для перевірки</param>
    /// <returns>true, якщо користувач має роль</returns>
    public static bool HasRole(this HttpContext context, string role)
    {
        return JwtAuthenticationMiddleware.HasRole(context, role);
    }

    /// <summary>
    ///     Перевіряє, чи є користувач адміністратором.
    /// </summary>
    /// <param name="context">HTTP контекст</param>
    /// <returns>true, якщо користувач є адміністратором</returns>
    public static bool IsAdmin(this HttpContext context)
    {
        return JwtAuthenticationMiddleware.IsAdmin(context);
    }

    /// <summary>
    ///     Повертає відповідь про відсутність авторизації.
    /// </summary>
    /// <param name="message">Повідомлення про помилку</param>
    /// <returns>HTTP результат з кодом 401</returns>
    public static IResult Unauthorized(string message = "Необхідна аутентифікація")
    {
        return Results.Problem(
            title: "Unauthorized",
            detail: message,
            statusCode: 401
        );
    }

    /// <summary>
    ///     Повертає відповідь про відсутність доступу.
    /// </summary>
    /// <param name="message">Повідомлення про помилку</param>
    /// <returns>HTTP результат з кодом 403</returns>
    public static IResult Forbidden(string message = "Недостатньо прав доступу")
    {
        return Results.Problem(
            title: "Forbidden",
            detail: message,
            statusCode: 403
        );
    }
}
