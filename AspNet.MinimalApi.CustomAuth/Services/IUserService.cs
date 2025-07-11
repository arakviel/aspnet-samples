using AspNet.MinimalApi.CustomAuth.Models;

namespace AspNet.MinimalApi.CustomAuth.Services;

/// <summary>
/// Інтерфейс сервісу для роботи з користувачами.
/// Визначає контракт для операцій аутентифікації та управління користувачами.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Асинхронно перевіряє облікові дані користувача.
    /// Виконує аутентифікацію користувача за email та паролем.
    /// </summary>
    /// <param name="email">Електронна пошта користувача</param>
    /// <param name="password">Пароль користувача</param>
    /// <returns>
    /// Task, що містить об'єкт User, якщо облікові дані правильні,
    /// або null, якщо аутентифікація не вдалася
    /// </returns>
    Task<User?> ValidateUserAsync(string email, string password);

    /// <summary>
    /// Асинхронно отримує користувача за його електронною поштою.
    /// Використовується для пошуку користувача в системі.
    /// </summary>
    /// <param name="email">Електронна пошта користувача</param>
    /// <returns>
    /// Task, що містить об'єкт User, якщо користувач знайдений,
    /// або null, якщо користувач не існує
    /// </returns>
    Task<User?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Асинхронно отримує користувача за його ідентифікатором.
    /// Використовується для швидкого пошуку користувача за ID.
    /// </summary>
    /// <param name="id">Унікальний ідентифікатор користувача</param>
    /// <returns>
    /// Task, що містить об'єкт User, якщо користувач знайдений,
    /// або null, якщо користувач не існує
    /// </returns>
    Task<User?> GetUserByIdAsync(int id);
}
