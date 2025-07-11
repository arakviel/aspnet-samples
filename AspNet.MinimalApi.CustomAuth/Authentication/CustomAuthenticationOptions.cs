using Microsoft.AspNetCore.Http;

namespace AspNet.MinimalApi.CustomAuth.Authentication;

/// <summary>
/// Опції конфігурації для кастомної системи аутентифікації через куки.
/// Аналог Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationOptions.
/// </summary>
public class CustomAuthenticationOptions
{
    /// <summary>
    /// Шлях для сторінки входу.
    /// Користувач буде перенаправлений на цю сторінку, якщо він не аутентифікований.
    /// </summary>
    public PathString LoginPath { get; set; } = CustomCookieAuthenticationDefaults.LoginPath;

    /// <summary>
    /// Шлях для сторінки виходу.
    /// Використовується для завершення сесії користувача.
    /// </summary>
    public PathString LogoutPath { get; set; } = CustomCookieAuthenticationDefaults.LogoutPath;

    /// <summary>
    /// Шлях для сторінки відмови в доступі.
    /// Користувач буде перенаправлений на цю сторінку, якщо у нього немає прав доступу.
    /// </summary>
    public PathString AccessDeniedPath { get; set; } = CustomCookieAuthenticationDefaults.AccessDeniedPath;

    /// <summary>
    /// Опції конфігурації куки для аутентифікації.
    /// Містить налаштування безпеки та поведінки куки.
    /// </summary>
    public CookieBuilder Cookie { get; set; } = new()
    {
        Name = CustomCookieAuthenticationDefaults.CookieName,
        HttpOnly = true,
        SecurePolicy = CookieSecurePolicy.SameAsRequest,
        SameSite = SameSiteMode.Lax
    };

    /// <summary>
    /// Час життя куки аутентифікації.
    /// Після закінчення цього часу кука стане недійсною.
    /// </summary>
    public TimeSpan ExpireTimeSpan { get; set; } = TimeSpan.FromMinutes(CustomCookieAuthenticationDefaults.DefaultExpireTimeMinutes);

    /// <summary>
    /// Визначає, чи має кука "ковзаючий" термін дії.
    /// Якщо true, термін дії куки буде продовжуватися при кожному запиті.
    /// </summary>
    public bool SlidingExpiration { get; set; } = true;
}
