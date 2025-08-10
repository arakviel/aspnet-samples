using System.ComponentModel.DataAnnotations;

namespace AspNet.MinimalApi.AuthSmsIdentity.Models;

public class RegisterRequest
{
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;
}

public class SendSmsCodeRequest
{
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;
}

public class ConfirmPhoneRequest
{
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(10, MinimumLength = 4)]
    public string Code { get; set; } = string.Empty;
}

public class SendSmsLoginCodeRequest
{
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;
}

public class SmsLoginRequest
{
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(10, MinimumLength = 4)]
    public string Code { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponse<T> SuccessResult(T data, string message = "Success")
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

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
}
