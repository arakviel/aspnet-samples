using AspNet.MinimalApi.BlogWithFront.Common;
using AspNet.MinimalApi.BlogWithFront.Domain;
using Microsoft.AspNetCore.Identity;

namespace AspNet.MinimalApi.BlogWithFront.Slices.Auth;

public static class ConfirmEmail
{
    public record ConfirmEmailRequest(string Email, string Token);
    public record Response(string Message);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/confirm-email", Handler)
                .WithTags("Auth")
                .WithOpenApi();
        }
    }

    public static async Task<IResult> Handler(
        ConfirmEmailRequest request,
        UserManager<ApplicationUser> userManager,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("ConfirmEmail");

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            logger.LogWarning("Email confirmation attempted for non-existent email: {Email}", request.Email);
            return Results.BadRequest(new Response("Invalid email or token."));
        }

        if (user.EmailConfirmed)
        {
            return Results.Ok(new Response("Email already confirmed. You can log in."));
        }

        var result = await userManager.ConfirmEmailAsync(user, request.Token);

        if (!result.Succeeded)
        {
            logger.LogWarning("Email confirmation failed for {Email}: {Errors}",
                request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return Results.BadRequest(new Response("Invalid email or token."));
        }

        logger.LogInformation("Email confirmed successfully for {Email}", request.Email);
        return Results.Ok(new Response("Email confirmed successfully. You can now log in."));
    }
}
