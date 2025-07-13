namespace AspNet.MinimalApi.CustomJwtAuth.Authentication;

/// <summary>
/// Константи для стандартних та кастомних claims у JWT токенах.
/// Використовуються для уніфікації назв claims по всьому додатку.
/// </summary>
public static class JwtClaims
{
    // Стандартні JWT claims (RFC 7519)
    
    /// <summary>
    /// Subject - унікальний ідентифікатор користувача.
    /// Стандартний claim "sub".
    /// </summary>
    public const string Subject = "sub";

    /// <summary>
    /// Issuer - видавець токена.
    /// Стандартний claim "iss".
    /// </summary>
    public const string Issuer = "iss";

    /// <summary>
    /// Audience - аудиторія токена.
    /// Стандартний claim "aud".
    /// </summary>
    public const string Audience = "aud";

    /// <summary>
    /// Expiration Time - час закінчення дії токена.
    /// Стандартний claim "exp".
    /// </summary>
    public const string ExpirationTime = "exp";

    /// <summary>
    /// Not Before - час, до якого токен не є дійсним.
    /// Стандартний claim "nbf".
    /// </summary>
    public const string NotBefore = "nbf";

    /// <summary>
    /// Issued At - час видачі токена.
    /// Стандартний claim "iat".
    /// </summary>
    public const string IssuedAt = "iat";

    /// <summary>
    /// JWT ID - унікальний ідентифікатор токена.
    /// Стандартний claim "jti".
    /// </summary>
    public const string JwtId = "jti";

    // Кастомні claims для нашого додатку

    /// <summary>
    /// Ім'я користувача.
    /// Кастомний claim для зберігання username.
    /// </summary>
    public const string Username = "username";

    /// <summary>
    /// Електронна пошта користувача.
    /// Кастомний claim для зберігання email.
    /// </summary>
    public const string Email = "email";

    /// <summary>
    /// Роль користувача в системі.
    /// Кастомний claim для зберігання ролі.
    /// </summary>
    public const string Role = "role";

    /// <summary>
    /// Тип токена (access або refresh).
    /// Кастомний claim для розрізнення типів токенів.
    /// </summary>
    public const string TokenType = "token_type";

    /// <summary>
    /// Версія токена.
    /// Кастомний claim для версіонування токенів.
    /// </summary>
    public const string TokenVersion = "token_version";

    // Значення для типів токенів

    /// <summary>
    /// Значення для токена доступу.
    /// </summary>
    public const string AccessTokenType = "access";

    /// <summary>
    /// Значення для токена оновлення.
    /// </summary>
    public const string RefreshTokenType = "refresh";

    // Ролі користувачів

    /// <summary>
    /// Роль звичайного користувача.
    /// </summary>
    public const string UserRole = "User";

    /// <summary>
    /// Роль адміністратора.
    /// </summary>
    public const string AdminRole = "Admin";

    /// <summary>
    /// Роль модератора.
    /// </summary>
    public const string ModeratorRole = "Moderator";
}
