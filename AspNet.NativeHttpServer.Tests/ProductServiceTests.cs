using AspNet.NativeHttpServer.Models;
using AspNet.NativeHttpServer.Repositories;
using AspNet.NativeHttpServer.Services;
using Xunit;

namespace AspNet.NativeHttpServer.Tests;

/// <summary>
/// Unit tests for the ProductService class.
/// </summary>
public class ProductServiceTests
{
    /// <summary>
    /// Creates a new ProductService instance with an in-memory repository for testing.
    /// </summary>
    /// <returns>A ProductService instance for testing.</returns>
    private static ProductService CreateProductService()
    {
        var repository = new InMemoryProductRepository();
        return new ProductService(repository);
    }

    /// <summary>
    /// Tests that GetAllProductsAsync returns all products.
    /// </summary>
    [Fact]
    public async Task GetAllProductsAsync_ShouldReturnAllProducts()
    {
        // Arrange
        var service = CreateProductService();

        // Act
        var products = await service.GetAllProductsAsync();

        // Assert
        Assert.NotNull(products);
        Assert.True(products.Any());
        Assert.Equal(3, products.Count()); // Initial seed data has 3 products
    }

    /// <summary>
    /// Tests that GetProductByIdAsync returns the correct product.
    /// </summary>
    [Fact]
    public async Task GetProductByIdAsync_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        var service = CreateProductService();
        var allProducts = await service.GetAllProductsAsync();
        var firstProduct = allProducts.First();

        // Act
        var product = await service.GetProductByIdAsync(firstProduct.Id);

        // Assert
        Assert.NotNull(product);
        Assert.Equal(firstProduct.Id, product.Id);
        Assert.Equal(firstProduct.Name, product.Name);
    }

    /// <summary>
    /// Tests that GetProductByIdAsync returns null for invalid ID.
    /// </summary>
    [Fact]
    public async Task GetProductByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var service = CreateProductService();

        // Act
        var product = await service.GetProductByIdAsync(999);

        // Assert
        Assert.Null(product);
    }

    /// <summary>
    /// Tests that GetProductByIdAsync returns null for zero or negative ID.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task GetProductByIdAsync_WithZeroOrNegativeId_ShouldReturnNull(int id)
    {
        // Arrange
        var service = CreateProductService();

        // Act
        var product = await service.GetProductByIdAsync(id);

        // Assert
        Assert.Null(product);
    }

    /// <summary>
    /// Tests that CreateProductAsync creates a new product successfully.
    /// </summary>
    [Fact]
    public async Task CreateProductAsync_WithValidProduct_ShouldCreateProduct()
    {
        // Arrange
        var service = CreateProductService();
        var newProduct = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = 10
        };

        // Act
        var createdProduct = await service.CreateProductAsync(newProduct);

        // Assert
        Assert.NotNull(createdProduct);
        Assert.True(createdProduct.Id > 0);
        Assert.Equal(newProduct.Name, createdProduct.Name);
        Assert.Equal(newProduct.Description, createdProduct.Description);
        Assert.Equal(newProduct.Price, createdProduct.Price);
        Assert.Equal(newProduct.Stock, createdProduct.Stock);
    }

    /// <summary>
    /// Tests that CreateProductAsync throws exception for null product.
    /// </summary>
    [Fact]
    public async Task CreateProductAsync_WithNullProduct_ShouldThrowArgumentNullException()
    {
        // Arrange
        var service = CreateProductService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateProductAsync(null!));
    }

    /// <summary>
    /// Tests that CreateProductAsync throws exception for product with empty name.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateProductAsync_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var service = CreateProductService();
        var product = new Product
        {
            Name = name!,
            Description = "Test Description",
            Price = 99.99m,
            Stock = 10
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateProductAsync(product));
    }

    /// <summary>
    /// Tests that CreateProductAsync throws exception for product with negative price.
    /// </summary>
    [Fact]
    public async Task CreateProductAsync_WithNegativePrice_ShouldThrowArgumentException()
    {
        // Arrange
        var service = CreateProductService();
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = -10.00m,
            Stock = 10
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateProductAsync(product));
    }

    /// <summary>
    /// Tests that CreateProductAsync throws exception for product with negative stock.
    /// </summary>
    [Fact]
    public async Task CreateProductAsync_WithNegativeStock_ShouldThrowArgumentException()
    {
        // Arrange
        var service = CreateProductService();
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = -5
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateProductAsync(product));
    }

    /// <summary>
    /// Tests that UpdateProductAsync updates an existing product successfully.
    /// </summary>
    [Fact]
    public async Task UpdateProductAsync_WithValidProduct_ShouldUpdateProduct()
    {
        // Arrange
        var service = CreateProductService();
        var allProducts = await service.GetAllProductsAsync();
        var existingProduct = allProducts.First();
        
        var updatedProduct = new Product
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 199.99m,
            Stock = 20
        };

        // Act
        var result = await service.UpdateProductAsync(existingProduct.Id, updatedProduct);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingProduct.Id, result.Id);
        Assert.Equal(updatedProduct.Name, result.Name);
        Assert.Equal(updatedProduct.Description, result.Description);
        Assert.Equal(updatedProduct.Price, result.Price);
        Assert.Equal(updatedProduct.Stock, result.Stock);
    }

    /// <summary>
    /// Tests that UpdateProductAsync returns null for non-existent product.
    /// </summary>
    [Fact]
    public async Task UpdateProductAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var service = CreateProductService();
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = 10
        };

        // Act
        var result = await service.UpdateProductAsync(999, product);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Tests that DeleteProductAsync deletes an existing product successfully.
    /// </summary>
    [Fact]
    public async Task DeleteProductAsync_WithValidId_ShouldDeleteProduct()
    {
        // Arrange
        var service = CreateProductService();
        var allProducts = await service.GetAllProductsAsync();
        var productToDelete = allProducts.First();

        // Act
        var result = await service.DeleteProductAsync(productToDelete.Id);

        // Assert
        Assert.True(result);
        
        // Verify product is deleted
        var deletedProduct = await service.GetProductByIdAsync(productToDelete.Id);
        Assert.Null(deletedProduct);
    }

    /// <summary>
    /// Tests that DeleteProductAsync returns false for non-existent product.
    /// </summary>
    [Fact]
    public async Task DeleteProductAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Arrange
        var service = CreateProductService();

        // Act
        var result = await service.DeleteProductAsync(999);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that DeleteProductAsync returns false for zero or negative ID.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DeleteProductAsync_WithZeroOrNegativeId_ShouldReturnFalse(int id)
    {
        // Arrange
        var service = CreateProductService();

        // Act
        var result = await service.DeleteProductAsync(id);

        // Assert
        Assert.False(result);
    }
}
