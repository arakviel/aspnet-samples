using Microsoft.AspNetCore.Identity;

namespace AspNet.MinimalApi.IdentityWithEmailConfirmationAuth.Models;

/// <summary>
/// Модель користувача, що розширює стандартну модель IdentityUser
/// 
/// IdentityUser вже містить:
/// - Id (string) - унікальний ідентифікатор
/// - UserName (string) - ім'я користувача
/// - Email (string) - електронна пошта
/// - PasswordHash (string) - хеш пароля
/// - PhoneNumber (string) - номер телефону
/// - EmailConfirmed (bool) - чи підтверджена пошта
/// - PhoneNumberConfirmed (bool) - чи підтверджений телефон
/// - TwoFactorEnabled (bool) - чи увімкнена двофакторна аутентифікація
/// - LockoutEnd (DateTimeOffset?) - час закінчення блокування
/// - LockoutEnabled (bool) - чи можна блокувати користувача
/// - AccessFailedCount (int) - кількість невдалих спроб входу
/// </summary>
public class User : IdentityUser
{
    /// <summary>
    /// Ім'я користувача (додаткове поле)
    /// </summary>
    public string? FirstName { get; set; }
    
    /// <summary>
    /// Прізвище користувача (додаткове поле)
    /// </summary>
    public string? LastName { get; set; }
    
    /// <summary>
    /// Дата створення акаунту
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Дата останнього входу
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
    
    /// <summary>
    /// Дата підтвердження email
    /// </summary>
    public DateTime? EmailConfirmedAt { get; set; }
    
    /// <summary>
    /// Повне ім'я користувача (обчислювана властивість)
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();
}
