namespace AspNet.MinimalApi.CustomAuth.Authentication;

/// <summary>
/// Константи за замовчуванням для кастомної системи аутентифікації через куки.
/// Аналог Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.
/// </summary>
public static class CustomCookieAuthenticationDefaults
{
    /// <summary>
    /// Назва схеми аутентифікації за замовчуванням.
    /// Використовується для ідентифікації типу аутентифікації в системі.
    /// </summary>
    public const string AuthenticationScheme = "CustomCookies";

    /// <summary>
    /// Назва куки за замовчуванням, в якій зберігається інформація про аутентифікацію.
    /// Ця кука містить зашифровані дані про користувача.
    /// </summary>
    public const string CookieName = ".AspNetCore.CustomAuth";

    /// <summary>
    /// Шлях для сторінки входу за замовчуванням.
    /// Користувач буде перенаправлений на цю сторінку, якщо він не аутентифікований.
    /// </summary>
    public const string LoginPath = "/login";

    /// <summary>
    /// Шлях для сторінки виходу за замовчуванням.
    /// Використовується для завершення сесії користувача.
    /// </summary>
    public const string LogoutPath = "/logout";

    /// <summary>
    /// Шлях для сторінки відмови в доступі за замовчуванням.
    /// Користувач буде перенаправлений на цю сторінку, якщо у нього немає прав доступу.
    /// </summary>
    public const string AccessDeniedPath = "/access-denied";

    /// <summary>
    /// Час життя куки за замовчуванням (у хвилинах).
    /// Після закінчення цього часу кука стане недійсною.
    /// </summary>
    public const int DefaultExpireTimeMinutes = 30;
}
