using System.ComponentModel.DataAnnotations;

namespace AspNet.MinimalApi.BlogWithFront.Slices.Auth;

/// <summary>
/// Запит на оновлення токена доступу
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// Рефреш токен
    /// </summary>
    [Required(ErrorMessage = "Рефреш токен є обов'язковим")]
    public string RefreshToken { get; set; } = string.Empty;
}
