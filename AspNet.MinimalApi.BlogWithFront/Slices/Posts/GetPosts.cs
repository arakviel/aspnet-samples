using AspNet.MinimalApi.BlogWithFront.Common;
using AspNet.MinimalApi.BlogWithFront.Data;
using Microsoft.EntityFrameworkCore;

namespace AspNet.MinimalApi.BlogWithFront.Slices.Posts;

public static class GetPosts
{
    public record Response(int Id, string Title, string Content, string? Summary, DateTime CreatedDate, DateTime? UpdatedDate, bool IsPublished, string[] Tags, string AuthorName, int CommentsCount, int LikesCount);

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

    public static async Task<IResult> Handler(AppDbContext db, int page = 1, int pageSize = 10)
    {
        var items = await db.Posts
            .Include(p => p.Author)
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new Response(
                p.Id,
                p.Title,
                p.Content,
                p.Summary,
                p.CreatedDate,
                p.UpdatedDate,
                p.IsPublished,
                !string.IsNullOrEmpty(p.Tags) ? p.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries) : new string[0],
                p.Author.UserName ?? p.Author.Email ?? "Unknown",
                p.Comments.Count,
                p.Likes.Count))
            .ToListAsync();

        var totalCount = await db.Posts.Where(p => p.IsPublished).CountAsync();

        return Results.Ok(new
        {
            Success = true,
            Data = new
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            }
        });
    }

    public static async Task<IResult> HandlerById(int id, AppDbContext db)
    {
        var post = await db.Posts
            .Include(p => p.Author)
            .Include(p => p.Comments)
                .ThenInclude(c => c.Author)
            .Include(p => p.Likes)
            .Where(p => p.Id == id && p.IsPublished)
            .Select(p => new
            {
                p.Id,
                p.Title,
                p.Content,
                p.Summary,
                p.CreatedDate,
                p.UpdatedDate,
                p.IsPublished,
                Tags = !string.IsNullOrEmpty(p.Tags) ? p.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries) : new string[0],
                AuthorName = p.Author.UserName ?? p.Author.Email ?? "Unknown",
                Comments = p.Comments.OrderBy(c => c.CreatedDate).Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.CreatedDate,
                    AuthorName = c.Author.UserName ?? c.Author.Email ?? "Unknown"
                }),
                CommentsCount = p.Comments.Count,
                LikesCount = p.Likes.Count
            })
            .FirstOrDefaultAsync();

        if (post is null)
            return Results.NotFound(new { Message = "Пост не знайдено" });

        return Results.Ok(new { Success = true, Data = post });
    }
}

