using AspNet.MinimalApi.BlogWithFront.Common;
using AspNet.MinimalApi.BlogWithFront.Domain;
using AspNet.MinimalApi.BlogWithFront.Services;
using Microsoft.AspNetCore.Identity;

namespace AspNet.MinimalApi.BlogWithFront.Slices.Auth;

public static class Register
{
    public record RegisterRequest(string UserName, string Email, string Password);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/register", Handler).WithTags("Auth");
        }
    }

    public static async Task<IResult> Handler(
        RegisterRequest request,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IEmailService emailService,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("Register");

        var user = new ApplicationUser { UserName = request.UserName, Email = request.Email, EmailConfirmed = false };
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) return Results.BadRequest(result.Errors);

        if (!await roleManager.RoleExistsAsync("User"))
            await roleManager.CreateAsync(new IdentityRole("User"));
        await userManager.AddToRoleAsync(user, "User");

        // Генеруємо токен для підтвердження email
        var emailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);

        try
        {
            // Надсилаємо email підтвердження
            await emailService.SendEmailConfirmationAsync(user.Email!, emailToken);
            logger.LogInformation("Email confirmation sent to {Email}", request.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email confirmation to {Email}", request.Email);
            // Продовжуємо, навіть якщо email не надіслався
        }

        return Results.Ok(new { Message = "User registered. Please check your email to confirm your account." });
    }
}

