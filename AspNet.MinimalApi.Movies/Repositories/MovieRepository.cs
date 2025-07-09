using AspNet.MinimalApi.Movies.Data;
using AspNet.MinimalApi.Movies.Exceptions;
using AspNet.MinimalApi.Movies.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNet.MinimalApi.Movies.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MovieRepository> _logger;

    public MovieRepository(ApplicationDbContext context, ILogger<MovieRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Movie>> SearchAsync(string title, int page = 1, int pageSize = 10)
    {
        _logger.LogInformation("Пошук фільмів: Title={Title}, Page={Page}, PageSize={PageSize}",
            title ?? "всі", page, pageSize);

        try
        {
            var query = _context.Movies.AsQueryable();

            // Якщо title не порожній, фільтруємо по назві
            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(m => m.Title.Contains(title));
                _logger.LogDebug("Застосовано фільтр по назві: {Title}", title);
            }

            var movies = await query
                .OrderBy(m => m.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Знайдено {Count} фільмів для запиту: {Title}",
                movies.Count(), title ?? "всі");

            return movies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при пошуку фільмів: Title={Title}, Page={Page}", title, page);
            throw;
        }
    }

    public async Task<Movie?> GetByIdAsync(int id)
    {
        _logger.LogDebug("Пошук фільму за ID: {Id}", id);

        try
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie != null)
            {
                _logger.LogDebug("Фільм знайдено: {Title} (ID: {Id})", movie.Title, id);
            }
            else
            {
                _logger.LogWarning("Фільм з ID {Id} не знайдено", id);
            }

            return movie;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при пошуку фільму за ID: {Id}", id);
            throw;
        }
    }

    public async Task<Movie?> GetByImdbIdAsync(string imdbId)
    {
        _logger.LogDebug("Пошук фільму за IMDB ID: {ImdbId}", imdbId);

        try
        {
            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.ImdbId == imdbId);

            if (movie != null)
            {
                _logger.LogDebug("Фільм знайдено: {Title} (IMDB: {ImdbId})", movie.Title, imdbId);
            }
            else
            {
                _logger.LogWarning("Фільм з IMDB ID {ImdbId} не знайдено", imdbId);
            }

            return movie;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при пошуку фільму за IMDB ID: {ImdbId}", imdbId);
            throw;
        }
    }

    public async Task<Movie> CreateAsync(Movie movie)
    {
        _logger.LogInformation("Створення нового фільму: {Title} (IMDB: {ImdbId})",
            movie.Title, movie.ImdbId);

        try
        {
            movie.CreatedAt = DateTime.UtcNow;
            movie.UpdatedAt = DateTime.UtcNow;

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Фільм успішно створено: {Title} з ID {Id}",
                movie.Title, movie.Id);

            return movie;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при створенні фільму: {Title} (IMDB: {ImdbId})",
                movie.Title, movie.ImdbId);
            throw;
        }
    }

    public async Task<Movie> UpdateAsync(Movie movie)
    {
        _logger.LogInformation("Оновлення фільму: {Title} (ID: {Id})", movie.Title, movie.Id);

        try
        {
            movie.UpdatedAt = DateTime.UtcNow;

            _context.Movies.Update(movie);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Фільм успішно оновлено: {Title} (ID: {Id})",
                movie.Title, movie.Id);

            return movie;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при оновленні фільму: {Title} (ID: {Id})",
                movie.Title, movie.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Видалення фільму з ID: {Id}", id);

        try
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                _logger.LogWarning("Спроба видалити неіснуючий фільм з ID: {Id}", id);
                return false;
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Фільм успішно видалено: {Title} (ID: {Id})",
                movie.Title, id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при видаленні фільму з ID: {Id}", id);
            throw;
        }
    }

    public async Task<int> GetTotalCountAsync(string title)
    {
        _logger.LogDebug("Підрахунок загальної кількості фільмів для: {Title}", title ?? "всі");

        try
        {
            var query = _context.Movies.AsQueryable();

            // Якщо title не порожній, фільтруємо по назві
            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(m => m.Title.Contains(title));
            }

            var count = await query.CountAsync();

            _logger.LogDebug("Загальна кількість фільмів: {Count} для запиту: {Title}",
                count, title ?? "всі");

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при підрахунку фільмів для: {Title}", title);
            throw;
        }
    }
}