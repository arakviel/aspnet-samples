namespace AspNet.MinimalApi.BlogWithFront.Domain;

public class PostLike
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public Post Post { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

