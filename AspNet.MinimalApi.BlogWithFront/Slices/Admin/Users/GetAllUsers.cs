using AspNet.MinimalApi.BlogWithFront.Common;
using AspNet.MinimalApi.BlogWithFront.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspNet.MinimalApi.BlogWithFront.Slices.Admin.Users;

/// <summary>
/// Отримання списку всіх користувачів - тільки для адмінів
/// </summary>
public static class GetAllUsers
{
    public record Response(string Id, string UserName, string Email, DateTime RegistrationDate, string[] Roles);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/admin/users", Handler)
                .WithTags("Admin")
                .RequireAuthorization("RequireAdminRole")
                .WithOpenApi();
        }
    }

    public static async Task<IResult> Handler(UserManager<ApplicationUser> userManager)
    {
        var users = await userManager.Users.ToListAsync();
        var result = new List<Response>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(new Response(
                user.Id,
                user.UserName ?? "Не вказано",
                user.Email ?? "Не вказано",
                user.RegistrationDate,
                roles.ToArray()
            ));
        }

        return Results.Ok(result);
    }
}
