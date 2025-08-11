using AspNet.MinimalApi.BlogWithFront.Common;
using AspNet.MinimalApi.BlogWithFront.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AspNet.MinimalApi.BlogWithFront.Slices.Posts;

/// <summary>
/// Видалення посту - доступно тільки автору або адміну
/// </summary>
public static class DeletePost
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("/posts/{id:int}", Handler)
               .WithTags("Posts")
               .RequireAuthorization();
        }
    }

    public static async Task<IResult> Handler(int id, AppDbContext db, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Results.Unauthorized();

        var isAdmin = user.IsInRole("Admin");
        
        var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == id);
        if (post is null) return Results.NotFound();

        // Перевіряємо, чи користувач є автором посту або адміном
        if (post.AuthorId != userId && !isAdmin)
            return Results.Forbid();

        db.Posts.Remove(post);
        await db.SaveChangesAsync();
        
        return Results.NoContent();
    }
}
