using System.Security.Claims;
using AspNet.MinimalApi.CustomJwtAuth.Authentication;

namespace AspNet.MinimalApi.CustomJwtAuth.Middleware;

/// <summary>
/// Кастомний middleware для JWT аутентифікації.
/// Перевіряє наявність та валідність JWT токенів у кожному запиті.
/// </summary>
public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JwtTokenGenerator _tokenGenerator;
    private readonly ILogger<JwtAuthenticationMiddleware> _logger;

    /// <summary>
    /// Константа для назви заголовка Authorization.
    /// </summary>
    private const string AuthorizationHeader = "Authorization";

    /// <summary>
    /// Префікс для Bearer токенів.
    /// </summary>
    private const string BearerPrefix = "Bearer ";

    public JwtAuthenticationMiddleware(
        RequestDelegate next,
        JwtTokenGenerator tokenGenerator,
        ILogger<JwtAuthenticationMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Обробляє HTTP запит та виконує JWT аутентифікацію.
    /// </summary>
    /// <param name="context">HTTP контекст запиту</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Витягуємо токен з заголовка Authorization
            var token = ExtractTokenFromHeader(context);

            if (!string.IsNullOrEmpty(token))
            {
                // Валідуємо токен та встановлюємо користувача
                var principal = _tokenGenerator.ValidateToken(token);
                
                if (principal != null)
                {
                    // Перевіряємо, що це токен доступу
                    if (JwtTokenGenerator.IsAccessToken(principal))
                    {
                        // Встановлюємо аутентифікованого користувача в контекст
                        context.User = principal;
                        
                        // Додаємо додаткову інформацію в Items для зручності
                        SetUserInfoInContext(context, principal);
                        
                        _logger.LogDebug("User authenticated successfully: {Username}", 
                            JwtTokenGenerator.GetClaimValue(principal, JwtClaims.Username));
                    }
                    else
                    {
                        _logger.LogWarning("Invalid token type provided in Authorization header");
                    }
                }
                else
                {
                    _logger.LogWarning("Invalid JWT token provided in Authorization header");
                }
            }

            // Продовжуємо обробку запиту
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during JWT authentication");
            
            // У разі помилки продовжуємо без аутентифікації
            await _next(context);
        }
    }

    /// <summary>
    /// Витягує JWT токен з заголовка Authorization.
    /// </summary>
    /// <param name="context">HTTP контекст</param>
    /// <returns>JWT токен або null, якщо токен не знайдено</returns>
    private static string? ExtractTokenFromHeader(HttpContext context)
    {
        var authorizationHeader = context.Request.Headers[AuthorizationHeader].FirstOrDefault();
        
        if (string.IsNullOrEmpty(authorizationHeader))
        {
            return null;
        }

        if (!authorizationHeader.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return authorizationHeader[BearerPrefix.Length..].Trim();
    }

    /// <summary>
    /// Встановлює інформацію про користувача в HttpContext.Items для зручного доступу.
    /// </summary>
    /// <param name="context">HTTP контекст</param>
    /// <param name="principal">Принципал аутентифікованого користувача</param>
    private static void SetUserInfoInContext(HttpContext context, ClaimsPrincipal principal)
    {
        context.Items["IsAuthenticated"] = true;
        context.Items["UserId"] = JwtTokenGenerator.GetClaimValue(principal, JwtClaims.Subject);
        context.Items["Username"] = JwtTokenGenerator.GetClaimValue(principal, JwtClaims.Username);
        context.Items["Email"] = JwtTokenGenerator.GetClaimValue(principal, JwtClaims.Email);
        context.Items["Role"] = JwtTokenGenerator.GetClaimValue(principal, JwtClaims.Role);
    }

    /// <summary>
    /// Перевіряє, чи аутентифікований користувач.
    /// </summary>
    /// <param name="context">HTTP контекст</param>
    /// <returns>true, якщо користувач аутентифікований</returns>
    public static bool IsAuthenticated(HttpContext context)
    {
        return context.Items["IsAuthenticated"] as bool? ?? false;
    }

    /// <summary>
    /// Отримує ID аутентифікованого користувача.
    /// </summary>
    /// <param name="context">HTTP контекст</param>
    /// <returns>ID користувача або null</returns>
    public static string? GetUserId(HttpContext context)
    {
        return context.Items["UserId"] as string;
    }

    /// <summary>
    /// Отримує ім'я аутентифікованого користувача.
    /// </summary>
    /// <param name="context">HTTP контекст</param>
    /// <returns>Ім'я користувача або null</returns>
    public static string? GetUsername(HttpContext context)
    {
        return context.Items["Username"] as string;
    }

    /// <summary>
    /// Отримує роль аутентифікованого користувача.
    /// </summary>
    /// <param name="context">HTTP контекст</param>
    /// <returns>Роль користувача або null</returns>
    public static string? GetUserRole(HttpContext context)
    {
        return context.Items["Role"] as string;
    }

    /// <summary>
    /// Перевіряє, чи має користувач певну роль.
    /// </summary>
    /// <param name="context">HTTP контекст</param>
    /// <param name="role">Роль для перевірки</param>
    /// <returns>true, якщо користувач має роль</returns>
    public static bool HasRole(HttpContext context, string role)
    {
        var userRole = GetUserRole(context);
        return string.Equals(userRole, role, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Перевіряє, чи є користувач адміністратором.
    /// </summary>
    /// <param name="context">HTTP контекст</param>
    /// <returns>true, якщо користувач є адміністратором</returns>
    public static bool IsAdmin(HttpContext context)
    {
        return HasRole(context, JwtClaims.AdminRole);
    }
}
