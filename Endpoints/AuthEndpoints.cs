using System.Text;
using System.Text.Encodings.Web;
using Auth2Factors.Models;
using Auth2Factors.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Auth2Factors.Endpoints;

/// <summary>
/// Endpoints для 2FA аутентифікації з Email та TOTP
/// Використовує нативні можливості ASP.NET Core Identity
/// </summary>
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth").WithTags("Authentication");

        // Базові операції
        auth.MapPost("/register", Register)
            .WithName("Register")
            .WithSummary("Register new user with email");

        auth.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Login user (may require 2FA)");

        auth.MapPost("/verify-2fa", Verify2Fa)
            .WithName("Verify2FA")
            .WithSummary("Verify 2FA code (email or TOTP)");

        auth.MapPost("/logout", Logout)
            .WithName("Logout")
            .WithSummary("Logout user");

        // Інформація про користувача
        auth.MapGet("/me", GetUserInfo)
            .WithName("GetUserInfo")
            .WithSummary("Get current user information")
            .RequireAuthorization();

        // TOTP (Authenticator App) управління
        auth.MapPost("/setup-totp", SetupTotp)
            .WithName("SetupTOTP")
            .WithSummary("Setup TOTP authenticator (Google Authenticator, etc.)")
            .RequireAuthorization();

        auth.MapPost("/verify-totp", VerifyTotp)
            .WithName("VerifyTOTP")
            .WithSummary("Verify and enable TOTP authenticator")
            .RequireAuthorization();

        auth.MapPost("/disable-2fa", Disable2Fa)
            .WithName("Disable2FA")
            .WithSummary("Disable two-factor authentication")
            .RequireAuthorization();

        // Recovery коди
        auth.MapPost("/generate-recovery-codes", GenerateRecoveryCodes)
            .WithName("GenerateRecoveryCodes")
            .WithSummary("Generate new recovery codes")
            .RequireAuthorization();

        auth.MapPost("/use-recovery-code", UseRecoveryCode)
            .WithName("UseRecoveryCode")
            .WithSummary("Login using recovery code");
    }

    /// <summary>
    /// Реєстрація користувача
    /// Identity автоматично хешує пароль та створює користувача
    /// </summary>
    private static async Task<IResult> Register(
        [FromBody] RegisterRequest request,
        UserManager<IdentityUser> userManager,
        IEmailSender emailSender)
    {
        // Створюємо користувача через Identity
        var user = new IdentityUser 
        { 
            UserName = request.Email, 
            Email = request.Email 
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return Results.BadRequest(ApiResponse<object>.ErrorResult("Registration failed", errors));
        }

        // Відправляємо email підтвердження через нативний метод Identity
        var emailConfirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = $"http://localhost:5092/api/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(emailConfirmationToken)}";
        
        await emailSender.SendEmailAsync(
            user.Email!,
            "Confirm your email",
            $"Please confirm your email by clicking <a href='{confirmationLink}'>here</a>");

        return Results.Ok(ApiResponse<object>.SuccessResult(null, 
            "Registration successful. Please check your email to confirm your account."));
    }

    /// <summary>
    /// Вхід користувача
    /// Identity автоматично перевіряє чи потрібна 2FA
    /// </summary>
    private static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager)
    {
        // Спочатку перевіряємо пароль без входу
        var result = await signInManager.PasswordSignInAsync(
            request.Email, 
            request.Password, 
            request.RememberMe, 
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            var userInfo = await CreateUserInfo(user!, userManager, signInManager);
            return Results.Ok(ApiResponse<UserInfo>.SuccessResult(userInfo, "Login successful"));
        }

        if (result.RequiresTwoFactor)
        {
            // Identity автоматично визначає що потрібна 2FA
            return Results.Ok(ApiResponse<object>.SuccessResult(new { RequiresTwoFactor = true }, 
                "Two-factor authentication required"));
        }

        if (result.IsLockedOut)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult("Account locked out"));
        }

        return Results.BadRequest(ApiResponse<object>.ErrorResult("Invalid email or password"));
    }

    /// <summary>
    /// Підтвердження 2FA коду
    /// Identity автоматично перевіряє TOTP або email коди
    /// </summary>
    private static async Task<IResult> Verify2Fa(
        [FromBody] Verify2FaRequest request,
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager)
    {
        // Спочатку пробуємо TOTP код (6 цифр)
        var result = await signInManager.TwoFactorAuthenticatorSignInAsync(
            request.Code, 
            request.RememberMachine, 
            rememberClient: request.RememberMachine);

        if (!result.Succeeded)
        {
            // Якщо TOTP не спрацював, пробуємо email код
            result = await signInManager.TwoFactorSignInAsync(
                TokenOptions.DefaultEmailProvider, 
                request.Code, 
                request.RememberMachine, 
                rememberClient: request.RememberMachine);
        }

        if (result.Succeeded)
        {
            var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
            var userInfo = await CreateUserInfo(user!, userManager, signInManager);
            return Results.Ok(ApiResponse<UserInfo>.SuccessResult(userInfo, "2FA verification successful"));
        }

        if (result.IsLockedOut)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult("Account locked out"));
        }

        return Results.BadRequest(ApiResponse<object>.ErrorResult("Invalid 2FA code"));
    }

    /// <summary>
    /// Налаштування TOTP (Google Authenticator)
    /// Identity автоматично генерує shared key та authenticator URI
    /// </summary>
    private static async Task<IResult> SetupTotp(
        UserManager<IdentityUser> userManager,
        HttpContext context)
    {
        var user = await userManager.GetUserAsync(context.User);
        if (user == null)
        {
            return Results.Unauthorized();
        }

        // Identity автоматично генерує унікальний ключ для TOTP
        var unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
        }

        var sharedKey = FormatKey(unformattedKey!);

        // Створюємо URI для QR коду (стандарт Google Authenticator)
        var email = await userManager.GetEmailAsync(user);
        var authenticatorUri = GenerateQrCodeUri(email!, unformattedKey!);
        var qrCodeUri = QrCodeHelper.GenerateQrCodeUri(authenticatorUri);

        // Генеруємо recovery коди
        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        var response = new SetupTotpResponse
        {
            SharedKey = sharedKey,
            AuthenticatorUri = authenticatorUri,
            QrCodeUri = qrCodeUri,
            RecoveryCodes = recoveryCodes!.ToArray()
        };

        return Results.Ok(ApiResponse<SetupTotpResponse>.SuccessResult(response, 
            "TOTP setup information generated. Scan QR code with authenticator app."));
    }

    /// <summary>
    /// Підтвердження та увімкнення TOTP
    /// Identity автоматично валідує TOTP код
    /// </summary>
    private static async Task<IResult> VerifyTotp(
        [FromBody] VerifyTotpRequest request,
        UserManager<IdentityUser> userManager,
        HttpContext context)
    {
        var user = await userManager.GetUserAsync(context.User);
        if (user == null)
        {
            return Results.Unauthorized();
        }

        // Identity автоматично валідує TOTP код
        var is2faTokenValid = await userManager.VerifyTwoFactorTokenAsync(
            user, userManager.Options.Tokens.AuthenticatorTokenProvider, request.Code);

        if (!is2faTokenValid)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult("Invalid TOTP code"));
        }

        // Увімкнуємо 2FA
        await userManager.SetTwoFactorEnabledAsync(user, true);

        return Results.Ok(ApiResponse<object>.SuccessResult(null, 
            "TOTP authenticator verified and enabled successfully"));
    }

    // Допоміжні методи...
    private static string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        int currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition));
        }

        return result.ToString().ToLowerInvariant();
    }

    private static string GenerateQrCodeUri(string email, string unformattedKey)
    {
        const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
        return string.Format(AuthenticatorUriFormat,
            UrlEncoder.Default.Encode("Auth2Factors"),
            UrlEncoder.Default.Encode(email),
            unformattedKey);
    }

    private static async Task<UserInfo> CreateUserInfo(
        IdentityUser user, 
        UserManager<IdentityUser> userManager, 
        SignInManager<IdentityUser> signInManager)
    {
        var recoveryCodesLeft = await userManager.CountRecoveryCodesAsync(user);
        var hasAuthenticator = await userManager.GetAuthenticatorKeyAsync(user) != null;
        var isMachineRemembered = await signInManager.IsTwoFactorClientRememberedAsync(user);

        return new UserInfo
        {
            Id = user.Id,
            Email = user.Email!,
            EmailConfirmed = user.EmailConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            HasAuthenticator = hasAuthenticator,
            RecoveryCodesLeft = recoveryCodesLeft,
            IsMachineRemembered = isMachineRemembered
        };
    }

    /// <summary>
    /// Вихід користувача
    /// </summary>
    private static async Task<IResult> Logout(SignInManager<IdentityUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return Results.Ok(ApiResponse<object>.SuccessResult(null, "Logout successful"));
    }

    /// <summary>
    /// Отримання інформації про поточного користувача
    /// </summary>
    private static async Task<IResult> GetUserInfo(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        HttpContext context)
    {
        var user = await userManager.GetUserAsync(context.User);
        if (user == null)
        {
            return Results.Unauthorized();
        }

        var userInfo = await CreateUserInfo(user, userManager, signInManager);
        return Results.Ok(ApiResponse<UserInfo>.SuccessResult(userInfo, "User information retrieved"));
    }

    /// <summary>
    /// Вимкнення двофакторної аутентифікації
    /// </summary>
    private static async Task<IResult> Disable2Fa(
        UserManager<IdentityUser> userManager,
        HttpContext context)
    {
        var user = await userManager.GetUserAsync(context.User);
        if (user == null)
        {
            return Results.Unauthorized();
        }

        // Identity автоматично вимикає 2FA та очищає authenticator key
        var disable2faResult = await userManager.SetTwoFactorEnabledAsync(user, false);
        if (!disable2faResult.Succeeded)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult("Failed to disable 2FA"));
        }

        await userManager.ResetAuthenticatorKeyAsync(user);

        return Results.Ok(ApiResponse<object>.SuccessResult(null,
            "Two-factor authentication disabled successfully"));
    }

    /// <summary>
    /// Генерація нових recovery кодів
    /// </summary>
    private static async Task<IResult> GenerateRecoveryCodes(
        UserManager<IdentityUser> userManager,
        HttpContext context)
    {
        var user = await userManager.GetUserAsync(context.User);
        if (user == null)
        {
            return Results.Unauthorized();
        }

        if (!user.TwoFactorEnabled)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult("2FA must be enabled to generate recovery codes"));
        }

        // Identity автоматично генерує нові recovery коди
        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        return Results.Ok(ApiResponse<IEnumerable<string>>.SuccessResult(recoveryCodes,
            "Recovery codes generated successfully"));
    }

    /// <summary>
    /// Використання recovery коду для входу
    /// </summary>
    private static async Task<IResult> UseRecoveryCode(
        [FromBody] RecoveryCodeRequest request,
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager)
    {
        // Identity автоматично валідує recovery код
        var result = await signInManager.TwoFactorRecoveryCodeSignInAsync(request.RecoveryCode);

        if (result.Succeeded)
        {
            var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
            var userInfo = await CreateUserInfo(user!, userManager, signInManager);
            return Results.Ok(ApiResponse<UserInfo>.SuccessResult(userInfo,
                "Recovery code login successful"));
        }

        if (result.IsLockedOut)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult("Account locked out"));
        }

        return Results.BadRequest(ApiResponse<object>.ErrorResult("Invalid recovery code"));
    }
}
