using AspNet.MinimalApi.Movies.Models;

namespace AspNet.MinimalApi.Movies.Repositories;

public interface IMovieRepository
{
    Task<IEnumerable<Movie>> SearchAsync(string title, int page = 1, int pageSize = 10);
    Task<Movie?> GetByIdAsync(int id);
    Task<Movie?> GetByImdbIdAsync(string imdbId);
    Task<Movie> CreateAsync(Movie movie);
    Task<Movie> UpdateAsync(Movie movie);
    Task<bool> DeleteAsync(int id);
    Task<int> GetTotalCountAsync(string title);
}
