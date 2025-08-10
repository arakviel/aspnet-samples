using System.ComponentModel.DataAnnotations;

namespace Auth2Factors.Models;

/// <summary>
/// Базова модель відповіді API
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponse<T> SuccessResult(T? data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}

/// <summary>
/// Модель для реєстрації користувача
/// </summary>
public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Модель для входу користувача
/// </summary>
public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;
}

/// <summary>
/// Модель для підтвердження 2FA коду
/// </summary>
public class Verify2FaRequest
{
    [Required]
    [StringLength(10, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;

    public bool RememberMachine { get; set; } = false;
}

/// <summary>
/// Модель для налаштування TOTP
/// </summary>
public class SetupTotpResponse
{
    public string SharedKey { get; set; } = string.Empty;
    public string AuthenticatorUri { get; set; } = string.Empty;
    public string QrCodeUri { get; set; } = string.Empty;
    public string[] RecoveryCodes { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Модель для підтвердження TOTP налаштування
/// </summary>
public class VerifyTotpRequest
{
    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;
}

/// <summary>
/// Інформація про користувача
/// </summary>
public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public bool HasAuthenticator { get; set; }
    public int RecoveryCodesLeft { get; set; }
    public bool IsMachineRemembered { get; set; }
}

/// <summary>
/// Модель для використання recovery коду
/// </summary>
public class RecoveryCodeRequest
{
    [Required]
    public string RecoveryCode { get; set; } = string.Empty;
}
