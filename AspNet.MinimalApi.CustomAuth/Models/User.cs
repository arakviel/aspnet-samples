using System.ComponentModel.DataAnnotations;

namespace AspNet.MinimalApi.CustomAuth.Models;

/// <summary>
/// Модель користувача для системи аутентифікації.
/// Представляє основну інформацію про користувача, що зберігається в базі даних.
/// </summary>
public class User
{
    /// <summary>
    /// Унікальний ідентифікатор користувача.
    /// Використовується як первинний ключ в базі даних.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Електронна пошта користувача.
    /// Використовується як унікальний логін для входу в систему.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Пароль користувача.
    /// В реальному додатку має бути захешований (наприклад, за допомогою BCrypt).
    /// Для демонстрації зберігається у відкритому вигляді.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Дата та час створення облікового запису користувача.
    /// Автоматично встановлюється при створенні нового користувача.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Конструктор за замовчуванням.
    /// Необхідний для Entity Framework Core.
    /// </summary>
    public User()
    {
    }

    /// <summary>
    /// Конструктор для створення користувача з email та паролем.
    /// Зручний для швидкого створення користувачів у тестах або seed даних.
    /// </summary>
    /// <param name="email">Електронна пошта користувача</param>
    /// <param name="password">Пароль користувача</param>
    public User(string email, string password)
    {
        Email = email;
        Password = password;
    }
}
