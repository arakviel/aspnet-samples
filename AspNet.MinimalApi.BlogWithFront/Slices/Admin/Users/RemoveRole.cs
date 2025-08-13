using AspNet.MinimalApi.BlogWithFront.Common;
using AspNet.MinimalApi.BlogWithFront.Domain;
using Microsoft.AspNetCore.Identity;

namespace AspNet.MinimalApi.BlogWithFront.Slices.Admin.Users;

/// <summary>
/// Видалення ролі у користувача - тільки для адмінів
/// </summary>
public static class RemoveRole
{
    public record RemoveRoleRequest(string UserId, string RoleName);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/admin/users/remove-role", Handler)
                .WithTags("Admin")
                .RequireAuthorization("RequireAdminRole");
        }
    }

    public static async Task<IResult> Handler(RemoveRoleRequest request, UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null) return Results.NotFound("Користувача не знайдено");

        if (!await userManager.IsInRoleAsync(user, request.RoleName))
            return Results.BadRequest("Користувач не має цієї ролі");

        var result = await userManager.RemoveFromRoleAsync(user, request.RoleName);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);

        return Results.Ok(new { Message = $"Роль {request.RoleName} видалена у користувача {user.UserName}" });
    }
}
