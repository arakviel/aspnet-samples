namespace AspNet.MinimalApi.AuthSms.Services;

public interface ISmsService
{
    Task<bool> SendSmsAsync(string phoneNumber, string message);
    Task<bool> SendVerificationCodeAsync(string phoneNumber, string code);
}
