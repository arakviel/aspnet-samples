using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace AspNet.MinimalApi.AuthSmsIdentity.Services;

public interface IJwtService
{
    string GenerateJwtToken(IdentityUser user);
    void SetJwtCookie(HttpResponse response, string token);
    void ClearJwtCookie(HttpResponse response);
    ClaimsPrincipal? ValidateToken(string token);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        var jwtSettings = _configuration.GetSection("JwtSettings");
        _secretKey = jwtSettings["SecretKey"] ?? "MyVeryLongSecretKeyForJWTTokenGeneration123456789";
        _issuer = jwtSettings["Issuer"] ?? "AspNet.MinimalApi.AuthSmsIdentity";
        _audience = jwtSettings["Audience"] ?? "AspNet.MinimalApi.AuthSmsIdentity.Client";
        _expirationMinutes = jwtSettings.GetValue<int>("ExpirationMinutes", 60);
    }

    public string GenerateJwtToken(IdentityUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.MobilePhone, user.PhoneNumber ?? ""),
            new("phoneNumberConfirmed", user.PhoneNumberConfirmed.ToString()),
            new("twoFactorEnabled", user.TwoFactorEnabled.ToString()),
            new("jti", Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        _logger.LogInformation("JWT token generated for user {UserId}", user.Id);
        
        return tokenString;
    }

    public void SetJwtCookie(HttpResponse response, string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, // Захист від XSS
            Secure = true,   // Тільки HTTPS (в production)
            SameSite = SameSiteMode.Strict, // CSRF захист
            Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
            Path = "/"
        };

        response.Cookies.Append("jwt", token, cookieOptions);
        
        _logger.LogInformation("JWT cookie set with expiration: {Expiration}", cookieOptions.Expires);
    }

    public void ClearJwtCookie(HttpResponse response)
    {
        response.Cookies.Delete("jwt", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });
        
        _logger.LogInformation("JWT cookie cleared");
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "JWT token validation failed");
            return null;
        }
    }
}
