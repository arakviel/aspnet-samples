using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AspNet.MinimalApi.ExtendedJwtAuth.Models;
using Microsoft.IdentityModel.Tokens;

namespace AspNet.MinimalApi.ExtendedJwtAuth.Services;

/// <summary>
///     Сервіс для роботи з JWT токенами
/// </summary>
public class JwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;

    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    ///     Генерує JWT access токен для користувача
    /// </summary>
    /// <param name="user">Користувач</param>
    /// <param name="roles">Ролі користувача</param>
    /// <returns>JWT токен</returns>
    public string GenerateAccessToken(User user, IList<string>? roles = null)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ??
                        throw new InvalidOperationException("JWT SecretKey не налаштований");
        var issuer = jwtSettings["Issuer"] ?? "AspNet.MinimalApi.ExtendedJwtAuth";
        var audience = jwtSettings["Audience"] ?? "AspNet.MinimalApi.ExtendedJwtAuth.Client";
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Створюємо claims для користувача
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        // Додаємо додаткові claims
        if (!string.IsNullOrEmpty(user.FirstName))
            claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));

        if (!string.IsNullOrEmpty(user.LastName))
            claims.Add(new Claim(ClaimTypes.Surname, user.LastName));

        // Додаємо ролі як claims
        if (roles != null)
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation("JWT токен згенеровано для користувача {UserId}", user.Id);

        return tokenString;
    }

    /// <summary>
    ///     Генерує refresh токен
    /// </summary>
    /// <returns>Refresh токен</returns>
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    ///     Отримує principal з JWT токена (без валідації терміну дії)
    /// </summary>
    /// <param name="token">JWT токен</param>
    /// <returns>ClaimsPrincipal або null</returns>
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ??
                        throw new InvalidOperationException("JWT SecretKey не налаштований");

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateLifetime = false // Не перевіряємо термін дії
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Невірний токен");

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Помилка при валідації токена");
            return null;
        }
    }

    /// <summary>
    ///     Отримує час закінчення дії токена
    /// </summary>
    /// <returns>Час закінчення дії</returns>
    public DateTime GetTokenExpiration()
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");
        return DateTime.UtcNow.AddMinutes(expirationMinutes);
    }

    /// <summary>
    ///     Отримує час закінчення дії refresh токена
    /// </summary>
    /// <returns>Час закінчення дії refresh токена</returns>
    public DateTime GetRefreshTokenExpiration()
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var refreshExpirationDays = int.Parse(jwtSettings["RefreshExpirationDays"] ?? "7");
        return DateTime.UtcNow.AddDays(refreshExpirationDays);
    }
}