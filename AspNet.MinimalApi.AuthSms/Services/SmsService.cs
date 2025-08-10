namespace AspNet.MinimalApi.AuthSms.Services;

public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;
    private readonly IConfiguration _configuration;

    public SmsService(ILogger<SmsService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            // В реальному проекті тут буде інтеграція з SMS провайдером
            // Наприклад: Twilio, AWS SNS, Azure Communication Services, тощо
            
            _logger.LogInformation("Sending SMS to {PhoneNumber}: {Message}", phoneNumber, message);
            
            // Симуляція відправки SMS
            await Task.Delay(100);
            
            // В development режимі логуємо код для тестування
            if (_configuration.GetValue<bool>("Development"))
            {
                _logger.LogWarning("SMS Code for {PhoneNumber}: {Message}", phoneNumber, message);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    public async Task<bool> SendVerificationCodeAsync(string phoneNumber, string code)
    {
        var message = $"Your verification code is: {code}. This code will expire in 5 minutes.";
        return await SendSmsAsync(phoneNumber, message);
    }
}
