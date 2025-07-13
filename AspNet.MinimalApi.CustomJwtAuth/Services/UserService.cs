using AspNet.MinimalApi.CustomJwtAuth.Authentication;
using AspNet.MinimalApi.CustomJwtAuth.Data;
using AspNet.MinimalApi.CustomJwtAuth.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

namespace AspNet.MinimalApi.CustomJwtAuth.Services;

/// <summary>
///     Сервіс для роботи з користувачами.
///     Реалізує бізнес-логіку управління користувачами та аутентифікації.
/// </summary>
public class UserService : IUserService
{
    private readonly AuthDbContext _context;
    private readonly JwtAuthenticationOptions _jwtOptions;
    private readonly ILogger<UserService> _logger;
    private readonly JwtTokenGenerator _tokenGenerator;

    public UserService(
        AuthDbContext context,
        JwtTokenGenerator tokenGenerator,
        JwtAuthenticationOptions jwtOptions,
        ILogger<UserService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
        _jwtOptions = jwtOptions ?? throw new ArgumentNullException(nameof(jwtOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Реєструє нового користувача в системі.
    /// </summary>
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // Валідація вхідних даних
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Email))
            {
                return new AuthResponse(false, "Всі поля є обов'язковими");
            }

            // Перевірка мінімальної довжини пароля
            if (request.Password.Length < 6)
            {
                return new AuthResponse(false, "Пароль має містити принаймні 6 символів");
            }

            // Перевірка формату email (базова)
            if (!IsValidEmail(request.Email))
            {
                return new AuthResponse(false, "Невірний формат електронної пошти");
            }

            // Перевірка унікальності username
            if (await UserExistsAsync(request.Username))
            {
                return new AuthResponse(false, "Користувач з таким ім'ям вже існує");
            }

            // Перевірка унікальності email
            if (await EmailExistsAsync(request.Email))
            {
                return new AuthResponse(false, "Користувач з такою електронною поштою вже існує");
            }

            // Створення нового користувача
            var user = new User
            {
                Username = request.Username.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                PasswordHash = HashPassword(request.Password),
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New user registered: {Username}", user.Username);

            // Генеруємо токени для нового користувача
            var tokens = GenerateTokens(user);

            // Оновлюємо час останнього входу
            await UpdateLastLoginAsync(user.Id);

            var userInfo = new UserInfo(
                user.Id,
                user.Username,
                user.Email,
                user.Role,
                user.CreatedAt,
                user.LastLoginAt
            );

            return new AuthResponse(
                true,
                "Користувач успішно зареєстрований",
                tokens.AccessToken,
                tokens.RefreshToken,
                tokens.ExpiresAt,
                userInfo
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return new AuthResponse(false, "Помилка при реєстрації користувача");
        }
    }

    /// <summary>
    ///     Виконує вхід користувача в систему.
    /// </summary>
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            // Валідація вхідних даних
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return new AuthResponse(false, "Ім'я користувача та пароль є обов'язковими");
            }

            // Пошук користувача за username або email
            var user = await GetUserByUsernameOrEmailAsync(request.Username);

            if (user == null)
            {
                _logger.LogWarning("Login attempt with non-existent user: {Username}", request.Username);
                return new AuthResponse(false, "Невірне ім'я користувача або пароль");
            }

            // Перевірка активності користувача
            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt with inactive user: {Username}", user.Username);
                return new AuthResponse(false, "Обліковий запис деактивований");
            }

            // Перевірка пароля
            if (!VerifyPassword(user, request.Password))
            {
                _logger.LogWarning("Failed login attempt for user: {Username}", user.Username);
                return new AuthResponse(false, "Невірне ім'я користувача або пароль");
            }

            // Генеруємо токени
            var tokens = GenerateTokens(user);

            // Оновлюємо час останнього входу
            await UpdateLastLoginAsync(user.Id);

            _logger.LogInformation("User logged in successfully: {Username}", user.Username);

            var userInfo = new UserInfo(
                user.Id,
                user.Username,
                user.Email,
                user.Role,
                user.CreatedAt,
                user.LastLoginAt
            );

            return new AuthResponse(
                true,
                "Успішний вхід в систему",
                tokens.AccessToken,
                tokens.RefreshToken,
                tokens.ExpiresAt,
                userInfo
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login");
            return new AuthResponse(false, "Помилка при вході в систему");
        }
    }

    /// <summary>
    ///     Оновлює токен доступу за допомогою токена оновлення.
    /// </summary>
    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return new AuthResponse(false, "Токен оновлення є обов'язковим");
            }

            // Валідуємо refresh token та отримуємо користувача
            var user = await ValidateRefreshTokenAsync(refreshToken);

            if (user == null)
            {
                return new AuthResponse(false, "Недійсний токен оновлення");
            }

            // Перевірка активності користувача
            if (!user.IsActive)
            {
                return new AuthResponse(false, "Обліковий запис деактивований");
            }

            // Генеруємо нові токени
            var tokens = GenerateTokens(user);

            _logger.LogInformation("Tokens refreshed for user: {Username}", user.Username);

            var userInfo = new UserInfo(
                user.Id,
                user.Username,
                user.Email,
                user.Role,
                user.CreatedAt,
                user.LastLoginAt
            );

            return new AuthResponse(
                true,
                "Токени успішно оновлені",
                tokens.AccessToken,
                tokens.RefreshToken,
                tokens.ExpiresAt,
                userInfo
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return new AuthResponse(false, "Помилка при оновленні токенів");
        }
    }

    /// <summary>
    ///     Отримує користувача за його ідентифікатором.
    /// </summary>
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
    }

    /// <summary>
    ///     Отримує користувача за ім'ям користувача.
    /// </summary>
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
    }

    /// <summary>
    ///     Отримує користувача за електронною поштою.
    /// </summary>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant() && u.IsActive);
    }

    /// <summary>
    ///     Отримує список всіх користувачів.
    /// </summary>
    public async Task<List<UserInfo>> GetAllUsersAsync()
    {
        return await _context.Users
            .Where(u => u.IsActive)
            .Select(u => new UserInfo(
                u.Id,
                u.Username,
                u.Email,
                u.Role,
                u.CreatedAt,
                u.LastLoginAt
            ))
            .ToListAsync();
    }

    /// <summary>
    ///     Перевіряє, чи існує користувач з таким ім'ям.
    /// </summary>
    public async Task<bool> UserExistsAsync(string username)
    {
        return await _context.Users
            .AnyAsync(u => u.Username == username);
    }

    /// <summary>
    ///     Перевіряє, чи існує користувач з такою електронною поштою.
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email.ToLowerInvariant());
    }

    /// <summary>
    ///     Оновлює час останнього входу користувача.
    /// </summary>
    public async Task UpdateLastLoginAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    ///     Перевіряє пароль користувача.
    /// </summary>
    public bool VerifyPassword(User user, string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    /// <summary>
    ///     Хешує пароль для безпечного зберігання.
    /// </summary>
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    ///     Отримує користувача за ім'ям користувача або електронною поштою.
    /// </summary>
    private async Task<User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
    {
        var normalizedInput = usernameOrEmail.Trim().ToLowerInvariant();

        return await _context.Users
            .FirstOrDefaultAsync(u =>
                (u.Username == usernameOrEmail || u.Email == normalizedInput) &&
                u.IsActive);
    }

    /// <summary>
    ///     Перевіряє валідність електронної пошти (базова перевірка).
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Генерує пару токенів (access та refresh) для користувача.
    /// </summary>
    private TokenResponse GenerateTokens(User user)
    {
        var accessToken = _tokenGenerator.GenerateAccessToken(user);
        var refreshToken = _tokenGenerator.GenerateRefreshToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);

        _logger.LogDebug("Generated tokens for user: {Username}", user.Username);

        return new TokenResponse(
            accessToken,
            refreshToken,
            expiresAt
        );
    }

    /// <summary>
    ///     Валідує токен оновлення та повертає користувача.
    /// </summary>
    private async Task<User?> ValidateRefreshTokenAsync(string refreshToken)
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
            var user = await GetUserByIdAsync(userId);

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
}
