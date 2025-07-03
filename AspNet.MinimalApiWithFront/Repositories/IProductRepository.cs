using AspNet.MinimalApiWithFront.Models;

namespace AspNet.MinimalApiWithFront.Repositories;

/// <summary>
/// Інтерфейс репозиторію для роботи з товарами
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Отримує всі товари асинхронно
    /// </summary>
    /// <returns>Колекція всіх товарів</returns>
    Task<IEnumerable<Product>> GetAllAsync();

    /// <summary>
    /// Отримує товар за ідентифікатором асинхронно
    /// </summary>
    /// <param name="id">Ідентифікатор товару</param>
    /// <returns>Товар або null, якщо не знайдено</returns>
    Task<Product?> GetByIdAsync(int id);

    /// <summary>
    /// Створює новий товар асинхронно
    /// </summary>
    /// <param name="product">Товар для створення</param>
    /// <returns>Створений товар</returns>
    Task<Product> CreateAsync(Product product);

    /// <summary>
    /// Оновлює існуючий товар асинхронно
    /// </summary>
    /// <param name="product">Товар для оновлення</param>
    /// <returns>Оновлений товар</returns>
    Task<Product> UpdateAsync(Product product);

    /// <summary>
    /// Видаляє товар за ідентифікатором асинхронно
    /// </summary>
    /// <param name="id">Ідентифікатор товару для видалення</param>
    /// <returns>True, якщо товар було видалено, інакше false</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Перевіряє, чи існує товар з вказаним ідентифікатором
    /// </summary>
    /// <param name="id">Ідентифікатор товару</param>
    /// <returns>True, якщо товар існує, інакше false</returns>
    Task<bool> ExistsAsync(int id);
}
