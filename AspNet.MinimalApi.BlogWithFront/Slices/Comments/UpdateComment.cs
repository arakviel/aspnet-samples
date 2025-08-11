using AspNet.MinimalApi.BlogWithFront.Common;
using AspNet.MinimalApi.BlogWithFront.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AspNet.MinimalApi.BlogWithFront.Slices.Comments;

/// <summary>
/// Оновлення коментаря - доступно тільки автору або адміну
/// </summary>
public static class UpdateComment
{
    public record Request(string Content);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("/comments/{id:int}", Handler)
               .WithTags("Comments")
               .RequireAuthorization();
        }
    }

    public static async Task<IResult> Handler(int id, Request request, AppDbContext db, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Results.Unauthorized();

        var isAdmin = user.IsInRole("Admin");
        
        var comment = await db.Comments.FirstOrDefaultAsync(c => c.Id == id);
        if (comment is null) return Results.NotFound();

        // Перевіряємо, чи користувач є автором коментаря або адміном
        if (comment.AuthorId != userId && !isAdmin)
            return Results.Forbid();

        comment.Content = request.Content;
        
        await db.SaveChangesAsync();
        
        return Results.Ok(new { comment.Id, comment.Content, comment.CreatedDate });
    }
}
