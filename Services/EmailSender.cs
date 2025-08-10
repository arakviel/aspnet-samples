using Microsoft.AspNetCore.Identity.UI.Services;

namespace Auth2Factors.Services;

/// <summary>
/// Демо реалізація IEmailSender для відправки email
/// В production використовуйте SendGrid, Mailgun, SMTP тощо
/// 
/// Identity автоматично використовує IEmailSender для:
/// - Підтвердження email
/// - Скидання паролю
/// - 2FA коди через email
/// </summary>
public class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(ILogger<EmailSender> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Відправка email (демо версія - виводить в консоль)
    /// </summary>
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // В демо режимі просто виводимо в консоль
        _logger.LogWarning("=== EMAIL SENT ===");
        _logger.LogWarning("To: {Email}", email);
        _logger.LogWarning("Subject: {Subject}", subject);
        _logger.LogWarning("Message: {Message}", htmlMessage);
        _logger.LogWarning("==================");

        // В production тут буде реальна відправка:
        // await _smtpClient.SendMailAsync(email, subject, htmlMessage);
        // або через SendGrid API, Mailgun тощо

        return Task.CompletedTask;
    }
}

/// <summary>
/// Розширення для генерації QR кодів для TOTP
/// Використовує Google Charts API для простоти
/// </summary>
public static class QrCodeHelper
{
    /// <summary>
    /// Генерує URL для QR коду через Google Charts API
    /// </summary>
    public static string GenerateQrCodeUri(string authenticatorUri)
    {
        // Використовуємо Google Charts API для генерації QR коду
        // В production краще використовувати локальну бібліотеку QR кодів
        var encodedUri = Uri.EscapeDataString(authenticatorUri);
        return $"https://chart.googleapis.com/chart?chs=200x200&chld=M|0&cht=qr&chl={encodedUri}";
    }
}
