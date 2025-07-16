using Microsoft.AspNetCore.Identity;

namespace AspNet.MinimalApi.ExtendedJwtAuth.Models;

/// <summary>
///     Модель користувача, що розширює стандартну модель IdentityUser
///     IdentityUser вже містить:
///     - Id (string) - унікальний ідентифікатор
///     - UserName (string) - ім'я користувача
///     - Email (string) - електронна пошта
///     - PasswordHash (string) - хеш пароля
///     - PhoneNumber (string) - номер телефону
///     - EmailConfirmed (bool) - чи підтверджена пошта
///     - PhoneNumberConfirmed (bool) - чи підтверджений телефон
///     - TwoFactorEnabled (bool) - чи увімкнена двофакторна аутентифікація
///     - LockoutEnd (DateTimeOffset?) - час закінчення блокування
///     - LockoutEnabled (bool) - чи можна блокувати користувача
///     - AccessFailedCount (int) - кількість невдалих спроб входу
/// </summary>
public class User : IdentityUser
{
    /// <summary>
    ///     Ім'я користувача (додаткове поле)
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    ///     Прізвище користувача (додаткове поле)
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    ///     Дата створення акаунту
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Дата останнього входу
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    ///     Повне ім'я користувача (обчислювана властивість)
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    ///     Refresh токени користувача
    /// </summary>
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}

/// <summary>
///     Модель для зберігання refresh токенів
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; }
    public string? RevokedBy { get; set; }
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    ///     ID користувача, якому належить токен
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    ///     Навігаційна властивість до користувача
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    ///     Чи активний токен
    /// </summary>
    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;
}