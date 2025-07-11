using AspNet.MinimalApi.CustomAuth.Authentication;
using System.Text;

namespace AspNet.MinimalApi.CustomAuth.Middleware;

/// <summary>
///     Middleware для кастомної аутентифікації через куки.
///     Аналог вбудованого CookieAuthenticationMiddleware в ASP.NET Core.
///     Відповідає за перевірку куки аутентифікації та встановлення користувача в контексті.
/// </summary>
public class CustomAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly CustomAuthenticationOptions _options;

    /// <summary>
    ///     Конструктор middleware аутентифікації.
    /// </summary>
    /// <param name="next">Наступний middleware в конвеєрі</param>
    /// <param name="options">Опції конфігурації аутентифікації</param>
    public CustomAuthenticationMiddleware(RequestDelegate next, CustomAuthenticationOptions options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    ///     Основний метод middleware, що виконується для кожного HTTP запиту.
    ///     Перевіряє наявність та дійсність куки аутентифікації.
    /// </summary>
    /// <param name="context">Контекст HTTP запиту</param>
    public async Task InvokeAsync(HttpContext context)
    {
        // Перевіряємо наявність куки аутентифікації
        if (context.Request.Cookies.TryGetValue(_options.Cookie.Name!, out var cookieValue) &&
            !string.IsNullOrEmpty(cookieValue))
        {
            try
            {
                // Декодуємо кукі з Base64
                var decodedTicket = Encoding.UTF8.GetString(Convert.FromBase64String(cookieValue));

                // Десеріалізуємо тікет аутентифікації
                var ticket = CustomAuthenticationTicket.Deserialize(decodedTicket);

                if (ticket != null && ticket.IsValid())
                {
                    // Встановлюємо користувача в контексті
                    context.User = ticket.Principal;

                    // Якщо увімкнено ковзаючий термін дії, оновлюємо кукі
                    if (_options.SlidingExpiration && ShouldRenewTicket(ticket))
                    {
                        await RenewTicketAsync(context, ticket);
                    }
                }
                else
                {
                    // Якщо тікет недійсний, видаляємо кукі
                    RemoveAuthenticationCookie(context);
                }
            }
            catch
            {
                // Якщо не вдалося обробити кукі, видаляємо її
                RemoveAuthenticationCookie(context);
            }
        }

        await _next(context);
    }

    /// <summary>
    ///     Визначає, чи потрібно оновити тікет аутентифікації.
    ///     Тікет оновлюється, якщо пройшло більше половини часу його життя.
    /// </summary>
    /// <param name="ticket">Тікет аутентифікації для перевірки</param>
    /// <returns>true, якщо тікет потрібно оновити; інакше false</returns>
    private bool ShouldRenewTicket(CustomAuthenticationTicket ticket)
    {
        if (ticket.ExpiresUtc == null)
            return false;

        var timeElapsed = DateTimeOffset.UtcNow - ticket.IssuedUtc;
        var timeRemaining = ticket.ExpiresUtc.Value - DateTimeOffset.UtcNow;

        // Оновлюємо, якщо пройшло більше половини часу життя тікета
        return timeElapsed > timeRemaining;
    }

    /// <summary>
    ///     Асинхронно оновлює тікет аутентифікації та встановлює нову кукі.
    ///     Продовжує термін дії аутентифікації користувача.
    /// </summary>
    /// <param name="context">Контекст HTTP запиту</param>
    /// <param name="oldTicket">Старий тікет аутентифікації</param>
    private async Task RenewTicketAsync(HttpContext context, CustomAuthenticationTicket oldTicket)
    {
        // Створюємо новий тікет з оновленим терміном дії
        var newTicket = new CustomAuthenticationTicket(oldTicket.Principal, oldTicket.AuthenticationScheme)
        {
            ExpiresUtc = DateTimeOffset.UtcNow.Add(_options.ExpireTimeSpan)
        };

        // Встановлюємо нову кукі
        await SetAuthenticationCookieAsync(context, newTicket);
    }

    /// <summary>
    ///     Асинхронно встановлює кукі аутентифікації в HTTP відповіді.
    ///     Серіалізує тікет та зберігає його в кукі браузера.
    /// </summary>
    /// <param name="context">Контекст HTTP запиту</param>
    /// <param name="ticket">Тікет аутентифікації для збереження</param>
    private async Task SetAuthenticationCookieAsync(HttpContext context, CustomAuthenticationTicket ticket)
    {
        // Серіалізуємо тікет в JSON
        var serializedTicket = ticket.Serialize();

        // Кодуємо в Base64 для безпечного зберігання в кукі
        var encodedTicket = Convert.ToBase64String(Encoding.UTF8.GetBytes(serializedTicket));

        // Налаштовуємо опції куки
        var cookieOptions = new CookieOptions
        {
            HttpOnly = _options.Cookie.HttpOnly,
            Secure = _options.Cookie.SecurePolicy == CookieSecurePolicy.Always ||
                     _options.Cookie.SecurePolicy == CookieSecurePolicy.SameAsRequest && context.Request.IsHttps,
            SameSite = _options.Cookie.SameSite,
            Expires = ticket.ExpiresUtc?.DateTime,
            Path = _options.Cookie.Path ?? "/"
        };

        // Встановлюємо кукі
        context.Response.Cookies.Append(_options.Cookie.Name!, encodedTicket, cookieOptions);

        await Task.CompletedTask;
    }

    /// <summary>
    ///     Видаляє кукі аутентифікації з HTTP відповіді.
    ///     Використовується при виході користувача або коли кукі стає недійсною.
    /// </summary>
    /// <param name="context">Контекст HTTP запиту</param>
    private void RemoveAuthenticationCookie(HttpContext context)
    {
        context.Response.Cookies.Delete(_options.Cookie.Name!, new CookieOptions
        {
            Path = _options.Cookie.Path ?? "/"
        });
    }
}
