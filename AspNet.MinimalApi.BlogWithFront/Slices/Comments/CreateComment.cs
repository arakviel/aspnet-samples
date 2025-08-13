using AspNet.MinimalApi.BlogWithFront.Common;
using AspNet.MinimalApi.BlogWithFront.Data;
using AspNet.MinimalApi.BlogWithFront.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AspNet.MinimalApi.BlogWithFront.Slices.Comments;

public static class CreateComment
{
    public record CreateCommentRequest(string Content);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/posts/{postId:int}/comments", Handler)
               .WithTags("Comments")
               .RequireAuthorization("MinimumRegistrationDays");
        }
    }

    public static async Task<IResult> Handler(int postId, CreateCommentRequest request, AppDbContext db, UserManager<ApplicationUser> userManager, ClaimsPrincipal user)
    {
        var uid = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (uid is null) return Results.Unauthorized();
        var author = await userManager.FindByIdAsync(uid);
        if (author is null) return Results.Unauthorized();

        var exists = await db.Posts.AnyAsync(p => p.Id == postId);
        if (!exists) return Results.NotFound();

        var comment = new Comment { PostId = postId, Content = request.Content, AuthorId = author.Id, Author = author };
        db.Comments.Add(comment);
        await db.SaveChangesAsync();
        return Results.Created($"/posts/{postId}/comments/{comment.Id}", new { comment.Id, comment.Content });
    }
}

