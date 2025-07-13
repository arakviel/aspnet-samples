namespace AspNet.MinimalApi.CustomJwtAuth.Models;

/// <summary>
/// Модель користувача для системи аутентифікації.
/// Містить основну інформацію про користувача та його облікові дані.
/// </summary>
public class User
{
    /// <summary>
    /// Унікальний ідентифікатор користувача.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Ім'я користувача (логін). Має бути унікальним.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Хеш пароля користувача. Зберігається в зашифрованому вигляді за допомогою BCrypt.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Електронна пошта користувача. Має бути унікальною.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Роль користувача в системі (наприклад, "User", "Admin").
    /// </summary>
    public string Role { get; set; } = "User";

    /// <summary>
    /// Дата та час створення облікового запису користувача.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата та час останнього входу користувача в систему.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Прапорець, що вказує, чи активний обліковий запис користувача.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
