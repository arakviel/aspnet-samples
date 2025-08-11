using AspNet.MinimalApi.BlogWithFront.Common;
using AspNet.MinimalApi.BlogWithFront.Data;
using Microsoft.EntityFrameworkCore;

namespace AspNet.MinimalApi.BlogWithFront.Slices.Admin.Posts;

/// <summary>
/// Отримання всіх постів для адмін-панелі з додатковою інформацією
/// </summary>
public static class GetAllPosts
{
    public record Response(int Id, string Title, string Content, DateTime CreatedDate, 
        string AuthorName, int CommentsCount, int LikesCount);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/admin/posts", Handler)
                .WithTags("Admin")
                .RequireAuthorization("RequireAdminRole");
        }
    }

    public static async Task<IResult> Handler(AppDbContext db)
    {
        var posts = await db.Posts
            .Include(p => p.Author)
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .OrderByDescending(p => p.CreatedDate)
            .Select(p => new Response(
                p.Id, 
                p.Title, 
                p.Content, 
                p.CreatedDate,
                p.Author.UserName ?? p.Author.Email ?? "Невідомий",
                p.Comments.Count,
                p.Likes.Count
            ))
            .ToListAsync();

        return Results.Ok(posts);
    }
}
