namespace AspNet.MinimalApi.Blog.Models
{
    public class Post
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public string AuthorId { get; set; }
        public ApplicationUser Author { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public ICollection<Comment> Comments { get; set; }
    }
}
