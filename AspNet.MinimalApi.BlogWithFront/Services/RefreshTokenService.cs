using System.Security.Cryptography;
using AspNet.MinimalApi.BlogWithFront.Data;
using AspNet.MinimalApi.BlogWithFront.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNet.MinimalApi.BlogWithFront.Services;

/// <summary>
/// Сервіс для роботи з рефреш токенами
/// </summary>
public class RefreshTokenService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public RefreshTokenService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Генерує новий рефреш токен для користувача
    /// </summary>
    /// <param name="userId">Ідентифікатор користувача</param>
    /// <returns>Новий рефреш токен</returns>
    public async Task<RefreshToken> GenerateRefreshTokenAsync(string userId)
    {
        // Генеруємо криптографічно стійкий токен
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var token = Convert.ToBase64String(randomBytes);

        // Отримуємо час життя токена з конфігурації (за замовчуванням 7 днів)
        var refreshTokenLifetimeDays = _configuration.GetValue<int>("Jwt:RefreshTokenLifetimeDays", 7);

        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = userId,
            CreatedDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(refreshTokenLifetimeDays)
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    /// <summary>
    /// Перевіряє валідність рефреш токена
    /// </summary>
    /// <param name="token">Токен для перевірки</param>
    /// <returns>Рефреш токен, якщо він валідний, інакше null</returns>
    public async Task<RefreshToken?> ValidateRefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            return null;
        }

        return refreshToken;
    }

    /// <summary>
    /// Відкликає рефреш токен
    /// </summary>
    /// <param name="token">Токен для відкликання</param>
    /// <returns>True, якщо токен успішно відкликано</returns>
    public async Task<bool> RevokeRefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null)
        {
            return false;
        }

        refreshToken.IsRevoked = true;
        refreshToken.RevokedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Відкликає всі рефреш токени користувача
    /// </summary>
    /// <param name="userId">Ідентифікатор користувача</param>
    /// <returns>Кількість відкликаних токенів</returns>
    public async Task<int> RevokeAllUserRefreshTokensAsync(string userId)
    {
        var userTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in userTokens)
        {
            token.IsRevoked = true;
            token.RevokedDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return userTokens.Count;
    }

    /// <summary>
    /// Очищає застарілі рефреш токени
    /// </summary>
    /// <returns>Кількість видалених токенів</returns>
    public async Task<int> CleanupExpiredTokensAsync()
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiryDate < DateTime.UtcNow || rt.IsRevoked)
            .ToListAsync();

        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();

        return expiredTokens.Count;
    }

    /// <summary>
    /// Замінює старий рефреш токен на новий
    /// </summary>
    /// <param name="oldToken">Старий токен</param>
    /// <returns>Новий рефреш токен або null, якщо старий токен недійсний</returns>
    public async Task<RefreshToken?> RotateRefreshTokenAsync(string oldToken)
    {
        var oldRefreshToken = await ValidateRefreshTokenAsync(oldToken);
        if (oldRefreshToken == null)
        {
            return null;
        }

        // Відкликаємо старий токен
        await RevokeRefreshTokenAsync(oldToken);

        // Генеруємо новий токен
        return await GenerateRefreshTokenAsync(oldRefreshToken.UserId);
    }
}
