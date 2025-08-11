using AspNet.MinimalApi.BlogWithFront.Common;
using AspNet.MinimalApi.BlogWithFront.Domain;
using Microsoft.AspNetCore.Identity;

namespace AspNet.MinimalApi.BlogWithFront.Slices.Admin.Users;

public static class GetUsers
{
    public record Response(string Id, string? UserName, string? Email, string[] Roles);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/admin/users", Handler)
                .WithTags("Admin")
                .RequireAuthorization("RequireAdminRole");
        }
    }

    public static async Task<IResult> Handler(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        var users = userManager.Users.ToList();
        var list = new List<Response>();
        foreach (var u in users)
        {
            var roles = await userManager.GetRolesAsync(u);
            list.Add(new Response(u.Id, u.UserName, u.Email, roles.ToArray()));
        }
        return Results.Ok(list);
    }
}

