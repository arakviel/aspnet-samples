using AspNet.MinimalApi.BlogWithFront.Common;
using AspNet.MinimalApi.BlogWithFront.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AspNet.MinimalApi.BlogWithFront.Slices.Posts;

/// <summary>
/// Оновлення посту - доступно тільки автору або адміну
/// </summary>
public static class UpdatePost
{
    public record Request(string Title, string Content);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("/posts/{id:int}", Handler)
               .WithTags("Posts")
               .RequireAuthorization();
        }
    }

    public static async Task<IResult> Handler(int id, Request request, AppDbContext db, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Results.Unauthorized();

        var isAdmin = user.IsInRole("Admin");
        
        var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == id);
        if (post is null) return Results.NotFound();

        // Перевіряємо, чи користувач є автором посту або адміном
        if (post.AuthorId != userId && !isAdmin)
            return Results.Forbid();

        post.Title = request.Title;
        post.Content = request.Content;
        
        await db.SaveChangesAsync();
        
        return Results.Ok(new { post.Id, post.Title, post.Content, post.CreatedDate });
    }
}
