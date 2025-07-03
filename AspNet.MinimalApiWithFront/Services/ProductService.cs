using AspNet.MinimalApiWithFront.Models;
using AspNet.MinimalApiWithFront.Repositories;

namespace AspNet.MinimalApiWithFront.Services;

/// <summary>
/// Сервіс для роботи з товарами, що містить бізнес-логіку
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    /// <summary>
    /// Ініціалізує новий екземпляр ProductService
    /// </summary>
    /// <param name="productRepository">Репозиторій товарів</param>
    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    /// <summary>
    /// Отримує всі товари асинхронно
    /// </summary>
    /// <returns>Колекція всіх товарів</returns>
    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _productRepository.GetAllAsync();
    }

    /// <summary>
    /// Отримує товар за ідентифікатором асинхронно
    /// </summary>
    /// <param name="id">Ідентифікатор товару</param>
    /// <returns>Товар або null, якщо не знайдено</returns>
    public async Task<Product?> GetProductByIdAsync(int id)
    {
        if (id <= 0)
        {
            return null;
        }

        return await _productRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Створює новий товар асинхронно
    /// </summary>
    /// <param name="product">Товар для створення</param>
    /// <returns>Створений товар</returns>
    public async Task<Product> CreateProductAsync(Product product)
    {
        // Валідація бізнес-правил
        ValidateProduct(product);

        return await _productRepository.CreateAsync(product);
    }

    /// <summary>
    /// Оновлює існуючий товар асинхронно
    /// </summary>
    /// <param name="id">Ідентифікатор товару</param>
    /// <param name="product">Дані товару для оновлення</param>
    /// <returns>Оновлений товар або null, якщо товар не знайдено</returns>
    public async Task<Product?> UpdateProductAsync(int id, Product product)
    {
        if (id <= 0 || !await _productRepository.ExistsAsync(id))
        {
            return null;
        }

        // Валідація бізнес-правил
        ValidateProduct(product);

        product.Id = id;
        return await _productRepository.UpdateAsync(product);
    }

    /// <summary>
    /// Видаляє товар за ідентифікатором асинхронно
    /// </summary>
    /// <param name="id">Ідентифікатор товару для видалення</param>
    /// <returns>True, якщо товар було видалено, інакше false</returns>
    public async Task<bool> DeleteProductAsync(int id)
    {
        if (id <= 0)
        {
            return false;
        }

        return await _productRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Валідує товар згідно з бізнес-правилами
    /// </summary>
    /// <param name="product">Товар для валідації</param>
    /// <exception cref="ArgumentException">Викидається при порушенні бізнес-правил</exception>
    private static void ValidateProduct(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            throw new ArgumentException("Назва товару не може бути порожньою", nameof(product.Name));
        }

        if (product.Price <= 0)
        {
            throw new ArgumentException("Ціна товару повинна бути більше нуля", nameof(product.Price));
        }

        if (product.Stock < 0)
        {
            throw new ArgumentException("Кількість товару не може бути від'ємною", nameof(product.Stock));
        }
    }
}
