using AspNet.MinimalApi.IdentityWithEmailConfirmationAuth.Models;
using Microsoft.AspNetCore.Identity;

namespace AspNet.MinimalApi.IdentityWithEmailConfirmationAuth.Services;

/// <summary>
/// Сервіс для обробки подій Identity (реєстрація, підтвердження email тощо)
/// </summary>
public class IdentityEventService
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<IdentityEventService> _logger;
    private readonly IConfiguration _configuration;

    public IdentityEventService(
        UserManager<User> userManager,
        IEmailService emailService,
        ILogger<IdentityEventService> logger,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Обробляє подію успішної реєстрації користувача
    /// </summary>
    /// <param name="user">Зареєстрований користувач</param>
    /// <returns>Task</returns>
    public async Task HandleUserRegisteredAsync(User user)
    {
        try
        {
            _logger.LogInformation("Обробка реєстрації користувача {Email}", user.Email);

            // Генеруємо токен підтвердження email
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            // Отримуємо базовий URL з конфігурації або використовуємо за замовчуванням
            var baseUrl = _configuration["BaseUrl"] ?? "https://localhost:7084";
            
            // Створюємо посилання для підтвердження
            var confirmationLink = $"{baseUrl}/confirmEmail?userId={user.Id}&code={Uri.EscapeDataString(token)}";
            
            // Відправляємо email підтвердження
            await _emailService.SendEmailConfirmationAsync(user.Email!, confirmationLink, user.FullName);
            
            _logger.LogInformation("Email підтвердження відправлено на {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при відправці email підтвердження для користувача {Email}", user.Email);
            // Не кидаємо виняток, щоб не перервати процес реєстрації
        }
    }

    /// <summary>
    /// Обробляє подію підтвердження email
    /// </summary>
    /// <param name="user">Користувач, який підтвердив email</param>
    /// <returns>Task</returns>
    public async Task HandleEmailConfirmedAsync(User user)
    {
        try
        {
            _logger.LogInformation("Email підтверджено для користувача {Email}", user.Email);

            // Оновлюємо дату підтвердження email
            user.EmailConfirmedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            
            _logger.LogInformation("Дата підтвердження email оновлена для користувача {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при оновленні дати підтвердження email для користувача {Email}", user.Email);
        }
    }

    /// <summary>
    /// Обробляє подію скидання пароля
    /// </summary>
    /// <param name="user">Користувач, який запросив скидання пароля</param>
    /// <returns>Task</returns>
    public async Task HandlePasswordResetRequestedAsync(User user)
    {
        try
        {
            _logger.LogInformation("Запит на скидання пароля для користувача {Email}", user.Email);

            // Генеруємо токен скидання пароля
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // Отримуємо базовий URL з конфігурації
            var baseUrl = _configuration["BaseUrl"] ?? "https://localhost:7084";
            
            // Створюємо посилання для скидання пароля
            var resetLink = $"{baseUrl}/resetPassword?email={Uri.EscapeDataString(user.Email!)}&code={Uri.EscapeDataString(token)}";
            
            // Відправляємо email для скидання пароля
            await _emailService.SendPasswordResetAsync(user.Email!, resetLink, user.FullName);
            
            _logger.LogInformation("Email для скидання пароля відправлено на {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при відправці email для скидання пароля для користувача {Email}", user.Email);
        }
    }
}
