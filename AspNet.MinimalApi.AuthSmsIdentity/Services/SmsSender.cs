using Microsoft.AspNetCore.Identity;

namespace AspNet.MinimalApi.AuthSmsIdentity.Services;

public interface ISmsSender
{
    Task SendSmsAsync(string number, string message);
}

public class SmsSender : ISmsSender
{
    private readonly ILogger<SmsSender> _logger;
    private readonly IConfiguration _configuration;

    public SmsSender(ILogger<SmsSender> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public Task SendSmsAsync(string number, string message)
    {
        // В development режимі логуємо SMS в консоль
        if (_configuration.GetValue<bool>("Development", true))
        {
            _logger.LogWarning("SMS to {PhoneNumber}: {Message}", number, message);
            return Task.CompletedTask;
        }

        // Тут буде реальна інтеграція з SMS провайдером
        // Наприклад, Twilio, AWS SNS, Azure Communication Services
        _logger.LogInformation("Sending SMS to {PhoneNumber}", number);
        
        // TODO: Реалізувати відправку через реальний провайдер
        // await twilioClient.SendSmsAsync(number, message);
        
        return Task.CompletedTask;
    }
}
