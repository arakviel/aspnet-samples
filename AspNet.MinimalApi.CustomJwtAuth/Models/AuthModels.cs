namespace AspNet.MinimalApi.CustomJwtAuth.Models;

/// <summary>
/// DTO для запиту входу в систему.
/// </summary>
/// <param name="Username">Ім'я користувача або електронна пошта</param>
/// <param name="Password">Пароль користувача</param>
public record LoginRequest(string Username, string Password);

/// <summary>
/// DTO для запиту реєстрації нового користувача.
/// </summary>
/// <param name="Username">Ім'я користувача (має бути унікальним)</param>
/// <param name="Password">Пароль користувача</param>
/// <param name="Email">Електронна пошта (має бути унікальною)</param>
public record RegisterRequest(string Username, string Password, string Email);

/// <summary>
/// DTO для відповіді на запити аутентифікації.
/// </summary>
/// <param name="Success">Чи успішна операція</param>
/// <param name="Message">Повідомлення про результат операції</param>
/// <param name="AccessToken">JWT токен доступу (якщо успішно)</param>
/// <param name="RefreshToken">Токен оновлення (якщо успішно)</param>
/// <param name="ExpiresAt">Час закінчення дії токена доступу</param>
/// <param name="User">Інформація про користувача (без чутливих даних)</param>
public record AuthResponse(
    bool Success,
    string Message,
    string? AccessToken = null,
    string? RefreshToken = null,
    DateTime? ExpiresAt = null,
    UserInfo? User = null
);

/// <summary>
/// DTO для публічної інформації про користувача (без чутливих даних).
/// </summary>
/// <param name="Id">Ідентифікатор користувача</param>
/// <param name="Username">Ім'я користувача</param>
/// <param name="Email">Електронна пошта</param>
/// <param name="Role">Роль користувача</param>
/// <param name="CreatedAt">Дата створення облікового запису</param>
/// <param name="LastLoginAt">Дата останнього входу</param>
public record UserInfo(
    int Id,
    string Username,
    string Email,
    string Role,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);

/// <summary>
/// DTO для запиту оновлення токена.
/// </summary>
/// <param name="RefreshToken">Токен оновлення</param>
public record RefreshTokenRequest(string RefreshToken);

/// <summary>
/// DTO для відповіді з інформацією про токен.
/// </summary>
/// <param name="AccessToken">JWT токен доступу</param>
/// <param name="RefreshToken">Токен оновлення</param>
/// <param name="ExpiresAt">Час закінчення дії токена доступу</param>
/// <param name="TokenType">Тип токена (зазвичай "Bearer")</param>
public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string TokenType = "Bearer"
);
