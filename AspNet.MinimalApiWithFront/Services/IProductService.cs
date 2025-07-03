using AspNet.MinimalApiWithFront.Models;

namespace AspNet.MinimalApiWithFront.Services;

/// <summary>
/// Інтерфейс сервісу для роботи з товарами
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Отримує всі товари асинхронно
    /// </summary>
    /// <returns>Колекція всіх товарів</returns>
    Task<IEnumerable<Product>> GetAllProductsAsync();

    /// <summary>
    /// Отримує товар за ідентифікатором асинхронно
    /// </summary>
    /// <param name="id">Ідентифікатор товару</param>
    /// <returns>Товар або null, якщо не знайдено</returns>
    Task<Product?> GetProductByIdAsync(int id);

    /// <summary>
    /// Створює новий товар асинхронно
    /// </summary>
    /// <param name="product">Товар для створення</param>
    /// <returns>Створений товар</returns>
    Task<Product> CreateProductAsync(Product product);

    /// <summary>
    /// Оновлює існуючий товар асинхронно
    /// </summary>
    /// <param name="id">Ідентифікатор товару</param>
    /// <param name="product">Дані товару для оновлення</param>
    /// <returns>Оновлений товар або null, якщо товар не знайдено</returns>
    Task<Product?> UpdateProductAsync(int id, Product product);

    /// <summary>
    /// Видаляє товар за ідентифікатором асинхронно
    /// </summary>
    /// <param name="id">Ідентифікатор товару для видалення</param>
    /// <returns>True, якщо товар було видалено, інакше false</returns>
    Task<bool> DeleteProductAsync(int id);
}
