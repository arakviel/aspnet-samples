using System.Security.Claims;
using AspNet.MinimalApi.BlogWithFront.Common;
using AspNet.MinimalApi.BlogWithFront.Services;
using Microsoft.AspNetCore.Authorization;

namespace AspNet.MinimalApi.BlogWithFront.Slices.Auth;

public static class Logout
{
    public record Request(string? RefreshToken = null);
    public record Response(string Message);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/logout", Handler)
                .WithTags("Auth")
                .WithOpenApi()
                .RequireAuthorization();
        }
    }

    public static async Task<IResult> Handler(
        Request request, 
        RefreshTokenService refreshTokenService, 
        ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        int revokedCount = 0;

        // Якщо передано конкретний рефреш токен, відкликаємо тільки його
        if (!string.IsNullOrEmpty(request.RefreshToken))
        {
            var revoked = await refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken);
            revokedCount = revoked ? 1 : 0;
        }
        else
        {
            // Інакше відкликаємо всі рефреш токени користувача
            revokedCount = await refreshTokenService.RevokeAllUserRefreshTokensAsync(userId);
        }

        var message = revokedCount > 0 
            ? $"Успішно відкликано {revokedCount} токен(ів)" 
            : "Активних токенів не знайдено";

        return Results.Ok(new Response(message));
    }
}
