using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AspNet.MinimalApi.BlogWithFront.Common;
using AspNet.MinimalApi.BlogWithFront.Domain;
using AspNet.MinimalApi.BlogWithFront.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace AspNet.MinimalApi.BlogWithFront.Slices.Auth;

public static class Login
{
    public record Request(string UserName, string Password);
    public record Response(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/login", Handler)
                .WithTags("Auth")
                .WithOpenApi();
        }
    }

    public static async Task<IResult> Handler(Request request, UserManager<ApplicationUser> userManager, IConfiguration config, RefreshTokenService refreshTokenService)
    {
        var user = await userManager.FindByNameAsync(request.UserName) ?? await userManager.FindByEmailAsync(request.UserName);
        if (user is null) return Results.Unauthorized();
        if (!await userManager.CheckPasswordAsync(user, request.Password)) return Results.Unauthorized();

        var jwtSection = config.GetSection("Jwt");
        var issuer = jwtSection["Issuer"] ?? "blog-api";
        var audience = jwtSection["Audience"] ?? "blog-api-client";
        var key = jwtSection["Key"] ?? "dev-secret-change";
        var expiresMinutes = int.TryParse(jwtSection["ExpiresMinutes"], out var m) ? m : 60;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id)
        };
        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256)
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        // Генеруємо рефреш токен
        var refreshToken = await refreshTokenService.GenerateRefreshTokenAsync(user.Id);

        return Results.Ok(new Response(jwt, refreshToken.Token, token.ValidTo));
    }
}

