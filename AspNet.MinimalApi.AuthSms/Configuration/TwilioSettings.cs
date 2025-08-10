namespace AspNet.MinimalApi.AuthSms.Configuration;

public class TwilioSettings
{
    public const string SectionName = "TwilioSettings";
    
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromPhoneNumber { get; set; } = string.Empty;
    public bool EnableLogging { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;
}

public class SmsSettings
{
    public const string SectionName = "SmsSettings";
    
    public string Provider { get; set; } = "Demo"; // "Demo", "Twilio"
    public string ApplicationName { get; set; } = "AuthApp";
    public int CodeExpirationMinutes { get; set; } = 5;
    public int MaxAttemptsPerPeriod { get; set; } = 3;
    public int RateLimitPeriodMinutes { get; set; } = 10;
}
