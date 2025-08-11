using Microsoft.AspNetCore.Identity;
using AspNet.MinimalApi.BlogWithFront.Models;

namespace AspNet.MinimalApi.BlogWithFront.Domain;

public class ApplicationUser : IdentityUser
{
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Навігаційна властивість до постів користувача
    /// </summary>
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    /// <summary>
    /// Навігаційна властивість до коментарів користувача
    /// </summary>
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    /// <summary>
    /// Навігаційна властивість до лайків користувача
    /// </summary>
    public virtual ICollection<PostLike> Likes { get; set; } = new List<PostLike>();

    /// <summary>
    /// Навігаційна властивість до рефреш токенів користувача
    /// </summary>
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

