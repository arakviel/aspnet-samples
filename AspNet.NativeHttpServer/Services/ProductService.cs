using AspNet.NativeHttpServer.Models;
using AspNet.NativeHttpServer.Repositories;

namespace AspNet.NativeHttpServer.Services;

/// <summary>
/// Service class that handles business logic for product operations.
/// </summary>
public class ProductService
{
    private readonly IProductRepository _productRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductService"/> class.
    /// </summary>
    /// <param name="productRepository">The product repository.</param>
    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    }

    /// <summary>
    /// Gets all products.
    /// </summary>
    /// <returns>A collection of all products.</returns>
    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _productRepository.GetAllAsync();
    }

    /// <summary>
    /// Gets a product by its identifier.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <returns>The product if found; otherwise, null.</returns>
    public async Task<Product?> GetProductByIdAsync(int id)
    {
        if (id <= 0)
            return null;

        return await _productRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="product">The product to create.</param>
    /// <returns>The created product.</returns>
    /// <exception cref="ArgumentException">Thrown when product data is invalid.</exception>
    public async Task<Product> CreateProductAsync(Product product)
    {
        ValidateProduct(product);
        return await _productRepository.CreateAsync(product);
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="product">The updated product data.</param>
    /// <returns>The updated product if successful; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when product data is invalid.</exception>
    public async Task<Product?> UpdateProductAsync(int id, Product product)
    {
        if (id <= 0)
            return null;

        ValidateProduct(product);
        product.Id = id;
        
        return await _productRepository.UpdateAsync(product);
    }

    /// <summary>
    /// Deletes a product.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <returns>True if the product was deleted; otherwise, false.</returns>
    public async Task<bool> DeleteProductAsync(int id)
    {
        if (id <= 0)
            return false;

        return await _productRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Validates product data.
    /// </summary>
    /// <param name="product">The product to validate.</param>
    /// <exception cref="ArgumentException">Thrown when product data is invalid.</exception>
    private static void ValidateProduct(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (string.IsNullOrWhiteSpace(product.Name))
            throw new ArgumentException("Product name is required.", nameof(product));

        if (product.Price < 0)
            throw new ArgumentException("Product price cannot be negative.", nameof(product));

        if (product.Stock < 0)
            throw new ArgumentException("Product stock cannot be negative.", nameof(product));
    }
}
