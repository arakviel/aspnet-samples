namespace AspNet.MinimalApi.CustomJwtAuth.Authentication;

/// <summary>
/// Опції конфігурації для кастомної JWT аутентифікації.
/// Містить всі необхідні параметри для генерації та валідації JWT токенів.
/// </summary>
public class JwtAuthenticationOptions
{
    /// <summary>
    /// Секретний ключ для підпису JWT токенів.
    /// Має бути достатньо довгим та складним для забезпечення безпеки.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Видавець токена (Issuer).
    /// Ідентифікує сервер, який видав токен.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Аудиторія токена (Audience).
    /// Ідентифікує призначених отримувачів токена.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Час дії токена доступу в хвилинах.
    /// Після закінчення цього часу токен стає недійсним.
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Час дії токена оновлення в днях.
    /// Використовується для отримання нових токенів доступу.
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;

    /// <summary>
    /// Алгоритм підпису токена.
    /// За замовчуванням використовується HMAC SHA-256.
    /// </summary>
    public string Algorithm { get; set; } = "HS256";

    /// <summary>
    /// Чи включати час видачі токена (iat claim).
    /// </summary>
    public bool IncludeIssuedAt { get; set; } = true;

    /// <summary>
    /// Чи включати унікальний ідентифікатор токена (jti claim).
    /// Корисно для відстеження та відкликання токенів.
    /// </summary>
    public bool IncludeJwtId { get; set; } = true;

    /// <summary>
    /// Допустиме відхилення часу в секундах для валідації токена.
    /// Компенсує можливі розбіжності в часі між серверами.
    /// </summary>
    public int ClockSkewSeconds { get; set; } = 300; // 5 хвилин
}
