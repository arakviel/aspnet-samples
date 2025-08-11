namespace AspNet.MinimalApi.BlogWithFront.Domain;

public class Post
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public string AuthorId { get; set; } = default!;
    public ApplicationUser Author { get; set; } = default!;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
}

