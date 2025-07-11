using AspNet.MinimalApi.CustomAuth.Authentication;
using AspNet.MinimalApi.CustomAuth.Middleware;
using System.Security.Claims;
using System.Text;

namespace AspNet.MinimalApi.CustomAuth.Extensions;

/// <summary>
///     Розширення для налаштування кастомної системи аутентифікації.
///     Надає зручні методи для конфігурації та використання аутентифікації в додатку.
/// </summary>
public static class CustomAuthenticationExtensions
{
    /// <summary>
    ///     Додає кастомну аутентифікацію до колекції сервісів.
    ///     Реєструє необхідні сервіси та опції для роботи системи аутентифікації.
    /// </summary>
    /// <param name="services">Колекція сервісів додатку</param>
    /// <param name="configureOptions">Делегат для налаштування опцій аутентифікації</param>
    /// <returns>Колекція сервісів для подальшого налаштування</returns>
    public static IServiceCollection AddCustomAuthentication(
        this IServiceCollection services,
        Action<CustomAuthenticationOptions>? configureOptions = null)
    {
        // Створюємо та налаштовуємо опції
        var options = new CustomAuthenticationOptions();
        configureOptions?.Invoke(options);

        // Реєструємо опції як singleton
        services.AddSingleton(options);

        return services;
    }

    /// <summary>
    ///     Додає middleware кастомної аутентифікації до конвеєра обробки запитів.
    ///     Має викликатися після UseRouting() та перед UseAuthorization().
    /// </summary>
    /// <param name="app">Конструктор додатку</param>
    /// <returns>Конструктор додатку для подальшого налаштування</returns>
    public static IApplicationBuilder UseCustomAuthentication(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CustomAuthenticationMiddleware>();
    }

    /// <summary>
    ///     Асинхронно виконує вхід користувача в систему.
    ///     Створює тікет аутентифікації та встановлює кукі в браузері.
    ///     Аналог HttpContext.SignInAsync() в ASP.NET Core.
    /// </summary>
    /// <param name="context">Контекст HTTP запиту</param>
    /// <param name="principal">Принципал користувача з claims</param>
    /// <param name="authenticationScheme">Схема аутентифікації (опціонально)</param>
    public static async Task CustomSignInAsync(
        this HttpContext context,
        ClaimsPrincipal principal,
        string authenticationScheme = CustomCookieAuthenticationDefaults.AuthenticationScheme)
    {
        // Отримуємо опції аутентифікації з DI контейнера
        var options = context.RequestServices.GetRequiredService<CustomAuthenticationOptions>();

        // Створюємо тікет аутентифікації
        var ticket = new CustomAuthenticationTicket(principal, authenticationScheme)
        {
            ExpiresUtc = DateTimeOffset.UtcNow.Add(options.ExpireTimeSpan)
        };

        // Серіалізуємо тікет
        var serializedTicket = ticket.Serialize();
        var encodedTicket = Convert.ToBase64String(Encoding.UTF8.GetBytes(serializedTicket));

        // Налаштовуємо опції куки
        var cookieOptions = new CookieOptions
        {
            HttpOnly = options.Cookie.HttpOnly,
            Secure = options.Cookie.SecurePolicy == CookieSecurePolicy.Always ||
                     options.Cookie.SecurePolicy == CookieSecurePolicy.SameAsRequest && context.Request.IsHttps,
            SameSite = options.Cookie.SameSite,
            Expires = ticket.ExpiresUtc?.DateTime,
            Path = options.Cookie.Path ?? "/"
        };

        // Встановлюємо кукі
        context.Response.Cookies.Append(options.Cookie.Name!, encodedTicket, cookieOptions);

        // Встановлюємо користувача в поточному контексті
        context.User = principal;

        await Task.CompletedTask;
    }

    /// <summary>
    ///     Асинхронно виконує вихід користувача з системи.
    ///     Видаляє кукі аутентифікації та очищає інформацію про користувача.
    ///     Аналог HttpContext.SignOutAsync() в ASP.NET Core.
    /// </summary>
    /// <param name="context">Контекст HTTP запиту</param>
    /// <param name="authenticationScheme">Схема аутентифікації (опціонально)</param>
    public static async Task CustomSignOutAsync(
        this HttpContext context,
        string authenticationScheme = CustomCookieAuthenticationDefaults.AuthenticationScheme)
    {
        // Отримуємо опції аутентифікації з DI контейнера
        var options = context.RequestServices.GetRequiredService<CustomAuthenticationOptions>();

        // Видаляємо кукі аутентифікації
        context.Response.Cookies.Delete(options.Cookie.Name!, new CookieOptions
        {
            Path = options.Cookie.Path ?? "/"
        });

        // Очищуємо інформацію про користувача
        context.User = new ClaimsPrincipal(new ClaimsIdentity());

        await Task.CompletedTask;
    }

    /// <summary>
    ///     Перевіряє, чи аутентифікований поточний користувач.
    ///     Зручний метод для швидкої перевірки статусу аутентифікації.
    /// </summary>
    /// <param name="context">Контекст HTTP запиту</param>
    /// <returns>true, якщо користувач аутентифікований; інакше false</returns>
    public static bool IsAuthenticated(this HttpContext context)
    {
        return context.User?.Identity?.IsAuthenticated == true;
    }

    /// <summary>
    ///     Отримує ім'я поточного аутентифікованого користувача.
    ///     Повертає значення claim з типом ClaimTypes.Name.
    /// </summary>
    /// <param name="context">Контекст HTTP запиту</param>
    /// <returns>Ім'я користувача або null, якщо користувач не аутентифікований</returns>
    public static string? GetUserName(this HttpContext context)
    {
        return context.User?.Identity?.Name;
    }

    /// <summary>
    ///     Отримує значення конкретного claim поточного користувача.
    ///     Зручний метод для отримання додаткової інформації про користувача.
    /// </summary>
    /// <param name="context">Контекст HTTP запиту</param>
    /// <param name="claimType">Тип claim для пошуку</param>
    /// <returns>Значення claim або null, якщо claim не знайдено</returns>
    public static string? GetUserClaim(this HttpContext context, string claimType)
    {
        return context.User?.FindFirst(claimType)?.Value;
    }
}
