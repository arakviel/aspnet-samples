using AspNet.MinimalApi.Movies.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNet.MinimalApi.Movies.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Movie> Movies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Конфігурація для Movie
        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ImdbId).IsUnique();
            entity.HasIndex(e => e.Title);
            entity.Property(e => e.ImdbRating).HasPrecision(3, 1);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var now = DateTime.UtcNow;

        modelBuilder.Entity<Movie>().HasData(
            // Оригінальні 3 фільми
            new Movie
            {
                Id = 1,
                Title = "The Shawshank Redemption",
                Year = "1994",
                ImdbId = "tt0111161",
                Type = "movie",
                Poster = "https://m.media-amazon.com/images/M/MV5BNDE3ODcxYzMtY2YzZC00NmNlLWJiNDMtZDViZWM2MzIxZDYwXkEyXkFqcGdeQXVyNjAwNDUxODI@._V1_SX300.jpg",
                Rated = "R",
                Released = "14 Oct 1994",
                Runtime = "142 min",
                Genre = "Drama",
                Director = "Frank Darabont",
                Writer = "Stephen King, Frank Darabont",
                Actors = "Tim Robbins, Morgan Freeman, Bob Gunton",
                Plot = "Two imprisoned men bond over a number of years, finding solace and eventual redemption through acts of common decency.",
                Language = "English",
                Country = "United States",
                Awards = "Nominated for 7 Oscars",
                ImdbRating = 9.3m,
                ImdbVotes = "2,500,000",
                CreatedAt = now,
                UpdatedAt = now
            },
            new Movie
            {
                Id = 2,
                Title = "The Godfather",
                Year = "1972",
                ImdbId = "tt0068646",
                Type = "movie",
                Poster = "https://m.media-amazon.com/images/M/MV5BM2MyNjYxNmUtYTAwNi00MTYxLWJmNWYtYzZlODY3ZTk3OTFlXkEyXkFqcGdeQXVyNzkwMjQ5NzM@._V1_SX300.jpg",
                Rated = "R",
                Released = "24 Mar 1972",
                Runtime = "175 min",
                Genre = "Crime, Drama",
                Director = "Francis Ford Coppola",
                Writer = "Mario Puzo, Francis Ford Coppola",
                Actors = "Marlon Brando, Al Pacino, James Caan",
                Plot = "The aging patriarch of an organized crime dynasty transfers control of his clandestine empire to his reluctant son.",
                Language = "English, Italian, Latin",
                Country = "United States",
                Awards = "Won 3 Oscars",
                ImdbRating = 9.2m,
                ImdbVotes = "1,800,000",
                CreatedAt = now,
                UpdatedAt = now
            },
            new Movie
            {
                Id = 3,
                Title = "The Dark Knight",
                Year = "2008",
                ImdbId = "tt0468569",
                Type = "movie",
                Poster = "https://m.media-amazon.com/images/M/MV5BMTMxNTMwODM0NF5BMl5BanBnXkFtZTcwODAyMTk2Mw@@._V1_SX300.jpg",
                Rated = "PG-13",
                Released = "18 Jul 2008",
                Runtime = "152 min",
                Genre = "Action, Crime, Drama",
                Director = "Christopher Nolan",
                Writer = "Jonathan Nolan, Christopher Nolan, David S. Goyer",
                Actors = "Christian Bale, Heath Ledger, Aaron Eckhart",
                Plot = "When the menace known as the Joker wreaks havoc and chaos on the people of Gotham, Batman must accept one of the greatest psychological and physical tests of his ability to fight injustice.",
                Language = "English, Mandarin",
                Country = "United States, United Kingdom",
                Awards = "Won 2 Oscars",
                ImdbRating = 9.0m,
                ImdbVotes = "2,600,000",
                CreatedAt = now,
                UpdatedAt = now
            },
            new Movie
            {
                Id = 4,
                Title = "Pulp Fiction",
                Year = "1994",
                ImdbId = "tt0110912",
                Type = "movie",
                Poster = "https://m.media-amazon.com/images/M/MV5BNGNhMDIzZTUtNTBlZi00MTRlLWFjM2ItYzViMjE3YzI5MjljXkEyXkFqcGdeQXVyNzkwMjQ5NzM@._V1_SX300.jpg",
                Rated = "R",
                Released = "14 Oct 1994",
                Runtime = "154 min",
                Genre = "Crime, Drama",
                Director = "Quentin Tarantino",
                Writer = "Quentin Tarantino, Roger Avary",
                Actors = "John Travolta, Uma Thurman, Samuel L. Jackson",
                Plot = "The lives of two mob hitmen, a boxer, a gangster and his wife intertwine in four tales of violence and redemption.",
                Language = "English, Spanish, French",
                Country = "United States",
                Awards = "Won 1 Oscar",
                ImdbRating = 8.9m,
                ImdbVotes = "2,000,000",
                CreatedAt = now,
                UpdatedAt = now
            },
            new Movie
            {
                Id = 5,
                Title = "Forrest Gump",
                Year = "1994",
                ImdbId = "tt0109830",
                Type = "movie",
                Poster = "https://m.media-amazon.com/images/M/MV5BNWIwODRlZTUtY2U3ZS00Yzg1LWJhNzYtMmZiYmEyNmU1NjMzXkEyXkFqcGdeQXVyMTQxNzMzNDI@._V1_SX300.jpg",
                Rated = "PG-13",
                Released = "06 Jul 1994",
                Runtime = "142 min",
                Genre = "Drama, Romance",
                Director = "Robert Zemeckis",
                Writer = "Winston Groom, Eric Roth",
                Actors = "Tom Hanks, Robin Wright, Gary Sinise",
                Plot = "The presidencies of Kennedy and Johnson, the Vietnam War, the Watergate scandal and other historical events unfold from the perspective of an Alabama man with an IQ of 75.",
                Language = "English",
                Country = "United States",
                Awards = "Won 6 Oscars",
                ImdbRating = 8.8m,
                ImdbVotes = "2,100,000",
                CreatedAt = now,
                UpdatedAt = now
            },
            new Movie
            {
                Id = 6,
                Title = "Inception",
                Year = "2010",
                ImdbId = "tt1375666",
                Type = "movie",
                Poster = "https://m.media-amazon.com/images/M/MV5BMjAxMzY3NjcxNF5BMl5BanBnXkFtZTcwNTI5OTM0Mw@@._V1_SX300.jpg",
                Rated = "PG-13",
                Released = "16 Jul 2010",
                Runtime = "148 min",
                Genre = "Action, Sci-Fi, Thriller",
                Director = "Christopher Nolan",
                Writer = "Christopher Nolan",
                Actors = "Leonardo DiCaprio, Marion Cotillard, Tom Hardy",
                Plot = "A thief who steals corporate secrets through the use of dream-sharing technology is given the inverse task of planting an idea into the mind of a C.E.O.",
                Language = "English, Japanese, French",
                Country = "United States, United Kingdom",
                Awards = "Won 4 Oscars",
                ImdbRating = 8.8m,
                ImdbVotes = "2,300,000",
                CreatedAt = now,
                UpdatedAt = now
            },
            new Movie
            {
                Id = 7,
                Title = "The Matrix",
                Year = "1999",
                ImdbId = "tt0133093",
                Type = "movie",
                Poster = "https://m.media-amazon.com/images/M/MV5BNzQzOTk3OTAtNDQ0Zi00ZTVkLWI0MTEtMDllZjNkYzNjNTc4L2ltYWdlXkEyXkFqcGdeQXVyNjU0OTQ0OTY@._V1_SX300.jpg",
                Rated = "R",
                Released = "31 Mar 1999",
                Runtime = "136 min",
                Genre = "Action, Sci-Fi",
                Director = "Lana Wachowski, Lilly Wachowski",
                Writer = "Lilly Wachowski, Lana Wachowski",
                Actors = "Keanu Reeves, Laurence Fishburne, Carrie-Anne Moss",
                Plot = "When a beautiful stranger leads computer hacker Neo to a forbidding underworld, he discovers the shocking truth--the life he knows is the elaborate deception of an evil cyber-intelligence.",
                Language = "English",
                Country = "United States",
                Awards = "Won 4 Oscars",
                ImdbRating = 8.7m,
                ImdbVotes = "1,900,000",
                CreatedAt = now,
                UpdatedAt = now
            },
            new Movie
            {
                Id = 8,
                Title = "Goodfellas",
                Year = "1990",
                ImdbId = "tt0099685",
                Type = "movie",
                Poster = "https://m.media-amazon.com/images/M/MV5BY2NkZjEzMDgtN2RjYy00YzM1LWI4ZmQtMjIwYjFjNmI3ZGEwXkEyXkFqcGdeQXVyNzkwMjQ5NzM@._V1_SX300.jpg",
                Rated = "R",
                Released = "21 Sep 1990",
                Runtime = "146 min",
                Genre = "Biography, Crime, Drama",
                Director = "Martin Scorsese",
                Writer = "Nicholas Pileggi, Martin Scorsese",
                Actors = "Robert De Niro, Ray Liotta, Joe Pesci",
                Plot = "The story of Henry Hill and his life in the mob, covering his relationship with his wife Karen Hill and his mob partners Jimmy Conway and Tommy DeVito.",
                Language = "English, Italian",
                Country = "United States",
                Awards = "Won 1 Oscar",
                ImdbRating = 8.7m,
                ImdbVotes = "1,100,000",
                CreatedAt = now,
                UpdatedAt = now
            },
            new Movie
            {
                Id = 9,
                Title = "Fight Club",
                Year = "1999",
                ImdbId = "tt0137523",
                Type = "movie",
                Poster = "https://m.media-amazon.com/images/M/MV5BNDIzNDU0YzEtYzE5Ni00ZjlkLTk5ZjgtNjM3NWE4YzA3Nzk3XkEyXkFqcGdeQXVyMjUzOTY1NTc@._V1_SX300.jpg",
                Rated = "R",
                Released = "15 Oct 1999",
                Runtime = "139 min",
                Genre = "Drama",
                Director = "David Fincher",
                Writer = "Chuck Palahniuk, Jim Uhls",
                Actors = "Brad Pitt, Edward Norton, Meat Loaf",
                Plot = "An insomniac office worker and a devil-may-care soap maker form an underground fight club that evolves into an anarchist organization.",
                Language = "English",
                Country = "United States",
                Awards = "Nominated for 1 Oscar",
                ImdbRating = 8.8m,
                ImdbVotes = "2,000,000",
                CreatedAt = now,
                UpdatedAt = now
            },
            new Movie
            {
                Id = 10,
                Title = "The Lord of the Rings: The Return of the King",
                Year = "2003",
                ImdbId = "tt0167260",
                Type = "movie",
                Poster = "https://m.media-amazon.com/images/M/MV5BNzA5ZDNlZWMtM2NhNS00NDJjLTk4NDItYTRmY2EwMWI5MTktXkEyXkFqcGdeQXVyNzkwMjQ5NzM@._V1_SX300.jpg",
                Rated = "PG-13",
                Released = "17 Dec 2003",
                Runtime = "201 min",
                Genre = "Action, Adventure, Drama",
                Director = "Peter Jackson",
                Writer = "J.R.R. Tolkien, Fran Walsh, Philippa Boyens",
                Actors = "Elijah Wood, Viggo Mortensen, Ian McKellen",
                Plot = "Gandalf and Aragorn lead the World of Men against Sauron's army to draw his gaze from Frodo and Sam as they approach Mount Doom with the One Ring.",
                Language = "English, Quenya, Old English",
                Country = "New Zealand, United States",
                Awards = "Won 11 Oscars",
                ImdbRating = 9.0m,
                ImdbVotes = "1,800,000",
                CreatedAt = now,
                UpdatedAt = now
            },
            new Movie
            {
                Id = 11,
                Title = "Star Wars: Episode IV - A New Hope",
                Year = "1977",
                ImdbId = "tt0076759",
                Type = "movie",
                Poster = "https://m.media-amazon.com/images/M/MV5BOTA5NjhiOTAtZWM0ZC00MWNhLThiMzEtZDFkOTk2OTU1ZDJkXkEyXkFqcGdeQXVyMTA4NDI1NTQx._V1_SX300.jpg",
                Rated = "PG",
                Released = "25 May 1977",
                Runtime = "121 min",
                Genre = "Action, Adventure, Fantasy",
                Director = "George Lucas",
                Writer = "George Lucas",
                Actors = "Mark Hamill, Harrison Ford, Carrie Fisher",
                Plot = "Luke Skywalker joins forces with a Jedi Knight, a cocky pilot, a Wookiee and two droids to save the galaxy from the Empire's world-destroying battle station.",
                Language = "English",
                Country = "United States",
                Awards = "Won 6 Oscars",
                ImdbRating = 8.6m,
                ImdbVotes = "1,400,000",
                CreatedAt = now,
                UpdatedAt = now
            },
            new Movie
            {
                Id = 12,
                Title = "Interstellar",
                Year = "2014",
                ImdbId = "tt0816692",
                Type = "movie",
                Poster = "https://m.media-amazon.com/images/M/MV5BZjdkOTU3MDktN2IxOS00OGEyLWFmMjktY2FiMmZkNWIyODZiXkEyXkFqcGdeQXVyMTMxODk2OTU@._V1_SX300.jpg",
                Rated = "PG-13",
                Released = "07 Nov 2014",
                Runtime = "169 min",
                Genre = "Adventure, Drama, Sci-Fi",
                Director = "Christopher Nolan",
                Writer = "Jonathan Nolan, Christopher Nolan",
                Actors = "Matthew McConaughey, Anne Hathaway, Jessica Chastain",
                Plot = "A team of explorers travel through a wormhole in space in an attempt to ensure humanity's survival.",
                Language = "English",
                Country = "United States, United Kingdom, Canada",
                Awards = "Won 1 Oscar",
                ImdbRating = 8.6m,
                ImdbVotes = "1,700,000",
                CreatedAt = now,
                UpdatedAt = now
            },
            new Movie
            {
                Id = 13,
                Title = "Parasite",
                Year = "2019",
                ImdbId = "tt6751668",
                Type = "movie",
                Poster = "https://m.media-amazon.com/images/M/MV5BYWZjMjk3ZTItODQ2ZC00NTY5LWE0ZDYtZTI3MjcwN2Q5NTVkXkEyXkFqcGdeQXVyODk4OTc3MTY@._V1_SX300.jpg",
                Rated = "R",
                Released = "30 May 2019",
                Runtime = "132 min",
                Genre = "Comedy, Drama, Thriller",
                Director = "Bong Joon Ho",
                Writer = "Bong Joon Ho, Han Jin-won",
                Actors = "Kang-ho Song, Sun-kyun Lee, Yeo-jeong Jo",
                Plot = "A poor family schemes to become employed by a wealthy family and infiltrate their household by posing as unrelated, highly qualified individuals.",
                Language = "Korean, English",
                Country = "South Korea",
                Awards = "Won 4 Oscars",
                ImdbRating = 8.5m,
                ImdbVotes = "800,000",
                CreatedAt = now,
                UpdatedAt = now
            },
            new Movie
            {
                Id = 14,
                Title = "The Avengers",
                Year = "2012",
                ImdbId = "tt0848228",
                Type = "movie",
                Poster = "https://m.media-amazon.com/images/M/MV5BNDYxNjQyMjAtNTdiOS00NGYwLWFmNTAtNThmYjU5ZGI2YTI1XkEyXkFqcGdeQXVyMTMxODk2OTU@._V1_SX300.jpg",
                Rated = "PG-13",
                Released = "04 May 2012",
                Runtime = "143 min",
                Genre = "Action, Adventure, Sci-Fi",
                Director = "Joss Whedon",
                Writer = "Joss Whedon, Zak Penn",
                Actors = "Robert Downey Jr., Chris Evans, Scarlett Johansson",
                Plot = "Earth's mightiest heroes must come together and learn to fight as a team if they are going to stop the mischievous Loki and his alien army from enslaving humanity.",
                Language = "English, Russian",
                Country = "United States",
                Awards = "Nominated for 1 Oscar",
                ImdbRating = 8.0m,
                ImdbVotes = "1,300,000",
                CreatedAt = now,
                UpdatedAt = now
            },
            new Movie
            {
                Id = 15,
                Title = "Titanic",
                Year = "1997",
                ImdbId = "tt0120338",
                Type = "movie",
                Poster = "https://m.media-amazon.com/images/M/MV5BMDdmZGU3NDQtY2E5My00ZTliLWIzOTUtMTY4ZGI1YjdiNjk3XkEyXkFqcGdeQXVyNTA4NzY1MzY@._V1_SX300.jpg",
                Rated = "PG-13",
                Released = "19 Dec 1997",
                Runtime = "194 min",
                Genre = "Drama, Romance",
                Director = "James Cameron",
                Writer = "James Cameron",
                Actors = "Leonardo DiCaprio, Kate Winslet, Billy Zane",
                Plot = "A seventeen-year-old aristocrat falls in love with a kind but poor artist aboard the luxurious, ill-fated R.M.S. Titanic.",
                Language = "English, Swedish, Italian",
                Country = "United States, Mexico",
                Awards = "Won 11 Oscars",
                ImdbRating = 7.9m,
                ImdbVotes = "1,200,000",
                CreatedAt = now,
                UpdatedAt = now
            }
        );
    }
}
