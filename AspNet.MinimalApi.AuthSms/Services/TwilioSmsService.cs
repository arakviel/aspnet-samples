using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace AspNet.MinimalApi.AuthSms.Services;

public class TwilioSmsService : ISmsService
{
    private readonly ILogger<TwilioSmsService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _fromPhoneNumber;

    public TwilioSmsService(ILogger<TwilioSmsService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        // Отримуємо конфігурацію Twilio
        var twilioSettings = _configuration.GetSection("TwilioSettings");
        _accountSid = twilioSettings["AccountSid"] ?? throw new InvalidOperationException("Twilio AccountSid not configured");
        _authToken = twilioSettings["AuthToken"] ?? throw new InvalidOperationException("Twilio AuthToken not configured");
        _fromPhoneNumber = twilioSettings["FromPhoneNumber"] ?? throw new InvalidOperationException("Twilio FromPhoneNumber not configured");
        
        // Ініціалізуємо Twilio клієнт
        TwilioClient.Init(_accountSid, _authToken);
        
        _logger.LogInformation("TwilioSmsService initialized with phone number: {FromPhoneNumber}", _fromPhoneNumber);
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            _logger.LogInformation("Sending SMS via Twilio to {PhoneNumber}", phoneNumber);
            
            // Відправляємо SMS через Twilio
            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_fromPhoneNumber),
                to: new PhoneNumber(phoneNumber)
            );

            // Перевіряємо статус відправки
            if (messageResource.Status == MessageResource.StatusEnum.Failed ||
                messageResource.Status == MessageResource.StatusEnum.Undelivered)
            {
                _logger.LogError("Failed to send SMS via Twilio. Status: {Status}, ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                    messageResource.Status, messageResource.ErrorCode, messageResource.ErrorMessage);
                return false;
            }

            _logger.LogInformation("SMS sent successfully via Twilio. MessageSid: {MessageSid}, Status: {Status}",
                messageResource.Sid, messageResource.Status);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending SMS via Twilio to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    public async Task<bool> SendVerificationCodeAsync(string phoneNumber, string code)
    {
        var appName = _configuration.GetValue<string>("ApplicationName", "AuthApp");
        var message = $"Your {appName} verification code is: {code}. This code will expire in 5 minutes. Do not share this code with anyone.";
        
        return await SendSmsAsync(phoneNumber, message);
    }
}
