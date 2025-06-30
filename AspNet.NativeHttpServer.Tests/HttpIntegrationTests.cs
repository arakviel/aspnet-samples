using System.Net;
using System.Text;
using System.Text.Json;
using AspNet.NativeHttpServer.Models;
using Xunit;

namespace AspNet.NativeHttpServer.Tests;

/// <summary>
/// Integration tests for HTTP endpoints using HttpClient.
/// </summary>
public class HttpIntegrationTests : IClassFixture<TestServerFixture>
{
    private readonly TestServerFixture _fixture;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpIntegrationTests"/> class.
    /// </summary>
    /// <param name="fixture">The test server fixture.</param>
    public HttpIntegrationTests(TestServerFixture fixture)
    {
        _fixture = fixture;
        _httpClient = fixture.HttpClient;
    }

    /// <summary>
    /// Tests that the home page returns successfully.
    /// </summary>
    [Fact]
    public async Task Get_HomePage_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _httpClient.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Welcome to Native HTTP Server Demo", content);
        Assert.Contains("<!DOCTYPE html>", content);
    }

    /// <summary>
    /// Tests that the products page returns successfully.
    /// </summary>
    [Fact]
    public async Task Get_ProductsPage_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _httpClient.GetAsync("/products");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Products", content);
        Assert.Contains("<!DOCTYPE html>", content);
    }

    /// <summary>
    /// Tests that the API returns all products successfully.
    /// </summary>
    [Fact]
    public async Task Get_ApiProducts_ReturnsSuccessAndJsonContentType()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        
        var content = await response.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<Product[]>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        Assert.NotNull(products);
        Assert.True(products.Length >= 3); // Should have at least the seed data
    }

    /// <summary>
    /// Tests that getting a specific product by ID returns successfully.
    /// </summary>
    [Fact]
    public async Task Get_ApiProductById_ReturnsSuccessAndCorrectProduct()
    {
        // Arrange - First get all products to find a valid ID
        var allProductsResponse = await _httpClient.GetAsync("/api/products");
        allProductsResponse.EnsureSuccessStatusCode();
        
        var allProductsContent = await allProductsResponse.Content.ReadAsStringAsync();
        var allProducts = JsonSerializer.Deserialize<Product[]>(allProductsContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        Assert.NotNull(allProducts);
        Assert.True(allProducts.Length > 0);
        
        var firstProduct = allProducts[0];

        // Act
        var response = await _httpClient.GetAsync($"/api/products/{firstProduct.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        
        var content = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<Product>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        Assert.NotNull(product);
        Assert.Equal(firstProduct.Id, product.Id);
        Assert.Equal(firstProduct.Name, product.Name);
    }

    /// <summary>
    /// Tests that getting a non-existent product returns 404.
    /// </summary>
    [Fact]
    public async Task Get_ApiProductById_WithInvalidId_Returns404()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/products/999999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Tests that creating a new product works successfully.
    /// </summary>
    [Fact]
    public async Task Post_ApiProducts_CreatesProductSuccessfully()
    {
        // Arrange
        var newProduct = new
        {
            name = "Test Product",
            description = "Test Description",
            price = 99.99m,
            stock = 10
        };

        var json = JsonSerializer.Serialize(newProduct, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _httpClient.PostAsync("/api/products", content);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<Product>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        Assert.NotNull(createdProduct);
        Assert.True(createdProduct.Id > 0);
        Assert.Equal(newProduct.name, createdProduct.Name);
        Assert.Equal(newProduct.description, createdProduct.Description);
        Assert.Equal(newProduct.price, createdProduct.Price);
        Assert.Equal(newProduct.stock, createdProduct.Stock);
    }

    /// <summary>
    /// Tests that creating a product with invalid data returns 400.
    /// </summary>
    [Fact]
    public async Task Post_ApiProducts_WithInvalidData_Returns400()
    {
        // Arrange
        var invalidProduct = new
        {
            name = "", // Invalid: empty name
            description = "Test Description",
            price = -10.0m, // Invalid: negative price
            stock = 10
        };

        var json = JsonSerializer.Serialize(invalidProduct, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _httpClient.PostAsync("/api/products", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Tests that updating an existing product works successfully.
    /// </summary>
    [Fact]
    public async Task Put_ApiProducts_UpdatesProductSuccessfully()
    {
        // Arrange - First create a product
        var newProduct = new
        {
            name = "Product to Update",
            description = "Original Description",
            price = 50.0m,
            stock = 5
        };

        var createJson = JsonSerializer.Serialize(newProduct, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        
        var createResponse = await _httpClient.PostAsync("/api/products", createContent);
        createResponse.EnsureSuccessStatusCode();
        
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<Product>(createResponseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        Assert.NotNull(createdProduct);

        // Arrange - Prepare update data
        var updatedProduct = new
        {
            name = "Updated Product",
            description = "Updated Description",
            price = 75.0m,
            stock = 8
        };

        var updateJson = JsonSerializer.Serialize(updatedProduct, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _httpClient.PutAsync($"/api/products/{createdProduct.Id}", updateContent);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var resultProduct = JsonSerializer.Deserialize<Product>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        Assert.NotNull(resultProduct);
        Assert.Equal(createdProduct.Id, resultProduct.Id);
        Assert.Equal(updatedProduct.name, resultProduct.Name);
        Assert.Equal(updatedProduct.description, resultProduct.Description);
        Assert.Equal(updatedProduct.price, resultProduct.Price);
        Assert.Equal(updatedProduct.stock, resultProduct.Stock);
    }

    /// <summary>
    /// Tests that updating a non-existent product returns 404.
    /// </summary>
    [Fact]
    public async Task Put_ApiProducts_WithInvalidId_Returns404()
    {
        // Arrange
        var updateProduct = new
        {
            name = "Updated Product",
            description = "Updated Description",
            price = 75.0m,
            stock = 8
        };

        var json = JsonSerializer.Serialize(updateProduct, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _httpClient.PutAsync("/api/products/999999", content);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Tests that deleting an existing product works successfully.
    /// </summary>
    [Fact]
    public async Task Delete_ApiProducts_DeletesProductSuccessfully()
    {
        // Arrange - First create a product
        var newProduct = new
        {
            name = "Product to Delete",
            description = "Will be deleted",
            price = 25.0m,
            stock = 3
        };

        var createJson = JsonSerializer.Serialize(newProduct, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        
        var createResponse = await _httpClient.PostAsync("/api/products", createContent);
        createResponse.EnsureSuccessStatusCode();
        
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<Product>(createResponseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        Assert.NotNull(createdProduct);

        // Act
        var response = await _httpClient.DeleteAsync($"/api/products/{createdProduct.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("deleted successfully", responseContent);

        // Verify product is actually deleted
        var getResponse = await _httpClient.GetAsync($"/api/products/{createdProduct.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    /// <summary>
    /// Tests that deleting a non-existent product returns 404.
    /// </summary>
    [Fact]
    public async Task Delete_ApiProducts_WithInvalidId_Returns404()
    {
        // Act
        var response = await _httpClient.DeleteAsync("/api/products/999999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Tests that accessing a non-existent route returns 404.
    /// </summary>
    [Fact]
    public async Task Get_NonExistentRoute_Returns404()
    {
        // Act
        var response = await _httpClient.GetAsync("/non-existent-route");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
