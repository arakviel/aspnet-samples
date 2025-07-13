using AspNet.MinimalApi.CustomJwtAuth.Models;

namespace AspNet.MinimalApi.CustomJwtAuth.Services;

/// <summary>
/// Інтерфейс сервісу для роботи з користувачами.
/// Визначає основні операції управління користувачами в системі аутентифікації.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Реєструє нового користувача в системі.
    /// </summary>
    /// <param name="request">Дані для реєстрації користувача</param>
    /// <returns>Результат операції реєстрації</returns>
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Виконує вхід користувача в систему.
    /// </summary>
    /// <param name="request">Дані для входу користувача</param>
    /// <returns>Результат операції входу з токенами</returns>
    Task<AuthResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Оновлює токен доступу за допомогою токена оновлення.
    /// </summary>
    /// <param name="refreshToken">Токен оновлення</param>
    /// <returns>Новий токен доступу або помилка</returns>
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Отримує користувача за його ідентифікатором.
    /// </summary>
    /// <param name="userId">Ідентифікатор користувача</param>
    /// <returns>Користувач або null, якщо не знайдено</returns>
    Task<User?> GetUserByIdAsync(int userId);

    /// <summary>
    /// Отримує користувача за ім'ям користувача.
    /// </summary>
    /// <param name="username">Ім'я користувача</param>
    /// <returns>Користувач або null, якщо не знайдено</returns>
    Task<User?> GetUserByUsernameAsync(string username);

    /// <summary>
    /// Отримує користувача за електронною поштою.
    /// </summary>
    /// <param name="email">Електронна пошта</param>
    /// <returns>Користувач або null, якщо не знайдено</returns>
    Task<User?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Отримує список всіх користувачів (для адміністративних цілей).
    /// </summary>
    /// <returns>Список користувачів без чутливих даних</returns>
    Task<List<UserInfo>> GetAllUsersAsync();

    /// <summary>
    /// Перевіряє, чи існує користувач з таким ім'ям.
    /// </summary>
    /// <param name="username">Ім'я користувача для перевірки</param>
    /// <returns>true, якщо користувач існує</returns>
    Task<bool> UserExistsAsync(string username);

    /// <summary>
    /// Перевіряє, чи існує користувач з такою електронною поштою.
    /// </summary>
    /// <param name="email">Електронна пошта для перевірки</param>
    /// <returns>true, якщо користувач існує</returns>
    Task<bool> EmailExistsAsync(string email);

    /// <summary>
    /// Оновлює час останнього входу користувача.
    /// </summary>
    /// <param name="userId">Ідентифікатор користувача</param>
    /// <returns>Task для асинхронної операції</returns>
    Task UpdateLastLoginAsync(int userId);

    /// <summary>
    /// Перевіряє пароль користувача.
    /// </summary>
    /// <param name="user">Користувач</param>
    /// <param name="password">Пароль для перевірки</param>
    /// <returns>true, якщо пароль правильний</returns>
    bool VerifyPassword(User user, string password);

    /// <summary>
    /// Хешує пароль для безпечного зберігання.
    /// </summary>
    /// <param name="password">Пароль для хешування</param>
    /// <returns>Хеш пароля</returns>
    string HashPassword(string password);
}
