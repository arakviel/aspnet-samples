using AspNet.MinimalApi.ExtendedJwtAuth.Data;
using AspNet.MinimalApi.ExtendedJwtAuth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspNet.MinimalApi.ExtendedJwtAuth.Services;

/// <summary>
///     Сервіс аутентифікації з використанням UserManager та JWT
/// </summary>
public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtService _jwtService;
    private readonly ILogger<AuthService> _logger;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        JwtService jwtService,
        ApplicationDbContext context,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    ///     Реєстрація нового користувача
    /// </summary>
    /// <param name="request">Дані для реєстрації</param>
    /// <returns>Результат реєстрації</returns>
    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // Перевіряємо, чи існує користувач з таким email
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Користувач з таким email вже існує",
                    Errors = new List<string> { "Email вже використовується" }
                };

            // Створюємо нового користувача
            var user = new User
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                CreatedAt = DateTime.UtcNow
            };

            // Створюємо користувача з паролем
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Помилка при створенні користувача",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };

            _logger.LogInformation("Користувач {Email} успішно зареєстрований", request.Email);

            // Генеруємо токени
            var authResponse = await GenerateAuthResponseAsync(user);

            return new ApiResponse<AuthResponse>
            {
                Success = true,
                Message = "Користувач успішно зареєстрований",
                Data = authResponse
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при реєстрації користувача {Email}", request.Email);
            return new ApiResponse<AuthResponse>
            {
                Success = false,
                Message = "Внутрішня помилка сервера",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    /// <summary>
    ///     Вхід в систему
    /// </summary>
    /// <param name="request">Дані для входу</param>
    /// <returns>Результат входу</returns>
    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            // Знаходимо користувача за email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Невірний email або пароль",
                    Errors = new List<string> { "Користувача не знайдено" }
                };

            // Перевіряємо пароль
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);

            if (!result.Succeeded)
            {
                var message = result.IsLockedOut ? "Акаунт заблокований" :
                    result.IsNotAllowed ? "Вхід не дозволений" :
                    "Невірний email або пароль";

                return new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = message,
                    Errors = new List<string> { message }
                };
            }

            // Оновлюємо час останнього входу
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Користувач {Email} успішно увійшов в систему", request.Email);

            // Генеруємо токени
            var authResponse = await GenerateAuthResponseAsync(user);

            return new ApiResponse<AuthResponse>
            {
                Success = true,
                Message = "Успішний вхід в систему",
                Data = authResponse
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при вході користувача {Email}", request.Email);
            return new ApiResponse<AuthResponse>
            {
                Success = false,
                Message = "Внутрішня помилка сервера",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    /// <summary>
    ///     Оновлення токена за допомогою refresh токена
    /// </summary>
    /// <param name="request">Запит з refresh токеном</param>
    /// <returns>Новий набір токенів</returns>
    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        try
        {
            // Знаходимо refresh токен в базі даних
            var refreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (refreshToken == null || !refreshToken.IsActive)
                return new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Message = "Невірний або застарілий refresh токен",
                    Errors = new List<string> { "Токен недійсний" }
                };

            // Відкликаємо старий токен
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;

            // Генеруємо нові токени
            var authResponse = await GenerateAuthResponseAsync(refreshToken.User);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Токен оновлено для користувача {UserId}", refreshToken.User.Id);

            return new ApiResponse<AuthResponse>
            {
                Success = true,
                Message = "Токен успішно оновлено",
                Data = authResponse
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при оновленні токена");
            return new ApiResponse<AuthResponse>
            {
                Success = false,
                Message = "Внутрішня помилка сервера",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    /// <summary>
    ///     Вихід з системи (відкликання refresh токена)
    /// </summary>
    /// <param name="refreshToken">Refresh токен для відкликання</param>
    /// <returns>Результат операції</returns>
    public async Task<ApiResponse> LogoutAsync(string refreshToken)
    {
        try
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token != null)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Успішний вихід з системи"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при виході з системи");
            return new ApiResponse
            {
                Success = false,
                Message = "Помилка при виході з системи",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    /// <summary>
    ///     Генерує відповідь з токенами для користувача
    /// </summary>
    /// <param name="user">Користувач</param>
    /// <returns>Відповідь з токенами</returns>
    private async Task<AuthResponse> GenerateAuthResponseAsync(User user)
    {
        // Отримуємо ролі користувача
        var roles = await _userManager.GetRolesAsync(user);

        // Генеруємо access токен
        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Зберігаємо refresh токен в базі даних
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = _jwtService.GetRefreshTokenExpiration(),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = _jwtService.GetTokenExpiration(),
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            }
        };
    }
}