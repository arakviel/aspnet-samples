using AspNet.MinimalApi.AuthSms.DTOs;
using AspNet.MinimalApi.AuthSms.Models;

namespace AspNet.MinimalApi.AuthSms.Services;

public interface IAuthService
{
    Task<ApiResponse<string>> SendVerificationCodeAsync(string phoneNumber, SmsVerificationPurpose purpose);
    Task<AuthResponse> VerifyCodeAndRegisterAsync(RegisterRequest request);
    Task<AuthResponse> VerifyCodeAndLoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string userId);
    string GenerateJwtToken(ApplicationUser user);
    string GenerateRefreshToken();
}
