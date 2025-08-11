namespace AspNet.MinimalApi.BlogWithFront.Domain;

public class Comment
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public int PostId { get; set; }
    public Post Post { get; set; } = default!;
    public string AuthorId { get; set; } = default!;
    public ApplicationUser Author { get; set; } = default!;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

