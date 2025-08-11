using System.ComponentModel.DataAnnotations;
using AspNet.MinimalApi.BlogWithFront.Domain;

namespace AspNet.MinimalApi.BlogWithFront.Models;

/// <summary>
/// Модель рефреш токена для оновлення JWT токенів
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// Унікальний ідентифікатор рефреш токена
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Значення рефреш токена
    /// </summary>
    [Required]
    [StringLength(500)]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Дата створення токена
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата закінчення дії токена
    /// </summary>
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// Чи був токен відкликаний
    /// </summary>
    public bool IsRevoked { get; set; } = false;

    /// <summary>
    /// Дата відкликання токена
    /// </summary>
    public DateTime? RevokedDate { get; set; }

    /// <summary>
    /// Ідентифікатор користувача, якому належить токен
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Навігаційна властивість до користувача
    /// </summary>
    public virtual ApplicationUser? User { get; set; }

    /// <summary>
    /// Перевіряє, чи є токен активним
    /// </summary>
    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiryDate;
}
