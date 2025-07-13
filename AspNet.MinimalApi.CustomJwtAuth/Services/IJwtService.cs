using System.Security.Claims;
using AspNet.MinimalApi.CustomJwtAuth.Models;

namespace AspNet.MinimalApi.CustomJwtAuth.Services;

/// <summary>
/// Інтерфейс сервісу для роботи з JWT токенами.
/// Визначає операції генерації, валідації та управління JWT токенами.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Генерує пару токенів (access та refresh) для користувача.
    /// </summary>
    /// <param name="user">Користувач, для якого генеруються токени</param>
    /// <returns>Пара токенів з інформацією про термін дії</returns>
    TokenResponse GenerateTokens(User user);

    /// <summary>
    /// Генерує токен доступу для користувача.
    /// </summary>
    /// <param name="user">Користувач, для якого генерується токен</param>
    /// <returns>JWT токен доступу</returns>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Генерує токен оновлення для користувача.
    /// </summary>
    /// <param name="user">Користувач, для якого генерується токен</param>
    /// <returns>JWT токен оновлення</returns>
    string GenerateRefreshToken(User user);

    /// <summary>
    /// Валідує JWT токен та повертає claims.
    /// </summary>
    /// <param name="token">JWT токен для валідації</param>
    /// <returns>Claims з токена або null, якщо токен недійсний</returns>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Валідує токен оновлення та повертає користувача.
    /// </summary>
    /// <param name="refreshToken">Токен оновлення для валідації</param>
    /// <returns>Користувач або null, якщо токен недійсний</returns>
    Task<User?> ValidateRefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Отримує ідентифікатор користувача з токена.
    /// </summary>
    /// <param name="token">JWT токен</param>
    /// <returns>Ідентифікатор користувача або null</returns>
    int? GetUserIdFromToken(string token);

    /// <summary>
    /// Перевіряє, чи є токен токеном доступу.
    /// </summary>
    /// <param name="token">JWT токен для перевірки</param>
    /// <returns>true, якщо це токен доступу</returns>
    bool IsAccessToken(string token);

    /// <summary>
    /// Перевіряє, чи є токен токеном оновлення.
    /// </summary>
    /// <param name="token">JWT токен для перевірки</param>
    /// <returns>true, якщо це токен оновлення</returns>
    bool IsRefreshToken(string token);

    /// <summary>
    /// Отримує час закінчення дії токена.
    /// </summary>
    /// <param name="token">JWT токен</param>
    /// <returns>Час закінчення дії або null</returns>
    DateTime? GetTokenExpiration(string token);

    /// <summary>
    /// Перевіряє, чи не прострочений токен.
    /// </summary>
    /// <param name="token">JWT токен для перевірки</param>
    /// <returns>true, якщо токен ще дійсний</returns>
    bool IsTokenValid(string token);
}
