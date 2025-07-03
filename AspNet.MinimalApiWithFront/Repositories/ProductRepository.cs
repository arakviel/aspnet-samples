using AspNet.MinimalApiWithFront.Data;
using AspNet.MinimalApiWithFront.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNet.MinimalApiWithFront.Repositories;

/// <summary>
/// Реалізація репозиторію для роботи з товарами
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Ініціалізує новий екземпляр ProductRepository
    /// </summary>
    /// <param name="context">Контекст бази даних</param>
    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Отримує всі товари асинхронно
    /// </summary>
    /// <returns>Колекція всіх товарів</returns>
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Отримує товар за ідентифікатором асинхронно
    /// </summary>
    /// <param name="id">Ідентифікатор товару</param>
    /// <returns>Товар або null, якщо не знайдено</returns>
    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    /// <summary>
    /// Створює новий товар асинхронно
    /// </summary>
    /// <param name="product">Товар для створення</param>
    /// <returns>Створений товар</returns>
    public async Task<Product> CreateAsync(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        
        return product;
    }

    /// <summary>
    /// Оновлює існуючий товар асинхронно
    /// </summary>
    /// <param name="product">Товар для оновлення</param>
    /// <returns>Оновлений товар</returns>
    public async Task<Product> UpdateAsync(Product product)
    {
        product.UpdatedAt = DateTime.UtcNow;
        
        _context.Entry(product).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        
        return product;
    }

    /// <summary>
    /// Видаляє товар за ідентифікатором асинхронно
    /// </summary>
    /// <param name="id">Ідентифікатор товару для видалення</param>
    /// <returns>True, якщо товар було видалено, інакше false</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return false;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        
        return true;
    }

    /// <summary>
    /// Перевіряє, чи існує товар з вказаним ідентифікатором
    /// </summary>
    /// <param name="id">Ідентифікатор товару</param>
    /// <returns>True, якщо товар існує, інакше false</returns>
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Products.AnyAsync(p => p.Id == id);
    }
}
