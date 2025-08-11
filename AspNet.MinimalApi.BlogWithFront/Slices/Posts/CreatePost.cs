using AspNet.MinimalApi.BlogWithFront.Common;
using AspNet.MinimalApi.BlogWithFront.Data;
using AspNet.MinimalApi.BlogWithFront.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AspNet.MinimalApi.BlogWithFront.Slices.Posts;

public static class CreatePost
{
    public record Request(string Title, string Content);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/posts", Handler)
               .WithTags("Posts")
               .RequireAuthorization("RequireAdminRole");
        }
    }

    public static async Task<IResult> Handler(Request request, AppDbContext db, UserManager<ApplicationUser> userManager, ClaimsPrincipal user)
    {
        var uid = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (uid is null) return Results.Unauthorized();
        var author = await userManager.FindByIdAsync(uid);
        if (author is null) return Results.Unauthorized();

        var post = new Post { Title = request.Title, Content = request.Content, AuthorId = author.Id, Author = author };
        db.Posts.Add(post);
        await db.SaveChangesAsync();
        return Results.Created($"/posts/{post.Id}", new { post.Id, post.Title, post.Content, post.CreatedDate });
    }
}

