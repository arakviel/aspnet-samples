using AspNet.MinimalApi.AuthSmsIdentity.Models;
using AspNet.MinimalApi.AuthSmsIdentity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AspNet.MinimalApi.AuthSmsIdentity.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        // Реєстрація користувача
        auth.MapPost("/register", Register)
            .WithName("Register")
            .WithSummary("Register new user with phone number");

        // Вхід користувача (традиційний)
        auth.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Login user with phone number and password");

        // SMS-only логін
        auth.MapPost("/send-sms-login-code", SendSmsLoginCode)
            .WithName("SendSmsLoginCode")
            .WithSummary("Send SMS code for passwordless login");

        auth.MapPost("/sms-login", SmsLogin)
            .WithName("SmsLogin")
            .WithSummary("Login user with SMS code only (passwordless)");

        // Вихід користувача
        auth.MapPost("/logout", Logout)
            .WithName("Logout")
            .WithSummary("Logout current user")
            .RequireAuthorization();

        // Відправка SMS коду для підтвердження телефону
        auth.MapPost("/send-phone-confirmation", SendPhoneConfirmation)
            .WithName("SendPhoneConfirmation")
            .WithSummary("Send SMS code for phone confirmation")
            .RequireAuthorization();

        // Підтвердження номера телефону
        auth.MapPost("/confirm-phone", ConfirmPhone)
            .WithName("ConfirmPhone")
            .WithSummary("Confirm phone number with SMS code")
            .RequireAuthorization();

        // Увімкнення двофакторної аутентифікації через SMS
        auth.MapPost("/enable-2fa-sms", EnableTwoFactorSms)
            .WithName("EnableTwoFactorSms")
            .WithSummary("Enable two-factor authentication via SMS")
            .RequireAuthorization();

        // Вимкнення двофакторної аутентифікації
        auth.MapPost("/disable-2fa", DisableTwoFactor)
            .WithName("DisableTwoFactor")
            .WithSummary("Disable two-factor authentication")
            .RequireAuthorization();

        // Отримання інформації про користувача
        auth.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithSummary("Get current user information")
            .RequireAuthorization();

        // Генерація recovery кодів
        auth.MapPost("/generate-recovery-codes", GenerateRecoveryCodes)
            .WithName("GenerateRecoveryCodes")
            .WithSummary("Generate new recovery codes")
            .RequireAuthorization();
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterRequest request,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IJwtService jwtService,
        HttpResponse response)
    {
        // Створюємо користувача з номером телефону як UserName
        var user = new IdentityUser
        {
            UserName = request.PhoneNumber,
            PhoneNumber = request.PhoneNumber
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return Results.BadRequest(ApiResponse<object>.ErrorResult("Registration failed", errors));
        }

        // Автоматично входимо користувача після реєстрації
        await signInManager.SignInAsync(user, isPersistent: false);

        // Генеруємо JWT та зберігаємо в cookie
        var jwtToken = jwtService.GenerateJwtToken(user);
        jwtService.SetJwtCookie(response, jwtToken);

        var userInfo = new UserInfo
        {
            Id = user.Id,
            PhoneNumber = user.PhoneNumber!,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled
        };

        return Results.Ok(ApiResponse<UserInfo>.SuccessResult(userInfo, "Registration successful"));
    }

    private static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        IJwtService jwtService,
        HttpResponse response)
    {
        var user = await userManager.FindByNameAsync(request.PhoneNumber);
        if (user == null)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult("Invalid phone number or password"));
        }

        var result = await signInManager.PasswordSignInAsync(
            user, request.Password, request.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            // Генеруємо JWT та зберігаємо в cookie
            var jwtToken = jwtService.GenerateJwtToken(user);
            jwtService.SetJwtCookie(response, jwtToken);

            var userInfo = new UserInfo
            {
                Id = user.Id,
                PhoneNumber = user.PhoneNumber!,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled
            };

            return Results.Ok(ApiResponse<UserInfo>.SuccessResult(userInfo, "Login successful"));
        }

        if (result.RequiresTwoFactor)
        {
            return Results.Ok(ApiResponse<object>.SuccessResult(new { RequiresTwoFactor = true }, 
                "Two-factor authentication required"));
        }

        if (result.IsLockedOut)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult("Account locked out"));
        }

        return Results.BadRequest(ApiResponse<object>.ErrorResult("Invalid phone number or password"));
    }

    private static async Task<IResult> Logout(
        SignInManager<IdentityUser> signInManager,
        IJwtService jwtService,
        HttpResponse response)
    {
        await signInManager.SignOutAsync();
        jwtService.ClearJwtCookie(response);
        return Results.Ok(ApiResponse<object>.SuccessResult(null, "Logout successful"));
    }

    private static async Task<IResult> SendPhoneConfirmation(
        ClaimsPrincipal user,
        UserManager<IdentityUser> userManager,
        ISmsSender smsSender)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult("User not found"));
        }

        if (currentUser.PhoneNumberConfirmed)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult("Phone number already confirmed"));
        }

        // Генеруємо код підтвердження
        var code = await userManager.GenerateChangePhoneNumberTokenAsync(currentUser, currentUser.PhoneNumber!);

        // Відправляємо SMS
        await smsSender.SendSmsAsync(currentUser.PhoneNumber!,
            $"Your phone confirmation code is: {code}");

        return Results.Ok(ApiResponse<object>.SuccessResult(null, "SMS code sent"));
    }

    private static async Task<IResult> ConfirmPhone(
        [FromBody] ConfirmPhoneRequest request,
        ClaimsPrincipal user,
        UserManager<IdentityUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult("User not found"));
        }

        // Використовуємо готовий метод Identity
        var result = await userManager.ChangePhoneNumberAsync(currentUser, request.PhoneNumber, request.Code);

        // Альтернативний підхід (більше коду):
        // var isValidToken = await userManager.VerifyChangePhoneNumberTokenAsync(currentUser, request.Code, request.PhoneNumber);
        // if (isValidToken)
        // {
        //     currentUser.PhoneNumber = request.PhoneNumber;
        //     currentUser.PhoneNumberConfirmed = true;
        //     var updateResult = await userManager.UpdateAsync(currentUser);
        //     // result = updateResult;
        // }
        // else
        // {
        //     // result = IdentityResult.Failed(new IdentityError { Description = "Invalid code" });
        // }

        if (result.Succeeded)
        {
            return Results.Ok(ApiResponse<object>.SuccessResult(null, "Phone number confirmed"));
        }

        var errors = result.Errors.Select(e => e.Description).ToList();
        return Results.BadRequest(ApiResponse<object>.ErrorResult("Phone confirmation failed", errors));
    }

    private static async Task<IResult> EnableTwoFactorSms(
        ClaimsPrincipal user,
        UserManager<IdentityUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult("User not found"));
        }

        if (!currentUser.PhoneNumberConfirmed)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult("Phone number must be confirmed first"));
        }

        await userManager.SetTwoFactorEnabledAsync(currentUser, true);

        return Results.Ok(ApiResponse<object>.SuccessResult(null, "Two-factor authentication enabled"));
    }

    private static async Task<IResult> DisableTwoFactor(
        ClaimsPrincipal user,
        UserManager<IdentityUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult("User not found"));
        }

        await userManager.SetTwoFactorEnabledAsync(currentUser, false);

        return Results.Ok(ApiResponse<object>.SuccessResult(null, "Two-factor authentication disabled"));
    }

    private static async Task<IResult> GetCurrentUser(
        ClaimsPrincipal user,
        UserManager<IdentityUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult("User not found"));
        }

        var userInfo = new UserInfo
        {
            Id = currentUser.Id,
            PhoneNumber = currentUser.PhoneNumber!,
            PhoneNumberConfirmed = currentUser.PhoneNumberConfirmed,
            TwoFactorEnabled = currentUser.TwoFactorEnabled
        };

        return Results.Ok(ApiResponse<UserInfo>.SuccessResult(userInfo, "User information retrieved"));
    }

    private static async Task<IResult> GenerateRecoveryCodes(
        ClaimsPrincipal user,
        UserManager<IdentityUser> userManager)
    {
        var currentUser = await userManager.GetUserAsync(user);
        if (currentUser == null)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult("User not found"));
        }

        if (!currentUser.TwoFactorEnabled)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult("Two-factor authentication must be enabled"));
        }

        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(currentUser, 10);

        return Results.Ok(ApiResponse<IEnumerable<string>>.SuccessResult(recoveryCodes,
            "Recovery codes generated"));
    }

    // SMS-only логін методи
    private static async Task<IResult> SendSmsLoginCode(
        [FromBody] SendSmsLoginCodeRequest request,
        UserManager<IdentityUser> userManager,
        ISmsSender smsSender)
    {
        // Перевіряємо чи користувач існує
        var user = await userManager.FindByNameAsync(request.PhoneNumber);
        if (user == null)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult("User not found. Please register first."));
        }

        // Використовуємо нативний метод Identity для генерації токена для SMS логіну
        // Використовуємо ChangePhoneNumberTokenProvider для SMS логіну
        var code = await userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultPhoneProvider, "sms-login");

        // Відправляємо SMS
        var appName = "AuthApp";
        var message = $"Your {appName} login code is: {code}. This code will expire in 5 minutes.";
        await smsSender.SendSmsAsync(user.PhoneNumber!, message);

        return Results.Ok(ApiResponse<object>.SuccessResult(null, "SMS login code sent"));
    }

    private static async Task<IResult> SmsLogin(
        [FromBody] SmsLoginRequest request,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IJwtService jwtService,
        HttpResponse response)
    {
        // Знаходимо користувача
        var user = await userManager.FindByNameAsync(request.PhoneNumber);
        if (user == null)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult("User not found"));
        }

        // Валідуємо SMS код через нативний метод Identity
        var isValidToken = await userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultPhoneProvider, "sms-login", request.Code);

        if (!isValidToken)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult("Invalid or expired SMS code"));
        }

        // Входимо користувача через SignInManager
        await signInManager.SignInAsync(user, isPersistent: request.RememberMe);

        // Генеруємо JWT та зберігаємо в cookie
        var jwtToken = jwtService.GenerateJwtToken(user);
        jwtService.SetJwtCookie(response, jwtToken);

        var userInfo = new UserInfo
        {
            Id = user.Id,
            PhoneNumber = user.PhoneNumber!,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled
        };

        return Results.Ok(ApiResponse<UserInfo>.SuccessResult(userInfo, "SMS login successful"));
    }
}
