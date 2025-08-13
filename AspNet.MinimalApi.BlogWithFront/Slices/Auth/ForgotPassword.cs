using AspNet.MinimalApi.BlogWithFront.Common;
using AspNet.MinimalApi.BlogWithFront.Domain;
using AspNet.MinimalApi.BlogWithFront.Services;
using Microsoft.AspNetCore.Identity;

namespace AspNet.MinimalApi.BlogWithFront.Slices.Auth;

public static class ForgotPassword
{
    public record ForgotPasswordRequest(string Email);
    public record Response(string Message);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/forgot-password", Handler)
                .WithTags("Auth")
                .WithOpenApi();
        }
    }

    public static async Task<IResult> Handler(
        ForgotPasswordRequest request,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("ForgotPassword");

        var user = await userManager.FindByEmailAsync(request.Email);

        // З міркувань безпеки, завжди повертаємо успіх, навіть якщо користувача не існує
        if (user == null)
        {
            logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
            return Results.Ok(new Response("Якщо email існує в системі, інструкції для відновлення пароля будуть надіслані."));
        }

        // Генеруємо токен для скидання пароля
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);

        try
        {
            // Надсилаємо email з інструкціями для відновлення пароля
            await emailService.SendPasswordResetEmailAsync(user.Email!, resetToken);
            logger.LogInformation("Password reset email sent to {Email}", request.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send password reset email to {Email}", request.Email);
            // Не розкриваємо помилку користувачу з міркувань безпеки
        }

        return Results.Ok(new Response("Якщо email існує в системі, інструкції для відновлення пароля будуть надіслані."));
    }
}
