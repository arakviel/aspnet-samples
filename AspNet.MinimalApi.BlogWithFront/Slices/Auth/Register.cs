using AspNet.MinimalApi.BlogWithFront.Common;
using AspNet.MinimalApi.BlogWithFront.Domain;
using Microsoft.AspNetCore.Identity;

namespace AspNet.MinimalApi.BlogWithFront.Slices.Auth;

public static class Register
{
    public record Request(string UserName, string Email, string Password);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/register", Handler).WithTags("Auth");
        }
    }

    public static async Task<IResult> Handler(Request request, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        var user = new ApplicationUser { UserName = request.UserName, Email = request.Email, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) return Results.BadRequest(result.Errors);

        if (!await roleManager.RoleExistsAsync("User"))
            await roleManager.CreateAsync(new IdentityRole("User"));
        await userManager.AddToRoleAsync(user, "User");

        return Results.Ok(new { Message = "User registered" });
    }
}

