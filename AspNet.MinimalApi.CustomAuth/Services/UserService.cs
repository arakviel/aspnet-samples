using AspNet.MinimalApi.CustomAuth.Data;
using AspNet.MinimalApi.CustomAuth.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNet.MinimalApi.CustomAuth.Services;

/// <summary>
/// Реалізація сервісу для роботи з користувачами.
/// Надає методи для аутентифікації та управління користувачами через Entity Framework.
/// </summary>
public class UserService : IUserService
{
    private readonly AuthDbContext _context;

    /// <summary>
    /// Конструктор сервісу користувачів.
    /// </summary>
    /// <param name="context">Контекст бази даних для доступу до користувачів</param>
    public UserService(AuthDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Асинхронно перевіряє облікові дані користувача.
    /// Виконує аутентифікацію користувача за email та паролем.
    /// 
    /// УВАГА: В реальному додатку паролі мають бути захешовані!
    /// Цей приклад використовує відкриті паролі лише для демонстрації.
    /// </summary>
    /// <param name="email">Електронна пошта користувача</param>
    /// <param name="password">Пароль користувача</param>
    /// <returns>
    /// Task, що містить об'єкт User, якщо облікові дані правильні,
    /// або null, якщо аутентифікація не вдалася
    /// </returns>
    public async Task<User?> ValidateUserAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        // Пошук користувача за email та паролем
        // В реальному додатку тут має бути перевірка хешу пароля
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

        return user;
    }

    /// <summary>
    /// Асинхронно отримує користувача за його електронною поштою.
    /// Використовується для пошуку користувача в системі.
    /// </summary>
    /// <param name="email">Електронна пошта користувача</param>
    /// <returns>
    /// Task, що містить об'єкт User, якщо користувач знайдений,
    /// або null, якщо користувач не існує
    /// </returns>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <summary>
    /// Асинхронно отримує користувача за його ідентифікатором.
    /// Використовується для швидкого пошуку користувача за ID.
    /// </summary>
    /// <param name="id">Унікальний ідентифікатор користувача</param>
    /// <returns>
    /// Task, що містить об'єкт User, якщо користувач знайдений,
    /// або null, якщо користувач не існує
    /// </returns>
    public async Task<User?> GetUserByIdAsync(int id)
    {
        if (id <= 0)
        {
            return null;
        }

        return await _context.Users.FindAsync(id);
    }
}
