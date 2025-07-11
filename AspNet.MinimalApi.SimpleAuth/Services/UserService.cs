using AspNet.MinimalApi.SimpleAuth.Models;
using BCrypt.Net;

namespace AspNet.MinimalApi.SimpleAuth.Services;

public interface IUserService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<IEnumerable<User>> GetAllUsersAsync();
}

public class UserService : IUserService
{
    // В реальному додатку це була б база даних
    private readonly List<User> _users = new();
    private int _nextId = 1;

    public UserService()
    {
        // Додаємо тестових користувачів
        SeedTestUsers();
    }

    public Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Перевіряємо чи користувач вже існує
        if (_users.Any(u => u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase)))
        {
            return Task.FromResult(new AuthResponse(false, "Користувач з таким ім'ям вже існує"));
        }

        // Перевіряємо чи email вже використовується
        if (_users.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return Task.FromResult(new AuthResponse(false, "Користувач з таким email вже існує"));
        }

        // Валідація
        if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length < 3)
        {
            return Task.FromResult(new AuthResponse(false, "Ім'я користувача має бути не менше 3 символів"));
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            return Task.FromResult(new AuthResponse(false, "Пароль має бути не менше 6 символів"));
        }

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
        {
            return Task.FromResult(new AuthResponse(false, "Введіть коректний email"));
        }

        // Створюємо нового користувача
        var user = new User
        {
            Id = _nextId++,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        _users.Add(user);

        return Task.FromResult(new AuthResponse(true, "Користувач успішно зареєстрований", user.Username));
    }

    public Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = _users.FirstOrDefault(u => 
            u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            return Task.FromResult(new AuthResponse(false, "Неправильне ім'я користувача або пароль"));
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Task.FromResult(new AuthResponse(false, "Неправильне ім'я користувача або пароль"));
        }

        return Task.FromResult(new AuthResponse(true, "Успішний вхід", user.Username));
    }

    public Task<User?> GetUserByUsernameAsync(string username)
    {
        var user = _users.FirstOrDefault(u => 
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<IEnumerable<User>> GetAllUsersAsync()
    {
        // Повертаємо користувачів без паролів
        var usersWithoutPasswords = _users.Select(u => new User
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            CreatedAt = u.CreatedAt,
            PasswordHash = "***" // Приховуємо хеш пароля
        });

        return Task.FromResult(usersWithoutPasswords);
    }

    private void SeedTestUsers()
    {
        // Додаємо тестових користувачів
        var testUsers = new[]
        {
            new { Username = "admin", Password = "admin123", Email = "admin@example.com" },
            new { Username = "user", Password = "user123", Email = "user@example.com" },
            new { Username = "test", Password = "test123", Email = "test@example.com" }
        };

        foreach (var testUser in testUsers)
        {
            _users.Add(new User
            {
                Id = _nextId++,
                Username = testUser.Username,
                Email = testUser.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(testUser.Password),
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
