using AspNet.NativeHttpServer.Controllers;
using AspNet.NativeHttpServer.Http;
using AspNet.NativeHttpServer.Repositories;
using AspNet.NativeHttpServer.Services;
using AspNet.NativeHttpServer.Views;

namespace AspNet.NativeHttpServer;

/// <summary>
/// The main entry point for the Native HTTP Server application.
/// </summary>
class Program
{
    /// <summary>
    /// The main method that starts the HTTP server.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    static async Task Main(string[] args)
    {
        // Create dependencies
        var productRepository = new InMemoryProductRepository();
        var productService = new ProductService(productRepository);
        var productController = new ProductController(productService);

        // Create and configure the HTTP server
        var server = new HttpServer("http://localhost:8081/");

        // Configure routes
        ConfigureRoutes(server.Router, productController);

        // Handle Ctrl+C gracefully
        var cancellationTokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        try
        {
            // Start the server
            await server.StartAsync(cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Server shutdown requested.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Server error: {ex.Message}");
        }
        finally
        {
            server.Dispose();
        }
    }

    /// <summary>
    /// Configures the HTTP routes for the application.
    /// </summary>
    /// <param name="router">The router to configure.</param>
    /// <param name="productController">The product controller.</param>
    private static void ConfigureRoutes(Router router, ProductController productController)
    {
        // Home page
        router.Get("/", async context =>
        {
            var html = ProductViews.HomeView();
            await context.WriteResponseAsync(html);
        });

        // HTML views for products
        router.Get("/products", productController.GetAllProducts);
        router.Get("/products/{id}", productController.GetProduct);

        // JSON API endpoints
        router.Get("/api/products", productController.GetAllProductsJson);
        router.Get("/api/products/{id}", productController.GetProductJson);
        router.Post("/api/products", productController.CreateProduct);
        router.Put("/api/products/{id}", productController.UpdateProduct);
        router.Delete("/api/products/{id}", productController.DeleteProduct);
    }
}