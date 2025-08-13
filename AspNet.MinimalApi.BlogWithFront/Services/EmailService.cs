using System.Net;
using System.Net.Mail;

namespace AspNet.MinimalApi.BlogWithFront.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendPasswordResetEmailAsync(string to, string resetToken);
    Task SendEmailConfirmationAsync(string to, string confirmationToken);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var smtpHost = smtpSettings["Host"];
            var smtpPort = int.Parse(smtpSettings["Port"] ?? "587");
            var smtpUsername = smtpSettings["Username"];
            var smtpPassword = smtpSettings["Password"];
            var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = enableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpUsername!, "Blog System"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {Email}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
            throw;
        }
    }

    public async Task SendPasswordResetEmailAsync(string to, string resetToken)
    {
        var resetUrl = $"http://localhost:5174/reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(to)}";
        
        var subject = "Відновлення пароля - Blog System";
        var body = $@"
            <html>
            <body>
                <h2>Відновлення пароля</h2>
                <p>Ви запросили відновлення пароля для вашого акаунту.</p>
                <p>Натисніть на посилання нижче, щоб скинути пароль:</p>
                <p><a href='{resetUrl}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Скинути пароль</a></p>
                <p>Або скопіюйте це посилання у браузер:</p>
                <p>{resetUrl}</p>
                <p>Це посилання дійсне протягом 1 години.</p>
                <p>Якщо ви не запрошували відновлення пароля, проігноруйте цей лист.</p>
                <br>
                <p>З повагою,<br>Команда Blog System</p>
            </body>
            </html>";

        await SendEmailAsync(to, subject, body);
    }

    public async Task SendEmailConfirmationAsync(string to, string confirmationToken)
    {
        var confirmUrl = $"http://localhost:5174/confirm-email?token={Uri.EscapeDataString(confirmationToken)}&email={Uri.EscapeDataString(to)}";
        
        var subject = "Підтвердження email - Blog System";
        var body = $@"
            <html>
            <body>
                <h2>Підтвердження email адреси</h2>
                <p>Дякуємо за реєстрацію в Blog System!</p>
                <p>Натисніть на посилання нижче, щоб підтвердити вашу email адресу:</p>
                <p><a href='{confirmUrl}' style='background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Підтвердити Email</a></p>
                <p>Або скопіюйте це посилання у браузер:</p>
                <p>{confirmUrl}</p>
                <p>Якщо ви не реєструвалися на нашому сайті, проігноруйте цей лист.</p>
                <br>
                <p>З повагою,<br>Команда Blog System</p>
            </body>
            </html>";

        await SendEmailAsync(to, subject, body);
    }
}
