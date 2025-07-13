using System.Security.Claims;
using AspNet.MinimalApi.CustomJwtAuth.Authentication;
using AspNet.MinimalApi.CustomJwtAuth.Models;

namespace AspNet.MinimalApi.CustomJwtAuth.Services;

/// <summary>
/// Сервіс для роботи з JWT токенами.
/// Реалізує генерацію, валідацію та управління JWT токенами.
/// </summary>
public class JwtService : IJwtService
{
    private readonly JwtTokenGenerator _tokenGenerator;
    private readonly IUserService _userService;
    private readonly JwtAuthenticationOptions _options;
    private readonly ILogger<JwtService> _logger;

    public JwtService(
        JwtTokenGenerator tokenGenerator,
        IUserService userService,
        JwtAuthenticationOptions options,
        ILogger<JwtService> logger)
    {
        _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Генерує пару токенів (access та refresh) для користувача.
    /// </summary>
    public TokenResponse GenerateTokens(User user)
    {
        var accessToken = _tokenGenerator.GenerateAccessToken(user);
        var refreshToken = _tokenGenerator.GenerateRefreshToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes);

        _logger.LogDebug("Generated tokens for user: {Username}", user.Username);

        return new TokenResponse(
            accessToken,
            refreshToken,
            expiresAt,
            "Bearer"
        );
    }

    /// <summary>
    /// Генерує токен доступу для користувача.
    /// </summary>
    public string GenerateAccessToken(User user)
    {
        return _tokenGenerator.GenerateAccessToken(user);
    }

    /// <summary>
    /// Генерує токен оновлення для користувача.
    /// </summary>
    public string GenerateRefreshToken(User user)
    {
        return _tokenGenerator.GenerateRefreshToken(user);
    }

    /// <summary>
    /// Валідує JWT токен та повертає claims.
    /// </summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        return _tokenGenerator.ValidateToken(token);
    }

    /// <summary>
    /// Валідує токен оновлення та повертає користувача.
    /// </summary>
    public async Task<User?> ValidateRefreshTokenAsync(string refreshToken)
    {
        try
        {
            var principal = _tokenGenerator.ValidateToken(refreshToken);
            
            if (principal == null)
            {
                _logger.LogWarning("Invalid refresh token provided");
                return null;
            }

            // Перевіряємо, що це дійсно refresh token
            if (!JwtTokenGenerator.IsRefreshToken(principal))
            {
                _logger.LogWarning("Token is not a refresh token");
                return null;
            }

            // Отримуємо ID користувача з токена
            var userIdClaim = JwtTokenGenerator.GetClaimValue(principal, JwtClaims.Subject);
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("Invalid user ID in refresh token");
                return null;
            }

            // Отримуємо користувача з бази даних
            var user = await _userService.GetUserByIdAsync(userId);
            
            if (user == null)
            {
                _logger.LogWarning("User not found for refresh token: {UserId}", userId);
                return null;
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating refresh token");
            return null;
        }
    }

    /// <summary>
    /// Отримує ідентифікатор користувача з токена.
    /// </summary>
    public int? GetUserIdFromToken(string token)
    {
        try
        {
            var principal = _tokenGenerator.ValidateToken(token);
            
            if (principal == null)
                return null;

            var userIdClaim = JwtTokenGenerator.GetClaimValue(principal, JwtClaims.Subject);
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return null;

            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user ID from token");
            return null;
        }
    }

    /// <summary>
    /// Перевіряє, чи є токен токеном доступу.
    /// </summary>
    public bool IsAccessToken(string token)
    {
        try
        {
            var principal = _tokenGenerator.ValidateToken(token);
            return principal != null && JwtTokenGenerator.IsAccessToken(principal);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Перевіряє, чи є токен токеном оновлення.
    /// </summary>
    public bool IsRefreshToken(string token)
    {
        try
        {
            var principal = _tokenGenerator.ValidateToken(token);
            return principal != null && JwtTokenGenerator.IsRefreshToken(principal);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Отримує час закінчення дії токена.
    /// </summary>
    public DateTime? GetTokenExpiration(string token)
    {
        try
        {
            var principal = _tokenGenerator.ValidateToken(token);
            
            if (principal == null)
                return null;

            var expClaim = JwtTokenGenerator.GetClaimValue(principal, JwtClaims.ExpirationTime);
            
            if (string.IsNullOrEmpty(expClaim) || !long.TryParse(expClaim, out var exp))
                return null;

            return DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token expiration");
            return null;
        }
    }

    /// <summary>
    /// Перевіряє, чи не прострочений токен.
    /// </summary>
    public bool IsTokenValid(string token)
    {
        try
        {
            var principal = _tokenGenerator.ValidateToken(token);
            return principal != null;
        }
        catch
        {
            return false;
        }
    }
}
