using System.Text.RegularExpressions;
using AspNet.MinimalApi.IdentityWithEmailConfirmationAuth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace AspNet.MinimalApi.IdentityWithEmailConfirmationAuth.Services;

/// <summary>
///     Кастомний EmailSender для ASP.NET Core Identity
///     Цей клас автоматично викликається вбудованими ендпоінтами Identity (/register, /confirmEmail тощо)
///     при потребі відправки email підтвердження
///     Реалізує як IEmailSender, так і IEmailSender<User> для повної сумісності
/// </summary>
public class IdentityEmailSender : IEmailSender, IEmailSender<User>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<IdentityEmailSender> _logger;

    public IdentityEmailSender(IEmailService emailService, ILogger<IdentityEmailSender> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    // === МЕТОДИ ДЛЯ IEmailSender ===

    /// <summary>
    ///     Відправляє загальний email (IEmailSender)
    ///     Викликається як fallback для старих версій або загальних випадків
    /// </summary>
    /// <param name="email">Email адреса отримувача</param>
    /// <param name="subject">Тема листа</param>
    /// <param name="htmlMessage">HTML повідомлення</param>
    /// <returns>Task</returns>
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            _logger.LogInformation("📬 Відправка загального email на {Email} з темою '{Subject}'", email, subject);

            // Перевіряємо, чи це email підтвердження (за темою або вмістом)
            if (subject.Contains("Confirm") || htmlMessage.Contains("confirm") || htmlMessage.Contains("Confirm"))
            {
                _logger.LogInformation("📧 Виявлено email підтвердження - витягуємо посилання");

                // Витягуємо посилання підтвердження з HTML повідомлення
                var confirmationLink = ExtractConfirmationLink(htmlMessage);

                if (!string.IsNullOrEmpty(confirmationLink))
                {
                    // Використовуємо наш красивий шаблон для email підтвердження
                    await _emailService.SendEmailConfirmationAsync(email, confirmationLink);
                    _logger.LogInformation("✅ Email підтвердження відправлено з кастомним шаблоном");
                    return;
                }
            }

            // Для всіх інших випадків використовуємо стандартний метод
            await _emailService.SendEmailAsync(email, subject, htmlMessage);

            _logger.LogInformation("✅ Загальний email успішно відправлено на {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Помилка при відправці загального email на {Email}", email);
            throw;
        }
    }

    // === МЕТОДИ ДЛЯ IEmailSender<User> ===

    /// <summary>
    ///     Відправляє email підтвердження для конкретного користувача (IEmailSender
    ///     <User>
    ///         )
    ///         Викликається автоматично при POST /register та POST /resendConfirmationEmail
    /// </summary>
    /// <param name="user">Користувач, для якого відправляється підтвердження</param>
    /// <param name="email">Email адреса отримувача</param>
    /// <param name="confirmationLink">Посилання для підтвердження</param>
    /// <returns>Task</returns>
    public async Task SendConfirmationLinkAsync(User user, string email, string confirmationLink)
    {
        try
        {
            _logger.LogInformation("📧 Відправка email підтвердження для користувача {UserId} ({FullName}) на {Email}",
                user.Id, user.FullName, email);

            // Використовуємо наш красивий шаблон для email підтвердження
            await _emailService.SendEmailConfirmationAsync(email, confirmationLink, user.FullName);

            _logger.LogInformation("✅ Email підтвердження успішно відправлено для користувача {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Помилка при відправці email підтвердження для користувача {UserId}", user.Id);
            throw;
        }
    }

    /// <summary>
    ///     Відправляє email для скидання пароля для конкретного користувача (IEmailSender
    ///     <User>
    ///         )
    ///         Викликається автоматично при POST /forgotPassword
    /// </summary>
    /// <param name="user">Користувач, для якого відправляється скидання пароля</param>
    /// <param name="email">Email адреса отримувача</param>
    /// <param name="resetLink">Посилання для скидання пароля</param>
    /// <returns>Task</returns>
    public async Task SendPasswordResetLinkAsync(User user, string email, string resetLink)
    {
        try
        {
            _logger.LogInformation(
                "🔑 Відправка email скидання пароля для користувача {UserId} ({FullName}) на {Email}",
                user.Id, user.FullName, email);

            // Використовуємо наш красивий шаблон для скидання пароля
            await _emailService.SendPasswordResetAsync(email, resetLink, user.FullName);

            _logger.LogInformation("✅ Email скидання пароля успішно відправлено для користувача {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Помилка при відправці email скидання пароля для користувача {UserId}", user.Id);
            throw;
        }
    }

    /// <summary>
    ///     Відправляє код для скидання пароля для конкретного користувача (IEmailSender
    ///     <User>
    ///         )
    ///         Викликається автоматично при POST /resetPassword з кодом
    /// </summary>
    /// <param name="user">Користувач, для якого відправляється код</param>
    /// <param name="email">Email адреса отримувача</param>
    /// <param name="resetCode">Код для скидання пароля</param>
    /// <returns>Task</returns>
    public async Task SendPasswordResetCodeAsync(User user, string email, string resetCode)
    {
        try
        {
            _logger.LogInformation("🔢 Відправка коду скидання пароля для користувача {UserId} ({FullName}) на {Email}",
                user.Id, user.FullName, email);

            var subject = "Код скидання пароля";
            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #333;'>Код скидання пароля</h2>
                    <p>Привіт, {user.FullName}!</p>
                    <p>Ваш код для скидання пароля:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <span style='background-color: #f8f9fa; padding: 15px 25px; font-size: 24px; font-weight: bold; border: 2px solid #dee2e6; border-radius: 5px; display: inline-block; letter-spacing: 3px;'>
                            {resetCode}
                        </span>
                    </div>
                    <p style='color: #666; font-size: 14px;'>
                        Введіть цей код в додатку для скидання пароля.
                    </p>
                    <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                        Якщо ви не запитували скидання пароля, просто проігноруйте цей лист.
                    </p>
                </div>";

            await _emailService.SendEmailAsync(email, subject, htmlBody);

            _logger.LogInformation("✅ Код скидання пароля успішно відправлено для користувача {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Помилка при відправці коду скидання пароля для користувача {UserId}", user.Id);
            throw;
        }
    }

    /// <summary>
    ///     Витягує посилання підтвердження з HTML повідомлення
    /// </summary>
    private static string ExtractConfirmationLink(string htmlMessage)
    {
        try
        {
            // Шукаємо посилання в HTML (простий regex для демонстрації)
            var match = Regex.Match(htmlMessage, @"href=[""']([^""']*confirmEmail[^""']*)[""']");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}