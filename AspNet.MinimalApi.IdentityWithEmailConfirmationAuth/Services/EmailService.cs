using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace AspNet.MinimalApi.IdentityWithEmailConfirmationAuth.Services;

/// <summary>
/// Сервіс для відправки електронної пошти через SMTP
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Відправляє email підтвердження
    /// </summary>
    public async Task SendEmailConfirmationAsync(string email, string confirmationLink, string? userName = null)
    {
        var subject = "Підтвердження електронної пошти";
        var displayName = !string.IsNullOrEmpty(userName) ? userName : email;
        
        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #333;'>Підтвердження електронної пошти</h2>
                <p>Привіт, {displayName}!</p>
                <p>Дякуємо за реєстрацію в нашому додатку. Для завершення реєстрації, будь ласка, підтвердіть свою електронну пошту.</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{confirmationLink}' 
                       style='background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                        Підтвердити Email
                    </a>
                </div>
                <p style='color: #666; font-size: 14px;'>
                    Якщо кнопка не працює, скопіюйте та вставте це посилання в браузер:<br>
                    <a href='{confirmationLink}'>{confirmationLink}</a>
                </p>
                <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                    Якщо ви не реєструвалися в нашому додатку, просто проігноруйте цей лист.
                </p>
            </div>";

        var textBody = $@"
            Підтвердження електронної пошти
            
            Привіт, {displayName}!
            
            Дякуємо за реєстрацію в нашому додатку. Для завершення реєстрації, будь ласка, підтвердіть свою електронну пошту.
            
            Перейдіть за посиланням: {confirmationLink}
            
            Якщо ви не реєструвалися в нашому додатку, просто проігноруйте цей лист.";

        await SendEmailAsync(email, subject, htmlBody, textBody);
    }

    /// <summary>
    /// Відправляє email для скидання пароля
    /// </summary>
    public async Task SendPasswordResetAsync(string email, string resetLink, string? userName = null)
    {
        var subject = "Скидання пароля";
        var displayName = !string.IsNullOrEmpty(userName) ? userName : email;
        
        var htmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #333;'>Скидання пароля</h2>
                <p>Привіт, {displayName}!</p>
                <p>Ви запросили скидання пароля для вашого акаунту.</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{resetLink}' 
                       style='background-color: #dc3545; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                        Скинути пароль
                    </a>
                </div>
                <p style='color: #666; font-size: 14px;'>
                    Якщо кнопка не працює, скопіюйте та вставте це посилання в браузер:<br>
                    <a href='{resetLink}'>{resetLink}</a>
                </p>
                <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                    Якщо ви не запитували скидання пароля, просто проігноруйте цей лист.
                </p>
            </div>";

        var textBody = $@"
            Скидання пароля
            
            Привіт, {displayName}!
            
            Ви запросили скидання пароля для вашого акаунту.
            
            Перейдіть за посиланням: {resetLink}
            
            Якщо ви не запитували скидання пароля, просто проігноруйте цей лист.";

        await SendEmailAsync(email, subject, htmlBody, textBody);
    }

    /// <summary>
    /// Відправляє загальний email
    /// </summary>
    public async Task SendEmailAsync(string to, string subject, string htmlBody, string? textBody = null)
    {
        try
        {
            var email = new MimeMessage();
            
            // Налаштування відправника
            var fromName = _configuration["EmailSettings:FromName"] ?? "Identity App";
            var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@example.com";
            email.From.Add(new MailboxAddress(fromName, fromEmail));
            
            // Налаштування отримувача
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            // Створення тіла листа
            var bodyBuilder = new BodyBuilder();
            
            if (!string.IsNullOrEmpty(textBody))
            {
                bodyBuilder.TextBody = textBody;
            }
            
            bodyBuilder.HtmlBody = htmlBody;
            email.Body = bodyBuilder.ToMessageBody();

            // Відправка через SMTP
            using var smtp = new SmtpClient();
            
            var host = _configuration["EmailSettings:SmtpHost"] ?? "localhost";
            var port = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var username = _configuration["EmailSettings:SmtpUsername"];
            var password = _configuration["EmailSettings:SmtpPassword"];
            var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

            await smtp.ConnectAsync(host, port, enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
            
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                await smtp.AuthenticateAsync(username, password);
            }
            
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
            
            _logger.LogInformation("Email успішно відправлено на {Email}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при відправці email на {Email}", to);
            throw;
        }
    }
}
