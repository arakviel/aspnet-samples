namespace AspNet.MinimalApi.Movies.Dtos;

public class MovieSearchResponseDto
{
    public List<MovieSearchItemDto> Search { get; set; } = new();
    public string TotalResults { get; set; } = "0";
    public string Response { get; set; } = "True";
}

public class MovieSearchItemDto
{
    public string Title { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string ImdbID { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Poster { get; set; } = string.Empty;
}
