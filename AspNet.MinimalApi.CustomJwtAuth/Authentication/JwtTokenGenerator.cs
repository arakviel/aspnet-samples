using AspNet.MinimalApi.CustomJwtAuth.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AspNet.MinimalApi.CustomJwtAuth.Authentication;

/// <summary>
///     Кастомний генератор JWT токенів без використання готових бібліотек.
///     Реалізує повний цикл створення, підпису та валідації JWT токенів.
/// </summary>
public class JwtTokenGenerator
{
    private readonly ILogger<JwtTokenGenerator> _logger;
    private readonly JwtAuthenticationOptions _options;

    public JwtTokenGenerator(JwtAuthenticationOptions options, ILogger<JwtTokenGenerator> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Генерує JWT токен доступу для користувача.
    /// </summary>
    /// <param name="user">Користувач, для якого генерується токен</param>
    /// <returns>JWT токен у вигляді рядка</returns>
    public string GenerateAccessToken(User user)
    {
        var claims = CreateAccessTokenClaims(user);
        var expirationTime = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes);

        return GenerateToken(claims, expirationTime, JwtClaims.AccessTokenType);
    }

    /// <summary>
    ///     Генерує токен оновлення для користувача.
    /// </summary>
    /// <param name="user">Користувач, для якого генерується токен</param>
    /// <returns>Токен оновлення у вигляді рядка</returns>
    public string GenerateRefreshToken(User user)
    {
        var claims = CreateRefreshTokenClaims(user);
        var expirationTime = DateTime.UtcNow.AddDays(_options.RefreshTokenExpirationDays);

        return GenerateToken(claims, expirationTime, JwtClaims.RefreshTokenType);
    }

    /// <summary>
    ///     Валідує JWT токен та повертає claims.
    /// </summary>
    /// <param name="token">JWT токен для валідації</param>
    /// <returns>Claims з токена або null, якщо токен недійсний</returns>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                _logger.LogWarning("Invalid JWT token format");
                return null;
            }

            var header = parts[0];
            var payload = parts[1];
            var signature = parts[2];

            // Перевіряємо підпис
            if (!VerifySignature(header, payload, signature))
            {
                _logger.LogWarning("JWT token signature verification failed");
                return null;
            }

            // Декодуємо payload
            var payloadJson = DecodeBase64Url(payload);
            var claims = ParseClaims(payloadJson);

            // Перевіряємо термін дії
            if (!ValidateExpiration(claims))
            {
                _logger.LogWarning("JWT token has expired");
                return null;
            }

            // Перевіряємо issuer та audience
            if (!ValidateIssuerAndAudience(claims))
            {
                _logger.LogWarning("JWT token issuer or audience validation failed");
                return null;
            }

            var identity = new ClaimsIdentity(claims, "JWT");
            return new ClaimsPrincipal(identity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating JWT token");
            return null;
        }
    }

    /// <summary>
    ///     Створює claims для токена доступу.
    /// </summary>
    private Dictionary<string, object> CreateAccessTokenClaims(User user)
    {
        var claims = new Dictionary<string, object>
        {
            [JwtClaims.Subject] = user.Id.ToString(),
            [JwtClaims.Username] = user.Username,
            [JwtClaims.Email] = user.Email,
            [JwtClaims.Role] = user.Role,
            [JwtClaims.TokenType] = JwtClaims.AccessTokenType
        };

        AddStandardClaims(claims);
        return claims;
    }

    /// <summary>
    ///     Створює claims для токена оновлення.
    /// </summary>
    private Dictionary<string, object> CreateRefreshTokenClaims(User user)
    {
        var claims = new Dictionary<string, object>
        {
            [JwtClaims.Subject] = user.Id.ToString(),
            [JwtClaims.TokenType] = JwtClaims.RefreshTokenType
        };

        AddStandardClaims(claims);
        return claims;
    }

    /// <summary>
    ///     Додає стандартні claims до токена.
    /// </summary>
    private void AddStandardClaims(Dictionary<string, object> claims)
    {
        var now = DateTime.UtcNow;

        claims[JwtClaims.Issuer] = _options.Issuer;
        claims[JwtClaims.Audience] = _options.Audience;

        if (_options.IncludeIssuedAt)
        {
            claims[JwtClaims.IssuedAt] = ((DateTimeOffset)now).ToUnixTimeSeconds();
        }

        if (_options.IncludeJwtId)
        {
            claims[JwtClaims.JwtId] = Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    ///     Генерує JWT токен з заданими claims.
    /// </summary>
    private string GenerateToken(Dictionary<string, object> claims, DateTime expirationTime, string tokenType)
    {
        // Додаємо час закінчення дії
        claims[JwtClaims.ExpirationTime] = ((DateTimeOffset)expirationTime).ToUnixTimeSeconds();

        // Створюємо header
        var header = new Dictionary<string, object>
        {
            ["alg"] = _options.Algorithm,
            ["typ"] = "JWT"
        };

        // Кодуємо header та payload
        var encodedHeader = EncodeBase64Url(JsonSerializer.Serialize(header));
        var encodedPayload = EncodeBase64Url(JsonSerializer.Serialize(claims));

        // Створюємо підпис
        var signature = CreateSignature(encodedHeader, encodedPayload);

        return $"{encodedHeader}.{encodedPayload}.{signature}";
    }

    /// <summary>
    ///     Створює підпис для JWT токена.
    /// </summary>
    private string CreateSignature(string encodedHeader, string encodedPayload)
    {
        var data = $"{encodedHeader}.{encodedPayload}";
        var key = Encoding.UTF8.GetBytes(_options.SecretKey);

        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));

        return EncodeBase64Url(hash);
    }

    /// <summary>
    ///     Перевіряє підпис JWT токена.
    /// </summary>
    private bool VerifySignature(string encodedHeader, string encodedPayload, string signature)
    {
        var expectedSignature = CreateSignature(encodedHeader, encodedPayload);
        return signature == expectedSignature;
    }

    /// <summary>
    ///     Кодує дані в Base64URL формат.
    /// </summary>
    private static string EncodeBase64Url(string data)
    {
        return EncodeBase64Url(Encoding.UTF8.GetBytes(data));
    }

    /// <summary>
    ///     Кодує байти в Base64URL формат.
    /// </summary>
    private static string EncodeBase64Url(byte[] data)
    {
        var base64 = Convert.ToBase64String(data);
        return base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    /// <summary>
    ///     Декодує дані з Base64URL формату.
    /// </summary>
    private static string DecodeBase64Url(string data)
    {
        var base64 = data.Replace('-', '+').Replace('_', '/');

        // Додаємо padding якщо потрібно
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        var bytes = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    ///     Парсить claims з JSON payload.
    /// </summary>
    private static List<Claim> ParseClaims(string payloadJson)
    {
        var claims = new List<Claim>();

        using var document = JsonDocument.Parse(payloadJson);
        var root = document.RootElement;

        foreach (var property in root.EnumerateObject())
        {
            var value = property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString()!,
                JsonValueKind.Number => property.Value.GetInt64().ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => property.Value.ToString()
            };

            claims.Add(new Claim(property.Name, value));
        }

        return claims;
    }

    /// <summary>
    ///     Перевіряє термін дії токена.
    /// </summary>
    private bool ValidateExpiration(List<Claim> claims)
    {
        var expClaim = claims.FirstOrDefault(c => c.Type == JwtClaims.ExpirationTime);
        if (expClaim == null) return false;

        if (!long.TryParse(expClaim.Value, out var exp)) return false;

        var expirationTime = DateTimeOffset.FromUnixTimeSeconds(exp);
        var now = DateTimeOffset.UtcNow.AddSeconds(_options.ClockSkewSeconds);

        return expirationTime > now;
    }

    /// <summary>
    ///     Перевіряє issuer та audience токена.
    /// </summary>
    private bool ValidateIssuerAndAudience(List<Claim> claims)
    {
        var issuerClaim = claims.FirstOrDefault(c => c.Type == JwtClaims.Issuer);
        var audienceClaim = claims.FirstOrDefault(c => c.Type == JwtClaims.Audience);

        return issuerClaim?.Value == _options.Issuer &&
               audienceClaim?.Value == _options.Audience;
    }

    /// <summary>
    ///     Отримує значення claim з токена.
    /// </summary>
    public static string? GetClaimValue(ClaimsPrincipal principal, string claimType)
    {
        return principal.FindFirst(claimType)?.Value;
    }

    /// <summary>
    ///     Перевіряє, чи є токен токеном доступу.
    /// </summary>
    public static bool IsAccessToken(ClaimsPrincipal principal)
    {
        var tokenType = GetClaimValue(principal, JwtClaims.TokenType);
        return tokenType == JwtClaims.AccessTokenType;
    }

    /// <summary>
    ///     Перевіряє, чи є токен токеном оновлення.
    /// </summary>
    public static bool IsRefreshToken(ClaimsPrincipal principal)
    {
        var tokenType = GetClaimValue(principal, JwtClaims.TokenType);
        return tokenType == JwtClaims.RefreshTokenType;
    }
}
