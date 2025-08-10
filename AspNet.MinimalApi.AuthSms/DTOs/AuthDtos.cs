using System.ComponentModel.DataAnnotations;

namespace AspNet.MinimalApi.AuthSms.DTOs;

public record SendSmsRequest(
    [Required, Phone] string PhoneNumber,
    [Required] string Purpose
);

public record VerifySmsRequest(
    [Required, Phone] string PhoneNumber,
    [Required, StringLength(6, MinimumLength = 6)] string Code
);

public record RegisterRequest(
    [Required, Phone] string PhoneNumber,
    [Required, StringLength(6, MinimumLength = 6)] string Code,
    [Required, StringLength(50)] string FirstName,
    [Required, StringLength(50)] string LastName,
    [Required, MinLength(6)] string Password
);

public record LoginRequest(
    [Required, Phone] string PhoneNumber,
    [Required, StringLength(6, MinimumLength = 6)] string Code
);

public record AuthResponse(
    bool Success,
    string Message,
    string? AccessToken = null,
    string? RefreshToken = null,
    DateTime? ExpiresAt = null,
    UserInfo? User = null
);

public record UserInfo(
    string Id,
    string PhoneNumber,
    string? FirstName,
    string? LastName,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);

public record ApiResponse<T>(
    bool Success,
    string Message,
    T? Data = default
);
