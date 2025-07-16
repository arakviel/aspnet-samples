using System.ComponentModel.DataAnnotations;

namespace AspNet.MinimalApi.ExtendedJwtAuth.Models;

/// <summary>
///     Модель запиту для реєстрації користувача
/// </summary>
public class RegisterRequest
{
    [Required(ErrorMessage = "Email є обов'язковим")]
    [EmailAddress(ErrorMessage = "Невірний формат email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Пароль є обов'язковим")]
    [MinLength(6, ErrorMessage = "Пароль повинен містити мінімум 6 символів")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Підтвердження пароля є обов'язковим")]
    [Compare("Password", ErrorMessage = "Паролі не співпадають")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
}

/// <summary>
///     Модель запиту для входу в систему
/// </summary>
public class LoginRequest
{
    [Required(ErrorMessage = "Email є обов'язковим")]
    [EmailAddress(ErrorMessage = "Невірний формат email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Пароль є обов'язковим")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;
}

/// <summary>
///     Модель відповіді після успішної аутентифікації
/// </summary>
public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserInfo User { get; set; } = null!;
}

/// <summary>
///     Інформація про користувача для відповіді
/// </summary>
public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
///     Модель запиту для оновлення токена
/// </summary>
public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Refresh токен є обов'язковим")]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
///     Модель запиту для оновлення профілю
/// </summary>
public class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
}

/// <summary>
///     Модель запиту для зміни пароля
/// </summary>
public class ChangePasswordRequest
{
    [Required(ErrorMessage = "Поточний пароль є обов'язковим")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Новий пароль є обов'язковим")]
    [MinLength(6, ErrorMessage = "Пароль повинен містити мінімум 6 символів")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Підтвердження нового пароля є обов'язковим")]
    [Compare("NewPassword", ErrorMessage = "Паролі не співпадають")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

/// <summary>
///     Стандартна модель відповіді API
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
///     Модель відповіді без даних
/// </summary>
public class ApiResponse : ApiResponse<object>
{
}