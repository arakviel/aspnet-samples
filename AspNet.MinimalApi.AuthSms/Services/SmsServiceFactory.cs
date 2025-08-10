using AspNet.MinimalApi.AuthSms.Configuration;

namespace AspNet.MinimalApi.AuthSms.Services;

public interface ISmsServiceFactory
{
    ISmsService CreateSmsService();
}

public class SmsServiceFactory : ISmsServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmsServiceFactory> _logger;

    public SmsServiceFactory(
        IServiceProvider serviceProvider, 
        IConfiguration configuration,
        ILogger<SmsServiceFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public ISmsService CreateSmsService()
    {
        var smsSettings = _configuration.GetSection(SmsSettings.SectionName).Get<SmsSettings>() ?? new SmsSettings();
        
        _logger.LogInformation("Creating SMS service with provider: {Provider}", smsSettings.Provider);

        return smsSettings.Provider.ToLowerInvariant() switch
        {
            "twilio" => _serviceProvider.GetRequiredService<TwilioSmsService>(),
            "demo" => _serviceProvider.GetRequiredService<SmsService>(),
            _ => throw new InvalidOperationException($"Unknown SMS provider: {smsSettings.Provider}")
        };
    }
}
