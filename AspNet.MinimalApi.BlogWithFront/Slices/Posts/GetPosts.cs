using AspNet.MinimalApi.BlogWithFront.Common;
using AspNet.MinimalApi.BlogWithFront.Data;
using Microsoft.EntityFrameworkCore;

namespace AspNet.MinimalApi.BlogWithFront.Slices.Posts;

public static class GetPosts
{
    public record Response(int Id, string Title, string Content, DateTime CreatedDate, int CommentsCount, int LikesCount);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/posts", Handler)
                .WithTags("Posts")
                .WithOpenApi();
            app.MapGet("/posts/{id:int}", HandlerById)
                .WithTags("Posts")
                .WithOpenApi();
        }
    }

    public static async Task<IResult> Handler(AppDbContext db)
    {
        var items = await db.Posts
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .OrderByDescending(p => p.CreatedDate)
            .Select(p => new Response(p.Id, p.Title, p.Content, p.CreatedDate, p.Comments.Count, p.Likes.Count))
            .ToListAsync();
        return Results.Ok(items);
    }

    public static async Task<IResult> HandlerById(int id, AppDbContext db)
    {
        var post = await db.Posts
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Content,
                p.CreatedDate,
                Comments = p.Comments.OrderBy(c => c.CreatedDate).Select(c => new { c.Id, c.Content, c.CreatedDate, Author = c.Author.UserName }),
                LikesCount = p.Likes.Count
            })
            .FirstOrDefaultAsync();
        return post is null ? Results.NotFound() : Results.Ok(post);
    }
}

