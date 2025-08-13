using System.Security.Claims;
using AspNet.MinimalApi.BlogWithFront.Common;
using AspNet.MinimalApi.BlogWithFront.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AspNet.MinimalApi.BlogWithFront.Slices.Auth;

public static class GetCurrentUser
{
    public record Response(
        string Id, 
        string Name, 
        string Email, 
        bool EmailConfirmed, 
        bool TwoFactorEnabled,
        DateTime RegistrationDate,
        string[] Roles
    );

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/auth/me", Handler)
                .WithTags("Auth")
                .WithOpenApi()
                .RequireAuthorization();
        }
    }

    public static async Task<IResult> Handler(
        ClaimsPrincipal user, 
        UserManager<ApplicationUser> userManager)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Results.Unauthorized();

        var appUser = await userManager.FindByIdAsync(userId);
        if (appUser == null)
            return Results.Unauthorized();

        var roles = await userManager.GetRolesAsync(appUser);

        var response = new Response(
            appUser.Id,
            appUser.UserName ?? appUser.Email ?? "Unknown",
            appUser.Email ?? "",
            appUser.EmailConfirmed,
            appUser.TwoFactorEnabled,
            appUser.RegistrationDate,
            roles.ToArray()
        );

        return Results.Ok(response);
    }
}
