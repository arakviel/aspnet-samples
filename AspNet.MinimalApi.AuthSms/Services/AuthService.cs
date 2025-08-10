using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AspNet.MinimalApi.AuthSms.Data;
using AspNet.MinimalApi.AuthSms.DTOs;
using AspNet.MinimalApi.AuthSms.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AspNet.MinimalApi.AuthSms.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ISmsService _smsService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ISmsService smsService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _context = context;
        _smsService = smsService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ApiResponse<string>> SendVerificationCodeAsync(string phoneNumber, SmsVerificationPurpose purpose)
    {
        try
        {
            // Очищуємо старі коди для цього номера
            await CleanupExpiredCodesAsync(phoneNumber);

            // Перевіряємо ліміт запитів (не більше 3 кодів за 10 хвилин)
            var recentCodes = await _context.SmsVerificationCodes
                .Where(c => c.PhoneNumber == phoneNumber &&
                           c.CreatedAt > DateTime.UtcNow.AddMinutes(-10))
                .CountAsync();

            if (recentCodes >= 3)
            {
                return new ApiResponse<string>(false, "Too many verification attempts. Please try again later.");
            }

            // Генеруємо новий код
            var code = GenerateVerificationCode();
            var expiresAt = DateTime.UtcNow.AddMinutes(5);

            var verificationCode = new SmsVerificationCode
            {
                PhoneNumber = phoneNumber,
                Code = code,
                ExpiresAt = expiresAt,
                Purpose = purpose
            };

            _context.SmsVerificationCodes.Add(verificationCode);
            await _context.SaveChangesAsync();

            // Відправляємо SMS
            var smsSent = await _smsService.SendVerificationCodeAsync(phoneNumber, code);

            if (!smsSent)
            {
                return new ApiResponse<string>(false, "Failed to send SMS. Please try again.");
            }

            _logger.LogInformation("Verification code sent to {PhoneNumber} for {Purpose}", phoneNumber, purpose);

            return new ApiResponse<string>(true, "Verification code sent successfully.", code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending verification code to {PhoneNumber}", phoneNumber);
            return new ApiResponse<string>(false, "An error occurred while sending verification code.");
        }
    }

    public async Task<AuthResponse> VerifyCodeAndRegisterAsync(RegisterRequest request)
    {
        try
        {
            // Перевіряємо код
            var isValidCode = await ValidateVerificationCodeAsync(request.PhoneNumber, request.Code, SmsVerificationPurpose.Registration);
            if (!isValidCode)
            {
                return new AuthResponse(false, "Invalid or expired verification code.");
            }

            // Перевіряємо чи користувач вже існує
            var existingUser = await _userManager.FindByNameAsync(request.PhoneNumber);
            if (existingUser != null)
            {
                return new AuthResponse(false, "User with this phone number already exists.");
            }

            // Створюємо нового користувача
            var user = new ApplicationUser
            {
                UserName = request.PhoneNumber,
                PhoneNumber = request.PhoneNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumberConfirmed = true,
                IsPhoneNumberConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthResponse(false, $"Failed to create user: {errors}");
            }

            // Позначаємо код як використаний
            await MarkCodeAsUsedAsync(request.PhoneNumber, request.Code);

            // Генеруємо токени
            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            // Оновлюємо час останнього входу
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var userInfo = new UserInfo(
                user.Id,
                user.PhoneNumber!,
                user.FirstName,
                user.LastName,
                user.CreatedAt,
                user.LastLoginAt
            );

            _logger.LogInformation("User registered successfully: {PhoneNumber}", request.PhoneNumber);

            return new AuthResponse(
                true,
                "Registration successful.",
                accessToken,
                refreshToken,
                DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes()),
                userInfo
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for {PhoneNumber}", request.PhoneNumber);
            return new AuthResponse(false, "An error occurred during registration.");
        }
    }

    public async Task<AuthResponse> VerifyCodeAndLoginAsync(LoginRequest request)
    {
        try
        {
            // Перевіряємо код
            var isValidCode = await ValidateVerificationCodeAsync(request.PhoneNumber, request.Code, SmsVerificationPurpose.Login);
            if (!isValidCode)
            {
                return new AuthResponse(false, "Invalid or expired verification code.");
            }

            // Знаходимо користувача
            var user = await _userManager.FindByNameAsync(request.PhoneNumber);
            if (user == null)
            {
                return new AuthResponse(false, "User not found. Please register first.");
            }

            // Позначаємо код як використаний
            await MarkCodeAsUsedAsync(request.PhoneNumber, request.Code);

            // Генеруємо токени
            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            // Оновлюємо час останнього входу
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var userInfo = new UserInfo(
                user.Id,
                user.PhoneNumber!,
                user.FirstName,
                user.LastName,
                user.CreatedAt,
                user.LastLoginAt
            );

            _logger.LogInformation("User logged in successfully: {PhoneNumber}", request.PhoneNumber);

            return new AuthResponse(
                true,
                "Login successful.",
                accessToken,
                refreshToken,
                DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes()),
                userInfo
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {PhoneNumber}", request.PhoneNumber);
            return new AuthResponse(false, "An error occurred during login.");
        }
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        // В реальному проекті тут буде логіка валідації refresh token
        // Наприклад, зберігання в базі даних або Redis
        await Task.CompletedTask;
        return new AuthResponse(false, "Refresh token functionality not implemented in this demo.");
    }

    public async Task<bool> RevokeTokenAsync(string userId)
    {
        // В реальному проекті тут буде логіка відкликання токенів
        // Наприклад, додавання в blacklist або видалення з Redis
        await Task.CompletedTask;
        return true;
    }

    public string GenerateJwtToken(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "AspNet.MinimalApi.AuthSms";
        var audience = jwtSettings["Audience"] ?? "AspNet.MinimalApi.AuthSms.Client";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber!),
            new Claim("firstName", user.FirstName ?? ""),
            new Claim("lastName", user.LastName ?? ""),
            new Claim("jti", Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes()),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    // Приватні допоміжні методи
    private async Task CleanupExpiredCodesAsync(string phoneNumber)
    {
        var expiredCodes = await _context.SmsVerificationCodes
            .Where(c => c.PhoneNumber == phoneNumber && c.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        if (expiredCodes.Any())
        {
            _context.SmsVerificationCodes.RemoveRange(expiredCodes);
            await _context.SaveChangesAsync();
        }
    }

    private async Task<bool> ValidateVerificationCodeAsync(string phoneNumber, string code, SmsVerificationPurpose purpose)
    {
        var verificationCode = await _context.SmsVerificationCodes
            .FirstOrDefaultAsync(c =>
                c.PhoneNumber == phoneNumber &&
                c.Code == code &&
                c.Purpose == purpose &&
                !c.IsUsed &&
                c.ExpiresAt > DateTime.UtcNow);

        return verificationCode != null;
    }

    private async Task MarkCodeAsUsedAsync(string phoneNumber, string code)
    {
        var verificationCode = await _context.SmsVerificationCodes
            .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber && c.Code == code && !c.IsUsed);

        if (verificationCode != null)
        {
            verificationCode.IsUsed = true;
            await _context.SaveChangesAsync();
        }
    }

    private string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private int GetJwtExpirationMinutes()
    {
        return _configuration.GetValue<int>("JwtSettings:ExpirationMinutes", 60);
    }
}
