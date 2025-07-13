using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AspNet.MinimalApi.JwtAuth;

/// <summary>
/// Сервіс для роботи з JWT токенами використовуючи вбудовані можливості ASP.NET Core
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
    /// Генерує JWT токен для користувача
    /// </summary>
    public string GenerateToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "AspNet.MinimalApi.JwtAuth";
        var audience = jwtSettings["Audience"] ?? "AspNet.MinimalApi.JwtAuth.Users";
        var expirationMinutes = jwtSettings.GetValue<int>("ExpirationMinutes", 60);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, 
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        _logger.LogDebug("Generated JWT token for user {Username}", user.Username);
        
        return tokenString;
    }

    /// <summary>
    /// Валідує JWT токен (використовується middleware автоматично)
    /// </summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var issuer = jwtSettings["Issuer"] ?? "AspNet.MinimalApi.JwtAuth";
            var audience = jwtSettings["Audience"] ?? "AspNet.MinimalApi.JwtAuth.Users";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
    }

    /// <summary>
    /// Отримує інформацію з токена без валідації (для debugging)
    /// </summary>
    public JwtSecurityToken? ReadToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.ReadJwtToken(token);
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Сервіс для роботи з користувачами
/// </summary>
public class UserService
{
    private readonly List<User> _users;
    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
        
        // In-memory користувачі для демонстрації
        _users = new List<User>
        {
            new(1, "admin", BCrypt.Net.BCrypt.HashPassword("admin123"), "admin@example.com", "Admin"),
            new(2, "user", BCrypt.Net.BCrypt.HashPassword("user123"), "user@example.com", "User"),
            new(3, "manager", BCrypt.Net.BCrypt.HashPassword("manager123"), "manager@example.com", "Manager")
        };
    }

    /// <summary>
    /// Аутентифікація користувача
    /// </summary>
    public User? Authenticate(string username, string password)
    {
        var user = _users.FirstOrDefault(u => u.Username == username);
        
        if (user == null)
        {
            _logger.LogWarning("Authentication failed: user {Username} not found", username);
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            _logger.LogWarning("Authentication failed: invalid password for user {Username}", username);
            return null;
        }

        _logger.LogInformation("User {Username} authenticated successfully", username);
        return user;
    }

    /// <summary>
    /// Реєстрація нового користувача
    /// </summary>
    public User? Register(string username, string password, string email)
    {
        if (_users.Any(u => u.Username == username))
        {
            _logger.LogWarning("Registration failed: username {Username} already exists", username);
            return null;
        }

        if (_users.Any(u => u.Email == email))
        {
            _logger.LogWarning("Registration failed: email {Email} already exists", email);
            return null;
        }

        var newUser = new User(
            _users.Count + 1,
            username,
            BCrypt.Net.BCrypt.HashPassword(password),
            email,
            "User"
        );

        _users.Add(newUser);
        _logger.LogInformation("User {Username} registered successfully", username);
        
        return newUser;
    }

    /// <summary>
    /// Отримання користувача за ID
    /// </summary>
    public User? GetById(int id)
    {
        return _users.FirstOrDefault(u => u.Id == id);
    }

    /// <summary>
    /// Отримання всіх користувачів (для адміністраторів)
    /// </summary>
    public List<UserInfo> GetAllUsers()
    {
        return _users.Select(u => new UserInfo(u.Id, u.Username, u.Email ?? "", u.Role)).ToList();
    }
}

/// <summary>
/// Модель користувача
/// </summary>
public record User(int Id, string Username, string PasswordHash, string? Email, string Role);

/// <summary>
/// DTO для логіну
/// </summary>
public record LoginRequest(string Username, string Password);

/// <summary>
/// DTO для реєстрації
/// </summary>
public record RegisterRequest(string Username, string Password, string Email);

/// <summary>
/// DTO для відповіді аутентифікації
/// </summary>
public record AuthResponse(bool Success, string Message, string? Token = null, UserInfo? User = null);

/// <summary>
/// DTO для інформації про користувача
/// </summary>
public record UserInfo(int Id, string Username, string Email, string Role);

/// <summary>
/// DTO для відповіді з токеном
/// </summary>
public record TokenResponse(string Token, string TokenType, DateTime ExpiresAt, UserInfo User);
