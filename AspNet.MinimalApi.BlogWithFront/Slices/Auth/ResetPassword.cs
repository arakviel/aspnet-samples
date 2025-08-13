using AspNet.MinimalApi.BlogWithFront.Common;
using AspNet.MinimalApi.BlogWithFront.Domain;
using Microsoft.AspNetCore.Identity;

namespace AspNet.MinimalApi.BlogWithFront.Slices.Auth;

public static class ResetPassword
{
    public record ResetPasswordRequest(string Email, string Token, string Password, string ConfirmPassword);
    public record Response(string Message);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/reset-password", Handler)
                .WithTags("Auth")
                .WithOpenApi();
        }
    }

    public static async Task<IResult> Handler(
        ResetPasswordRequest request,
        UserManager<ApplicationUser> userManager)
    {
        // Валідація
        if (request.Password != request.ConfirmPassword)
        {
            return Results.BadRequest(new { Message = "Паролі не співпадають" });
        }

        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 6)
        {
            return Results.BadRequest(new { Message = "Пароль повинен містити мінімум 6 символів" });
        }

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Results.BadRequest(new { Message = "Невірний токен або email" });
        }

        // Скидаємо пароль
        var result = await userManager.ResetPasswordAsync(user, request.Token, request.Password);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Results.BadRequest(new { Message = $"Помилка скидання пароля: {errors}" });
        }

        return Results.Ok(new Response("Пароль успішно змінено"));
    }
}
