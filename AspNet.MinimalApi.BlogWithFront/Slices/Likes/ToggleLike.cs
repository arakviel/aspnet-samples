using AspNet.MinimalApi.BlogWithFront.Common;
using AspNet.MinimalApi.BlogWithFront.Data;
using AspNet.MinimalApi.BlogWithFront.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AspNet.MinimalApi.BlogWithFront.Slices.Likes;

public static class ToggleLike
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/posts/{postId:int}/likes/toggle", Handler)
               .WithTags("Likes")
               .RequireAuthorization();
        }
    }

    public static async Task<IResult> Handler(int postId, AppDbContext db, UserManager<ApplicationUser> userManager, ClaimsPrincipal user)
    {
        var uid = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (uid is null) return Results.Unauthorized();
        var exists = await db.Posts.AnyAsync(p => p.Id == postId);
        if (!exists) return Results.NotFound();

        var like = await db.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == uid);
        if (like is null)
        {
            db.Likes.Add(new PostLike { PostId = postId, UserId = uid });
            await db.SaveChangesAsync();
            var count = await db.Likes.CountAsync(l => l.PostId == postId);
            return Results.Ok(new { liked = true, likes = count });
        }
        else
        {
            db.Likes.Remove(like);
            await db.SaveChangesAsync();
            var count = await db.Likes.CountAsync(l => l.PostId == postId);
            return Results.Ok(new { liked = false, likes = count });
        }
    }
}

