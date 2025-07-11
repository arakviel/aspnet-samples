namespace AspNet.MinimalApi.SimpleAuth.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// DTO для запиту логіну
public record LoginRequest(string Username, string Password);

// DTO для запиту реєстрації
public record RegisterRequest(string Username, string Password, string Email);

// DTO для відповіді
public record AuthResponse(bool Success, string Message, string? Username = null);
