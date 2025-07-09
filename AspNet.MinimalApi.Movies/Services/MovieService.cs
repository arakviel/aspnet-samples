using AspNet.MinimalApi.Movies.Dtos;
using AspNet.MinimalApi.Movies.Exceptions;
using AspNet.MinimalApi.Movies.Models;
using AspNet.MinimalApi.Movies.Repositories;

namespace AspNet.MinimalApi.Movies.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly ILogger<MovieService> _logger;
    private const int PageSize = 10;

    public MovieService(IMovieRepository movieRepository, ILogger<MovieService> logger)
    {
        _movieRepository = movieRepository;
        _logger = logger;
    }

    public async Task<MovieSearchResponseDto> SearchAsync(string title, int page = 1)
    {
        _logger.LogInformation("Запит на пошук фільмів: Title={Title}, Page={Page}",
            title ?? "всі", page);

        if (page < 1)
        {
            _logger.LogWarning("Некоректний номер сторінки: {Page}", page);
            throw new ValidationException("page", "Номер сторінки повинен бути більше 0");
        }

        try
        {
            var movies = await _movieRepository.SearchAsync(title, page, PageSize);
            var totalCount = await _movieRepository.GetTotalCountAsync(title);

            var searchItems = movies.Select(m => new MovieSearchItemDto
            {
                Title = m.Title,
                Year = m.Year,
                ImdbID = m.ImdbId,
                Type = m.Type,
                Poster = m.Poster
            }).ToList();

            var response = new MovieSearchResponseDto
            {
                Search = searchItems,
                TotalResults = totalCount.ToString(),
                Response = searchItems.Any() ? "True" : "False"
            };

            _logger.LogInformation("Пошук завершено: знайдено {Count} з {Total} фільмів",
                searchItems.Count, totalCount);

            return response;
        }
        catch (Exception ex) when (!(ex is ValidationException))
        {
            _logger.LogError(ex, "Помилка при пошуку фільмів: Title={Title}, Page={Page}", title, page);
            throw;
        }
    }

    public async Task<Movie?> GetDetailsAsync(string imdbId)
    {
        _logger.LogInformation("Запит на отримання деталей фільму: ImdbId={ImdbId}", imdbId);

        if (string.IsNullOrWhiteSpace(imdbId))
        {
            _logger.LogWarning("Спроба отримати деталі фільму з порожнім IMDB ID");
            throw new ValidationException("imdbId", "IMDB ID не може бути порожнім");
        }

        try
        {
            var movie = await _movieRepository.GetByImdbIdAsync(imdbId);

            if (movie == null)
            {
                _logger.LogWarning("Фільм з IMDB ID {ImdbId} не знайдено", imdbId);
                throw new MovieNotFoundException(imdbId);
            }

            _logger.LogInformation("Деталі фільму отримано: {Title} (IMDB: {ImdbId})",
                movie.Title, imdbId);

            return movie;
        }
        catch (Exception ex) when (!(ex is MovieNotFoundException || ex is ValidationException))
        {
            _logger.LogError(ex, "Помилка при отриманні деталей фільму: ImdbId={ImdbId}", imdbId);
            throw;
        }
    }

    public async Task<Movie> CreateAsync(Movie movie)
    {
        _logger.LogInformation("Запит на створення фільму: {Title} (IMDB: {ImdbId})",
            movie.Title, movie.ImdbId);

        // Валідація
        ValidateMovie(movie);

        try
        {
            // Перевіряємо, чи не існує вже фільм з таким IMDB ID
            var existingMovie = await _movieRepository.GetByImdbIdAsync(movie.ImdbId);
            if (existingMovie != null)
            {
                _logger.LogWarning("Спроба створити фільм, який вже існує: IMDB ID {ImdbId}", movie.ImdbId);
                throw new MovieAlreadyExistsException(movie.ImdbId);
            }

            var createdMovie = await _movieRepository.CreateAsync(movie);

            _logger.LogInformation("Фільм успішно створено: {Title} з ID {Id}",
                createdMovie.Title, createdMovie.Id);

            return createdMovie;
        }
        catch (Exception ex) when (!(ex is MovieAlreadyExistsException || ex is ValidationException))
        {
            _logger.LogError(ex, "Помилка при створенні фільму: {Title} (IMDB: {ImdbId})",
                movie.Title, movie.ImdbId);
            throw;
        }
    }

    public async Task<Movie?> UpdateAsync(int id, Movie movie)
    {
        _logger.LogInformation("Запит на оновлення фільму з ID: {Id}", id);

        if (id <= 0)
        {
            _logger.LogWarning("Некоректний ID для оновлення: {Id}", id);
            throw new ValidationException("id", "ID повинен бути більше 0");
        }

        ValidateMovie(movie);

        try
        {
            var existingMovie = await _movieRepository.GetByIdAsync(id);
            if (existingMovie == null)
            {
                _logger.LogWarning("Спроба оновити неіснуючий фільм з ID: {Id}", id);
                throw new MovieNotFoundByIdException(id);
            }

            // Оновлюємо поля
            existingMovie.Title = movie.Title;
            existingMovie.Year = movie.Year;
            existingMovie.Type = movie.Type;
            existingMovie.Poster = movie.Poster;
            existingMovie.Rated = movie.Rated;
            existingMovie.Released = movie.Released;
            existingMovie.Runtime = movie.Runtime;
            existingMovie.Genre = movie.Genre;
            existingMovie.Director = movie.Director;
            existingMovie.Writer = movie.Writer;
            existingMovie.Actors = movie.Actors;
            existingMovie.Plot = movie.Plot;
            existingMovie.Language = movie.Language;
            existingMovie.Country = movie.Country;
            existingMovie.Awards = movie.Awards;
            existingMovie.ImdbRating = movie.ImdbRating;
            existingMovie.ImdbVotes = movie.ImdbVotes;

            var updatedMovie = await _movieRepository.UpdateAsync(existingMovie);

            _logger.LogInformation("Фільм успішно оновлено: {Title} (ID: {Id})",
                updatedMovie.Title, id);

            return updatedMovie;
        }
        catch (Exception ex) when (!(ex is MovieNotFoundByIdException || ex is ValidationException))
        {
            _logger.LogError(ex, "Помилка при оновленні фільму з ID: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Запит на видалення фільму з ID: {Id}", id);

        if (id <= 0)
        {
            _logger.LogWarning("Некоректний ID для видалення: {Id}", id);
            throw new ValidationException("id", "ID повинен бути більше 0");
        }

        try
        {
            var deleted = await _movieRepository.DeleteAsync(id);

            if (!deleted)
            {
                _logger.LogWarning("Спроба видалити неіснуючий фільм з ID: {Id}", id);
                throw new MovieNotFoundByIdException(id);
            }

            _logger.LogInformation("Фільм успішно видалено з ID: {Id}", id);
            return true;
        }
        catch (Exception ex) when (!(ex is MovieNotFoundByIdException || ex is ValidationException))
        {
            _logger.LogError(ex, "Помилка при видаленні фільму з ID: {Id}", id);
            throw;
        }
    }

    private void ValidateMovie(Movie movie)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(movie.Title))
            errors.Add("title", new[] { "Назва фільму є обов'язковою" });

        if (string.IsNullOrWhiteSpace(movie.Year))
            errors.Add("year", new[] { "Рік випуску є обов'язковим" });

        if (string.IsNullOrWhiteSpace(movie.ImdbId))
            errors.Add("imdbId", new[] { "IMDB ID є обов'язковим" });

        if (movie.ImdbRating < 0 || movie.ImdbRating > 10)
            errors.Add("imdbRating", new[] { "Рейтинг IMDB повинен бути від 0 до 10" });

        if (errors.Any())
        {
            _logger.LogWarning("Помилка валідації фільму: {Title}, Errors: {@Errors}",
                movie.Title, errors);
            throw new ValidationException("Помилка валідації фільму", errors);
        }
    }
}
