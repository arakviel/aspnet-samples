using AspNet.NativeHttpServer.Http;
using AspNet.NativeHttpServer.Models;
using AspNet.NativeHttpServer.Services;
using AspNet.NativeHttpServer.Views;
using System.Net;
using System.Text.Json;

namespace AspNet.NativeHttpServer.Controllers;

/// <summary>
/// Controller for handling product-related HTTP requests.
/// </summary>
public class ProductController
{
    private readonly ProductService _productService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductController"/> class.
    /// </summary>
    /// <param name="productService">The product service.</param>
    public ProductController(ProductService productService)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
    }

    /// <summary>
    /// Handles GET /products - returns all products as HTML.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task GetAllProducts(HttpContext context)
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            var html = ProductViews.ProductListView(products);
            await context.WriteResponseAsync(html);
        }
        catch (Exception ex)
        {
            await context.WriteResponseAsync(
                $"<h1>Error</h1><p>{ex.Message}</p>",
                HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Handles GET /api/products - returns all products as JSON.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task GetAllProductsJson(HttpContext context)
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            var json = JsonSerializer.Serialize(products, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
            await context.WriteJsonResponseAsync(json);
        }
        catch (Exception ex)
        {
            var errorJson = JsonSerializer.Serialize(new { error = ex.Message });
            await context.WriteJsonResponseAsync(errorJson, HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Handles GET /products/{id} - returns a specific product as HTML.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task GetProduct(HttpContext context)
    {
        try
        {
            if (!context.RouteParameters.TryGetValue("id", out var idStr) ||
                !int.TryParse(idStr, out var id))
            {
                await context.WriteResponseAsync(
                    "<h1>Bad Request</h1><p>Invalid product ID.</p>",
                    HttpStatusCode.BadRequest);
                return;
            }

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                await context.WriteResponseAsync(
                    "<h1>Not Found</h1><p>Product not found.</p>",
                    HttpStatusCode.NotFound);
                return;
            }

            var html = ProductViews.ProductDetailView(product);
            await context.WriteResponseAsync(html);
        }
        catch (Exception ex)
        {
            await context.WriteResponseAsync(
                $"<h1>Error</h1><p>{ex.Message}</p>",
                HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Handles GET /api/products/{id} - returns a specific product as JSON.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task GetProductJson(HttpContext context)
    {
        try
        {
            if (!context.RouteParameters.TryGetValue("id", out var idStr) ||
                !int.TryParse(idStr, out var id))
            {
                var errorJson = JsonSerializer.Serialize(new { error = "Invalid product ID" });
                await context.WriteJsonResponseAsync(errorJson, HttpStatusCode.BadRequest);
                return;
            }

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                var errorJson = JsonSerializer.Serialize(new { error = "Product not found" });
                await context.WriteJsonResponseAsync(errorJson, HttpStatusCode.NotFound);
                return;
            }

            var json = JsonSerializer.Serialize(product, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
            await context.WriteJsonResponseAsync(json);
        }
        catch (Exception ex)
        {
            var errorJson = JsonSerializer.Serialize(new { error = ex.Message });
            await context.WriteJsonResponseAsync(errorJson, HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Handles POST /api/products - creates a new product.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task CreateProduct(HttpContext context)
    {
        try
        {
            var requestBody = await context.ReadRequestBodyAsync();
            var product = JsonSerializer.Deserialize<Product>(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (product == null)
            {
                var errorJson = JsonSerializer.Serialize(new { error = "Invalid product data" });
                await context.WriteJsonResponseAsync(errorJson, HttpStatusCode.BadRequest);
                return;
            }

            var createdProduct = await _productService.CreateProductAsync(product);
            var json = JsonSerializer.Serialize(createdProduct, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
            await context.WriteJsonResponseAsync(json, HttpStatusCode.Created);
        }
        catch (ArgumentException ex)
        {
            var errorJson = JsonSerializer.Serialize(new { error = ex.Message });
            await context.WriteJsonResponseAsync(errorJson, HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            var errorJson = JsonSerializer.Serialize(new { error = ex.Message });
            await context.WriteJsonResponseAsync(errorJson, HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Handles PUT /api/products/{id} - updates an existing product.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateProduct(HttpContext context)
    {
        try
        {
            if (!context.RouteParameters.TryGetValue("id", out var idStr) ||
                !int.TryParse(idStr, out var id))
            {
                var errorJson = JsonSerializer.Serialize(new { error = "Invalid product ID" });
                await context.WriteJsonResponseAsync(errorJson, HttpStatusCode.BadRequest);
                return;
            }

            var requestBody = await context.ReadRequestBodyAsync();
            var product = JsonSerializer.Deserialize<Product>(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (product == null)
            {
                var errorJson = JsonSerializer.Serialize(new { error = "Invalid product data" });
                await context.WriteJsonResponseAsync(errorJson, HttpStatusCode.BadRequest);
                return;
            }

            var updatedProduct = await _productService.UpdateProductAsync(id, product);
            if (updatedProduct == null)
            {
                var errorJson = JsonSerializer.Serialize(new { error = "Product not found" });
                await context.WriteJsonResponseAsync(errorJson, HttpStatusCode.NotFound);
                return;
            }

            var json = JsonSerializer.Serialize(updatedProduct, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
            await context.WriteJsonResponseAsync(json);
        }
        catch (ArgumentException ex)
        {
            var errorJson = JsonSerializer.Serialize(new { error = ex.Message });
            await context.WriteJsonResponseAsync(errorJson, HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            var errorJson = JsonSerializer.Serialize(new { error = ex.Message });
            await context.WriteJsonResponseAsync(errorJson, HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Handles DELETE /api/products/{id} - deletes a product.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteProduct(HttpContext context)
    {
        try
        {
            if (!context.RouteParameters.TryGetValue("id", out var idStr) ||
                !int.TryParse(idStr, out var id))
            {
                var errorJson = JsonSerializer.Serialize(new { error = "Invalid product ID" });
                await context.WriteJsonResponseAsync(errorJson, HttpStatusCode.BadRequest);
                return;
            }

            var deleted = await _productService.DeleteProductAsync(id);
            if (!deleted)
            {
                var errorJson = JsonSerializer.Serialize(new { error = "Product not found" });
                await context.WriteJsonResponseAsync(errorJson, HttpStatusCode.NotFound);
                return;
            }

            var successJson = JsonSerializer.Serialize(new { message = "Product deleted successfully" });
            await context.WriteJsonResponseAsync(successJson);
        }
        catch (Exception ex)
        {
            var errorJson = JsonSerializer.Serialize(new { error = ex.Message });
            await context.WriteJsonResponseAsync(errorJson, HttpStatusCode.InternalServerError);
        }
    }
}
