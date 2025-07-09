namespace AspNet.MinimalApi.Movies.Exceptions;

public class MovieNotFoundException : Exception
{
    public string ImdbId { get; }

    public MovieNotFoundException(string imdbId) 
        : base($"Фільм з IMDB ID '{imdbId}' не знайдено")
    {
        ImdbId = imdbId;
    }

    public MovieNotFoundException(string imdbId, Exception innerException) 
        : base($"Фільм з IMDB ID '{imdbId}' не знайдено", innerException)
    {
        ImdbId = imdbId;
    }
}

public class MovieNotFoundByIdException : Exception
{
    public int Id { get; }

    public MovieNotFoundByIdException(int id) 
        : base($"Фільм з ID '{id}' не знайдено")
    {
        Id = id;
    }

    public MovieNotFoundByIdException(int id, Exception innerException) 
        : base($"Фільм з ID '{id}' не знайдено", innerException)
    {
        Id = id;
    }
}
