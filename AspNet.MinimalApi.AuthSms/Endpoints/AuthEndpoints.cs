using AspNet.MinimalApi.AuthSms.DTOs;
using AspNet.MinimalApi.AuthSms.Models;
using AspNet.MinimalApi.AuthSms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace AspNet.MinimalApi.AuthSms.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        // Відправка SMS коду для реєстрації
        auth.MapPost("/send-registration-code", SendRegistrationCode)
            .WithName("SendRegistrationCode")
            .WithSummary("Send SMS verification code for registration")
            .Produces<ApiResponse<string>>(200)
            .Produces<ApiResponse<string>>(400);

        // Відправка SMS коду для входу
        auth.MapPost("/send-login-code", SendLoginCode)
            .WithName("SendLoginCode")
            .WithSummary("Send SMS verification code for login")
            .Produces<ApiResponse<string>>(200)
            .Produces<ApiResponse<string>>(400);

        // Реєстрація з перевіркою SMS коду
        auth.MapPost("/register", Register)
            .WithName("Register")
            .WithSummary("Register new user with SMS verification")
            .Produces<AuthResponse>(200)
            .Produces<AuthResponse>(400);

        // Вхід з перевіркою SMS коду
        auth.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Login user with SMS verification")
            .Produces<AuthResponse>(200)
            .Produces<AuthResponse>(400);

        // Оновлення токена
        auth.MapPost("/refresh", RefreshToken)
            .WithName("RefreshToken")
            .WithSummary("Refresh access token")
            .Produces<AuthResponse>(200)
            .Produces<AuthResponse>(400);

        // Вихід (відкликання токена)
        auth.MapPost("/logout", Logout)
            .WithName("Logout")
            .WithSummary("Logout and revoke tokens")
            .RequireAuthorization()
            .Produces<ApiResponse<string>>(200);

        // Отримання інформації про поточного користувача
        auth.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithSummary("Get current user information")
            .RequireAuthorization()
            .Produces<ApiResponse<UserInfo>>(200);
    }

    private static async Task<IResult> SendRegistrationCode(
        [FromBody] SendSmsRequest request,
        IAuthService authService)
    {
        if (!Enum.TryParse<SmsVerificationPurpose>(request.Purpose, true, out var purpose) || 
            purpose != SmsVerificationPurpose.Registration)
        {
            return Results.BadRequest(new ApiResponse<string>(false, "Invalid purpose. Use 'Registration'."));
        }

        var result = await authService.SendVerificationCodeAsync(request.PhoneNumber, purpose);
        
        return result.Success 
            ? Results.Ok(result) 
            : Results.BadRequest(result);
    }

    private static async Task<IResult> SendLoginCode(
        [FromBody] SendSmsRequest request,
        IAuthService authService)
    {
        if (!Enum.TryParse<SmsVerificationPurpose>(request.Purpose, true, out var purpose) || 
            purpose != SmsVerificationPurpose.Login)
        {
            return Results.BadRequest(new ApiResponse<string>(false, "Invalid purpose. Use 'Login'."));
        }

        var result = await authService.SendVerificationCodeAsync(request.PhoneNumber, purpose);
        
        return result.Success 
            ? Results.Ok(result) 
            : Results.BadRequest(result);
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterRequest request,
        IAuthService authService)
    {
        var result = await authService.VerifyCodeAndRegisterAsync(request);
        
        return result.Success 
            ? Results.Ok(result) 
            : Results.BadRequest(result);
    }

    private static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        IAuthService authService)
    {
        var result = await authService.VerifyCodeAndLoginAsync(request);
        
        return result.Success 
            ? Results.Ok(result) 
            : Results.BadRequest(result);
    }

    private static async Task<IResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        IAuthService authService)
    {
        var result = await authService.RefreshTokenAsync(request.RefreshToken);
        
        return result.Success 
            ? Results.Ok(result) 
            : Results.BadRequest(result);
    }

    private static async Task<IResult> Logout(
        ClaimsPrincipal user,
        IAuthService authService)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.BadRequest(new ApiResponse<string>(false, "Invalid user."));
        }

        var success = await authService.RevokeTokenAsync(userId);
        
        return success 
            ? Results.Ok(new ApiResponse<string>(true, "Logged out successfully."))
            : Results.BadRequest(new ApiResponse<string>(false, "Failed to logout."));
    }

    private static IResult GetCurrentUser(ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var phoneNumber = user.FindFirst(ClaimTypes.MobilePhone)?.Value;
        var firstName = user.FindFirst("firstName")?.Value;
        var lastName = user.FindFirst("lastName")?.Value;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(phoneNumber))
        {
            return Results.BadRequest(new ApiResponse<UserInfo>(false, "Invalid user data."));
        }

        var userInfo = new UserInfo(
            userId,
            phoneNumber,
            firstName,
            lastName,
            DateTime.UtcNow, // В реальному проекті це буде з бази даних
            DateTime.UtcNow  // В реальному проекті це буде з бази даних
        );

        return Results.Ok(new ApiResponse<UserInfo>(true, "User information retrieved.", userInfo));
    }
}

public record RefreshTokenRequest([Required] string RefreshToken);
