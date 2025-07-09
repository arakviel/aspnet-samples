using System.ComponentModel.DataAnnotations;

namespace AspNet.MinimalApi.Movies.Models;

public class Movie
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(4)]
    public string Year { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string ImdbId { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Poster { get; set; } = string.Empty;
    
    // Детальна інформація про фільм
    [MaxLength(10)]
    public string Rated { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string Released { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string Runtime { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Genre { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string Director { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Writer { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Actors { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Plot { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Language { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Awards { get; set; } = string.Empty;
    
    public decimal ImdbRating { get; set; }
    
    [MaxLength(20)]
    public string ImdbVotes { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
