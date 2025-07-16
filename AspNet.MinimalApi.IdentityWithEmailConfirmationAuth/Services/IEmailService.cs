namespace AspNet.MinimalApi.IdentityWithEmailConfirmationAuth.Services;

/// <summary>
/// Інтерфейс для сервісу відправки електронної пошти
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Відправляє email підтвердження
    /// </summary>
    /// <param name="email">Email адреса отримувача</param>
    /// <param name="confirmationLink">Посилання для підтвердження</param>
    /// <param name="userName">Ім'я користувача</param>
    /// <returns>Task</returns>
    Task SendEmailConfirmationAsync(string email, string confirmationLink, string? userName = null);
    
    /// <summary>
    /// Відправляє email для скидання пароля
    /// </summary>
    /// <param name="email">Email адреса отримувача</param>
    /// <param name="resetLink">Посилання для скидання пароля</param>
    /// <param name="userName">Ім'я користувача</param>
    /// <returns>Task</returns>
    Task SendPasswordResetAsync(string email, string resetLink, string? userName = null);
    
    /// <summary>
    /// Відправляє загальний email
    /// </summary>
    /// <param name="to">Email адреса отримувача</param>
    /// <param name="subject">Тема листа</param>
    /// <param name="htmlBody">HTML тіло листа</param>
    /// <param name="textBody">Текстове тіло листа (опціонально)</param>
    /// <returns>Task</returns>
    Task SendEmailAsync(string to, string subject, string htmlBody, string? textBody = null);
}
