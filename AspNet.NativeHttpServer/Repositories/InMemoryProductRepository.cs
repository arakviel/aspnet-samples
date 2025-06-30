using AspNet.NativeHttpServer.Models;
using System.Collections.Concurrent;

namespace AspNet.NativeHttpServer.Repositories;

/// <summary>
/// In-memory implementation of the product repository.
/// Thread-safe implementation using ConcurrentDictionary.
/// </summary>
public class InMemoryProductRepository : IProductRepository
{
    private readonly ConcurrentDictionary<int, Product> _products = new();
    private int _nextId = 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryProductRepository"/> class
    /// with sample data.
    /// </summary>
    public InMemoryProductRepository()
    {
        SeedData();
    }

    /// <inheritdoc />
    public Task<IEnumerable<Product>> GetAllAsync()
    {
        var products = _products.Values.OrderBy(p => p.Id).AsEnumerable();
        return Task.FromResult(products);
    }

    /// <inheritdoc />
    public Task<Product?> GetByIdAsync(int id)
    {
        _products.TryGetValue(id, out var product);
        return Task.FromResult(product);
    }

    /// <inheritdoc />
    public Task<Product> CreateAsync(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        product.Id = Interlocked.Increment(ref _nextId);
        product.CreatedAt = DateTime.UtcNow;
        
        _products.TryAdd(product.Id, product);
        return Task.FromResult(product);
    }

    /// <inheritdoc />
    public Task<Product?> UpdateAsync(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (_products.ContainsKey(product.Id))
        {
            _products[product.Id] = product;
            return Task.FromResult<Product?>(product);
        }

        return Task.FromResult<Product?>(null);
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(int id)
    {
        var removed = _products.TryRemove(id, out _);
        return Task.FromResult(removed);
    }

    /// <summary>
    /// Seeds the repository with initial sample data.
    /// </summary>
    private void SeedData()
    {
        var sampleProducts = new[]
        {
            new Product { Name = "Laptop", Description = "High-performance laptop", Price = 999.99m, Stock = 10 },
            new Product { Name = "Mouse", Description = "Wireless optical mouse", Price = 29.99m, Stock = 50 },
            new Product { Name = "Keyboard", Description = "Mechanical gaming keyboard", Price = 149.99m, Stock = 25 }
        };

        foreach (var product in sampleProducts)
        {
            CreateAsync(product).Wait();
        }
    }
}
