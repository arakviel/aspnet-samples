using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AspNet.MinimalApi.SimpleJwtWithCookiesAuth;

/// <summary>
/// Простий JWT генератор для cookies
/// </summary>
public class SimpleJwtCookies
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly int _expirationDays;

    public SimpleJwtCookies(string secretKey, string issuer = "SimpleJwtCookiesAuth", int expirationDays = 14)
    {
        _secretKey = secretKey;
        _issuer = issuer;
        _expirationDays = expirationDays;
    }

    /// <summary>
    /// Генерує JWT токен для користувача
    /// </summary>
    public string GenerateToken(int userId, string username, string role = "User")
    {
        var header = new { alg = "HS256", typ = "JWT" };
        
        var payload = new
        {
            sub = userId.ToString(),
            username = username,
            role = role,
            iss = _issuer,
            exp = DateTimeOffset.UtcNow.AddDays(_expirationDays).ToUnixTimeSeconds()
        };

        var encodedHeader = EncodeBase64Url(JsonSerializer.Serialize(header));
        var encodedPayload = EncodeBase64Url(JsonSerializer.Serialize(payload));
        var signature = CreateSignature(encodedHeader, encodedPayload);

        return $"{encodedHeader}.{encodedPayload}.{signature}";
    }

    /// <summary>
    /// Валідує JWT токен і повертає claims
    /// </summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return null;

            var header = parts[0];
            var payload = parts[1];
            var signature = parts[2];

            // Перевіряємо підпис
            if (CreateSignature(header, payload) != signature) return null;

            // Декодуємо payload
            var payloadJson = DecodeBase64Url(payload);
            var payloadData = JsonSerializer.Deserialize<JsonElement>(payloadJson);

            // Перевіряємо термін дії
            if (payloadData.TryGetProperty("exp", out var expElement))
            {
                var exp = expElement.GetInt64();
                if (DateTimeOffset.FromUnixTimeSeconds(exp) <= DateTimeOffset.UtcNow)
                    return null;
            }

            // Створюємо claims
            var claims = new List<Claim>();
            
            if (payloadData.TryGetProperty("sub", out var sub))
                claims.Add(new Claim(ClaimTypes.NameIdentifier, sub.GetString()!));
            
            if (payloadData.TryGetProperty("username", out var username))
                claims.Add(new Claim(ClaimTypes.Name, username.GetString()!));
            
            if (payloadData.TryGetProperty("role", out var role))
                claims.Add(new Claim(ClaimTypes.Role, role.GetString()!));

            var identity = new ClaimsIdentity(claims, "JWT");
            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Створює HMAC SHA256 підпис
    /// </summary>
    private string CreateSignature(string encodedHeader, string encodedPayload)
    {
        var data = $"{encodedHeader}.{encodedPayload}";
        var key = Encoding.UTF8.GetBytes(_secretKey);
        
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        
        return EncodeBase64Url(hash);
    }

    /// <summary>
    /// Кодує в Base64URL
    /// </summary>
    private static string EncodeBase64Url(string data) => EncodeBase64Url(Encoding.UTF8.GetBytes(data));
    
    private static string EncodeBase64Url(byte[] data)
    {
        var base64 = Convert.ToBase64String(data);
        return base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    /// <summary>
    /// Декодує з Base64URL
    /// </summary>
    private static string DecodeBase64Url(string data)
    {
        var base64 = data.Replace('-', '+').Replace('_', '/');
        
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        
        var bytes = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(bytes);
    }
}

/// <summary>
/// Простий middleware для JWT cookies аутентифікації
/// </summary>
public class SimpleJwtCookiesMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SimpleJwtCookies _jwt;
    private const string CookieName = "auth_token";

    public SimpleJwtCookiesMiddleware(RequestDelegate next, SimpleJwtCookies jwt)
    {
        _next = next;
        _jwt = jwt;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Читаємо токен з cookie
        var token = context.Request.Cookies[CookieName];

        if (!string.IsNullOrEmpty(token))
        {
            var principal = _jwt.ValidateToken(token);
            if (principal != null)
            {
                context.User = principal;
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Розширення для роботи з JWT cookies
/// </summary>
public static class JwtCookiesExtensions
{
    /// <summary>
    /// Встановлює JWT токен в HttpOnly cookie
    /// </summary>
    public static void SetJwtCookie(this HttpContext context, string token, int expirationDays = 14)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,                    // ✅ JavaScript не може прочитати
            Secure = true,                      // ✅ Тільки через HTTPS
            SameSite = SameSiteMode.Lax,       // ✅ Захист від CSRF
            Expires = DateTimeOffset.UtcNow.AddDays(expirationDays),
            Path = "/"
        };

        context.Response.Cookies.Append("auth_token", token, cookieOptions);
    }

    /// <summary>
    /// Видаляє JWT cookie
    /// </summary>
    public static void DeleteJwtCookie(this HttpContext context)
    {
        context.Response.Cookies.Delete("auth_token");
    }

    /// <summary>
    /// Перевіряє, чи аутентифікований користувач
    /// </summary>
    public static bool IsAuthenticated(this HttpContext context)
    {
        return context.User.Identity?.IsAuthenticated == true;
    }

    /// <summary>
    /// Отримує роль користувача
    /// </summary>
    public static string? GetUserRole(this HttpContext context)
    {
        return context.User.FindFirst(ClaimTypes.Role)?.Value;
    }

    /// <summary>
    /// Перевіряє, чи є користувач адміністратором
    /// </summary>
    public static bool IsAdmin(this HttpContext context)
    {
        return context.GetUserRole() == "Admin";
    }
}

/// <summary>
/// Простий користувач
/// </summary>
public record User(int Id, string Username, string PasswordHash, string Role = "User");

/// <summary>
/// DTO для логіну
/// </summary>
public record LoginRequest(string Username, string Password);

/// <summary>
/// DTO для реєстрації
/// </summary>
public record RegisterRequest(string Username, string Password);

/// <summary>
/// DTO для відповіді (БЕЗ токена - він в cookie!)
/// </summary>
public record AuthResponse(bool Success, string Message, UserInfo? User = null);

/// <summary>
/// Інформація про користувача
/// </summary>
public record UserInfo(int Id, string Username, string Role);
