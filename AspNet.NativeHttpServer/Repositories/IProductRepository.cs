using AspNet.NativeHttpServer.Models;

namespace AspNet.NativeHttpServer.Repositories;

/// <summary>
/// Defines the contract for product repository operations.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Gets all products from the repository.
    /// </summary>
    /// <returns>A collection of all products.</returns>
    Task<IEnumerable<Product>> GetAllAsync();

    /// <summary>
    /// Gets a product by its unique identifier.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <returns>The product if found; otherwise, null.</returns>
    Task<Product?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new product in the repository.
    /// </summary>
    /// <param name="product">The product to create.</param>
    /// <returns>The created product with assigned identifier.</returns>
    Task<Product> CreateAsync(Product product);

    /// <summary>
    /// Updates an existing product in the repository.
    /// </summary>
    /// <param name="product">The product to update.</param>
    /// <returns>The updated product if successful; otherwise, null.</returns>
    Task<Product?> UpdateAsync(Product product);

    /// <summary>
    /// Deletes a product from the repository.
    /// </summary>
    /// <param name="id">The identifier of the product to delete.</param>
    /// <returns>True if the product was deleted; otherwise, false.</returns>
    Task<bool> DeleteAsync(int id);
}
