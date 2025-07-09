using AspNet.MinimalApi.Movies.Dtos;
using AspNet.MinimalApi.Movies.Models;

namespace AspNet.MinimalApi.Movies.Services;

public interface IMovieService
{
    Task<MovieSearchResponseDto> SearchAsync(string title, int page = 1);
    Task<Movie?> GetDetailsAsync(string imdbId);
    Task<Movie> CreateAsync(Movie movie);
    Task<Movie?> UpdateAsync(int id, Movie movie);
    Task<bool> DeleteAsync(int id);
}
