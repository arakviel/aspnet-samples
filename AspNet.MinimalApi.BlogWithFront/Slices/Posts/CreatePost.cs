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
    public record CreatePostRequest(string Title, string Content, string? Summary, string[] Tags, bool IsPublished);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/posts", Handler)
               .WithTags("Posts")
               .RequireAuthorization(); // Дозволяємо всім аутентифікованим користувачам
        }
    }

    public static async Task<IResult> Handler(CreatePostRequest request, AppDbContext db, UserManager<ApplicationUser> userManager, ClaimsPrincipal user)
    {
        var uid = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (uid is null) return Results.Unauthorized();
        var author = await userManager.FindByIdAsync(uid);
        if (author is null) return Results.Unauthorized();

        var post = new Post
        {
            Title = request.Title,
            Content = request.Content,
            Summary = request.Summary,
            AuthorId = author.Id,
            Author = author,
            IsPublished = request.IsPublished,
            Tags = string.Join(",", request.Tags) // Зберігаємо теги як рядок, розділений комами
        };

        db.Posts.Add(post);
        await db.SaveChangesAsync();

        var response = new
        {
            post.Id,
            post.Title,
            post.Content,
            post.Summary,
            post.CreatedDate,
            post.IsPublished,
            Tags = request.Tags,
            AuthorName = author.UserName ?? author.Email
        };

        return Results.Created($"/posts/{post.Id}", response);
    }
}

