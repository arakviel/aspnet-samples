using System.Security.Claims;
using System.Text.Json;

namespace AspNet.MinimalApi.CustomAuth.Authentication;

/// <summary>
/// Тікет аутентифікації, що містить інформацію про аутентифікованого користувача.
/// Аналог Microsoft.AspNetCore.Authentication.AuthenticationTicket.
/// </summary>
public class CustomAuthenticationTicket
{
    /// <summary>
    /// Принципал користувача з claims (твердженнями).
    /// Містить інформацію про ідентичність користувача та його права.
    /// </summary>
    public ClaimsPrincipal Principal { get; set; }

    /// <summary>
    /// Назва схеми аутентифікації.
    /// Ідентифікує тип аутентифікації, що використовується.
    /// </summary>
    public string AuthenticationScheme { get; set; }

    /// <summary>
    /// Час створення тікета.
    /// Використовується для визначення віку тікета.
    /// </summary>
    public DateTimeOffset IssuedUtc { get; set; }

    /// <summary>
    /// Час закінчення дії тікета.
    /// Після цього часу тікет стає недійсним.
    /// </summary>
    public DateTimeOffset? ExpiresUtc { get; set; }

    /// <summary>
    /// Конструктор тікета аутентифікації.
    /// </summary>
    /// <param name="principal">Принципал користувача з claims</param>
    /// <param name="authenticationScheme">Назва схеми аутентифікації</param>
    public CustomAuthenticationTicket(ClaimsPrincipal principal, string authenticationScheme)
    {
        Principal = principal ?? throw new ArgumentNullException(nameof(principal));
        AuthenticationScheme = authenticationScheme ?? throw new ArgumentNullException(nameof(authenticationScheme));
        IssuedUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Серіалізує тікет в JSON рядок для зберігання в куці.
    /// Перетворює об'єкт тікета в рядок, який можна зберегти в куці браузера.
    /// </summary>
    /// <returns>JSON представлення тікета</returns>
    public string Serialize()
    {
        var ticketData = new
        {
            Claims = Principal.Claims.Select(c => new { c.Type, c.Value }).ToArray(),
            AuthenticationScheme,
            IssuedUtc = IssuedUtc.ToString("O"), // ISO 8601 format
            ExpiresUtc = ExpiresUtc?.ToString("O")
        };

        return JsonSerializer.Serialize(ticketData);
    }

    /// <summary>
    /// Десеріалізує тікет з JSON рядка.
    /// Відновлює об'єкт тікета з рядка, збереженого в куці браузера.
    /// </summary>
    /// <param name="serializedTicket">JSON представлення тікета</param>
    /// <returns>Відновлений тікет аутентифікації або null, якщо десеріалізація не вдалася</returns>
    public static CustomAuthenticationTicket? Deserialize(string serializedTicket)
    {
        try
        {
            using var document = JsonDocument.Parse(serializedTicket);
            var root = document.RootElement;

            // Відновлення claims
            var claims = new List<Claim>();
            if (root.TryGetProperty("Claims", out var claimsElement))
            {
                foreach (var claimElement in claimsElement.EnumerateArray())
                {
                    if (claimElement.TryGetProperty("Type", out var typeElement) &&
                        claimElement.TryGetProperty("Value", out var valueElement))
                    {
                        claims.Add(new Claim(typeElement.GetString()!, valueElement.GetString()!));
                    }
                }
            }

            // Створення ClaimsIdentity та ClaimsPrincipal
            var authenticationScheme = root.GetProperty("AuthenticationScheme").GetString()!;
            var identity = new ClaimsIdentity(claims, authenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Створення тікета
            var ticket = new CustomAuthenticationTicket(principal, authenticationScheme);

            // Відновлення часових міток
            if (root.TryGetProperty("IssuedUtc", out var issuedElement))
            {
                ticket.IssuedUtc = DateTimeOffset.Parse(issuedElement.GetString()!);
            }

            if (root.TryGetProperty("ExpiresUtc", out var expiresElement) && 
                !expiresElement.ValueKind.Equals(JsonValueKind.Null))
            {
                ticket.ExpiresUtc = DateTimeOffset.Parse(expiresElement.GetString()!);
            }

            return ticket;
        }
        catch
        {
            // Якщо десеріалізація не вдалася, повертаємо null
            return null;
        }
    }

    /// <summary>
    /// Перевіряє, чи є тікет дійсним (не прострочений).
    /// </summary>
    /// <returns>true, якщо тікет дійсний; false, якщо прострочений</returns>
    public bool IsValid()
    {
        return ExpiresUtc == null || ExpiresUtc > DateTimeOffset.UtcNow;
    }
}
