using AspNet.NativeHttpServer.Controllers;
using AspNet.NativeHttpServer.Http;
using AspNet.NativeHttpServer.Repositories;
using AspNet.NativeHttpServer.Services;
using AspNet.NativeHttpServer.Views;

namespace AspNet.NativeHttpServer.Tests;

/// <summary>
/// Test fixture for setting up and managing the HTTP server for integration tests.
/// </summary>
public class TestServerFixture : IDisposable
{
    private HttpServer? _server;
    private readonly int _port;
    private bool _disposed = false;

    /// <summary>
    /// Gets the base URL for the test server.
    /// </summary>
    public string BaseUrl { get; }

    /// <summary>
    /// Gets the HTTP client for making requests to the test server.
    /// </summary>
    public HttpClient HttpClient { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestServerFixture"/> class.
    /// </summary>
    public TestServerFixture()
    {
        // Find an available port for testing
        _port = FindAvailablePort();
        BaseUrl = $"http://localhost:{_port}/";
        
        // Create HTTP client
        HttpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Start the server
        StartServer();
    }

    /// <summary>
    /// Starts the HTTP server for testing.
    /// </summary>
    private void StartServer()
    {
        // Create dependencies
        var productRepository = new InMemoryProductRepository();
        var productService = new ProductService(productRepository);
        var productController = new ProductController(productService);

        // Create and configure the HTTP server
        _server = new HttpServer(BaseUrl);
        
        // Configure routes
        ConfigureRoutes(_server.Router, productController);

        // Start the server in background
        _ = Task.Run(async () =>
        {
            try
            {
                await _server.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test server error: {ex.Message}");
            }
        });

        // Wait a bit for server to start
        Thread.Sleep(1000);
    }

    /// <summary>
    /// Configures the HTTP routes for the test server.
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

    /// <summary>
    /// Finds an available port for the test server.
    /// </summary>
    /// <returns>An available port number.</returns>
    private static int FindAvailablePort()
    {
        using var socket = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        socket.Start();
        var port = ((System.Net.IPEndPoint)socket.LocalEndpoint).Port;
        socket.Stop();
        return port;
    }

    /// <summary>
    /// Disposes the test server fixture.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the test server fixture.
    /// </summary>
    /// <param name="disposing">True if disposing; otherwise, false.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            HttpClient?.Dispose();
            _server?.Dispose();
            _disposed = true;
        }
    }
}
