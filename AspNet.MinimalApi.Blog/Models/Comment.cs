namespace AspNet.MinimalApi.Blog.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public required string Content { get; set; }
        public int PostId { get; set; }
        public Post Post { get; set; }
        public string AuthorId { get; set; }
        public ApplicationUser Author { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
