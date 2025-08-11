using AspNet.MinimalApi.BlogWithFront.Common;
using AspNet.MinimalApi.BlogWithFront.Domain;
using Microsoft.AspNetCore.Identity;

namespace AspNet.MinimalApi.BlogWithFront.Slices.Admin.Users;

/// <summary>
/// Призначення ролі користувачу - тільки для адмінів
/// </summary>
public static class AssignRole
{
    public record Request(string UserId, string RoleName);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/admin/users/assign-role", Handler)
                .WithTags("Admin")
                .RequireAuthorization("RequireAdminRole");
        }
    }

    public static async Task<IResult> Handler(Request request, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null) return Results.NotFound("Користувача не знайдено");

        if (!await roleManager.RoleExistsAsync(request.RoleName))
            return Results.BadRequest("Роль не існує");

        if (await userManager.IsInRoleAsync(user, request.RoleName))
            return Results.BadRequest("Користувач вже має цю роль");

        var result = await userManager.AddToRoleAsync(user, request.RoleName);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);

        return Results.Ok(new { Message = $"Роль {request.RoleName} призначена користувачу {user.UserName}" });
    }
}
