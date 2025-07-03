using System.ComponentModel.DataAnnotations;

namespace AspNet.MinimalApiWithFront.Models;

/// <summary>
/// Модель товару для демонстрації CRUD операцій
/// </summary>
public class Product
{
    /// <summary>
    /// Унікальний ідентифікатор товару
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Назва товару
    /// </summary>
    [Required(ErrorMessage = "Назва товару є обов'язковою")]
    [StringLength(100, ErrorMessage = "Назва товару не може перевищувати 100 символів")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Опис товару
    /// </summary>
    [StringLength(500, ErrorMessage = "Опис товару не може перевищувати 500 символів")]
    public string? Description { get; set; }

    /// <summary>
    /// Ціна товару
    /// </summary>
    [Required(ErrorMessage = "Ціна товару є обов'язковою")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Ціна товару повинна бути більше 0")]
    public decimal Price { get; set; }

    /// <summary>
    /// Кількість товару на складі
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Кількість товару не може бути від'ємною")]
    public int Stock { get; set; }

    /// <summary>
    /// Дата створення товару
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата останнього оновлення товару
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
