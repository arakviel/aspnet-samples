using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace AspNet.MinimalApi.AuthSmsIdentity.Services;

public class TwilioSmsSender : ISmsSender
{
    private readonly ILogger<TwilioSmsSender> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _fromPhoneNumber;

    public TwilioSmsSender(ILogger<TwilioSmsSender> logger, IConfiguration configuration)
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
        
        _logger.LogInformation("TwilioSmsSender initialized with phone number: {FromPhoneNumber}", _fromPhoneNumber);
    }

    public async Task SendSmsAsync(string number, string message)
    {
        try
        {
            _logger.LogInformation("Sending SMS via Twilio to {PhoneNumber}", number);
            
            // Відправляємо SMS через Twilio
            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_fromPhoneNumber),
                to: new PhoneNumber(number)
            );

            // Перевіряємо статус відправки
            if (messageResource.Status == MessageResource.StatusEnum.Failed ||
                messageResource.Status == MessageResource.StatusEnum.Undelivered)
            {
                _logger.LogError("Failed to send SMS via Twilio. Status: {Status}, ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                    messageResource.Status, messageResource.ErrorCode, messageResource.ErrorMessage);
                throw new InvalidOperationException($"SMS sending failed: {messageResource.ErrorMessage}");
            }

            _logger.LogInformation("SMS sent successfully via Twilio. MessageSid: {MessageSid}, Status: {Status}",
                messageResource.Sid, messageResource.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending SMS via Twilio to {PhoneNumber}", number);
            throw;
        }
    }
}
