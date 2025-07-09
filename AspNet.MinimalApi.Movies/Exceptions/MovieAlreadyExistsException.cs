namespace AspNet.MinimalApi.Movies.Exceptions;

public class MovieAlreadyExistsException : Exception
{
    public string ImdbId { get; }

    public MovieAlreadyExistsException(string imdbId) 
        : base($"Фільм з IMDB ID '{imdbId}' вже існує в базі даних")
    {
        ImdbId = imdbId;
    }

    public MovieAlreadyExistsException(string imdbId, Exception innerException) 
        : base($"Фільм з IMDB ID '{imdbId}' вже існує в базі даних", innerException)
    {
        ImdbId = imdbId;
    }
}
