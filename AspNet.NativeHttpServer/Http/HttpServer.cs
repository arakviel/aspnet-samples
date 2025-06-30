using System.Net;

namespace AspNet.NativeHttpServer.Http;

/// <summary>
/// A simple HTTP server built on top of HttpListener.
/// </summary>
public class HttpServer
{
    private readonly HttpListener _listener;
    private readonly Router _router;
    private readonly string[] _prefixes;
    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// Gets a value indicating whether the server is running.
    /// </summary>
    public bool IsRunning => _listener.IsListening;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpServer"/> class.
    /// </summary>
    /// <param name="prefixes">The URL prefixes to listen on.</param>
    public HttpServer(params string[] prefixes)
    {
        if (prefixes == null || prefixes.Length == 0)
            throw new ArgumentException("At least one prefix must be specified.", nameof(prefixes));

        _prefixes = prefixes;
        _listener = new HttpListener();
        _router = new Router();

        foreach (var prefix in _prefixes)
        {
            _listener.Prefixes.Add(prefix);
        }
    }

    /// <summary>
    /// Gets the router for configuring routes.
    /// </summary>
    public Router Router => _router;

    /// <summary>
    /// Starts the HTTP server.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (IsRunning)
            throw new InvalidOperationException("Server is already running.");

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        _listener.Start();
        Console.WriteLine($"HTTP Server started. Listening on:");
        
        foreach (var prefix in _prefixes)
        {
            Console.WriteLine($"  {prefix}");
        }

        await ListenAsync(_cancellationTokenSource.Token);
    }

    /// <summary>
    /// Stops the HTTP server.
    /// </summary>
    public void Stop()
    {
        if (!IsRunning)
            return;

        _cancellationTokenSource?.Cancel();
        _listener.Stop();
        Console.WriteLine("HTTP Server stopped.");
    }

    /// <summary>
    /// Listens for incoming HTTP requests.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ListenAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _listener.IsListening)
        {
            try
            {
                var listenerContext = await _listener.GetContextAsync();
                
                // Handle request in background to avoid blocking
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var httpContext = new HttpContext(listenerContext);
                        await _router.HandleRequestAsync(httpContext);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error handling request: {ex.Message}");
                        await HandleServerError(listenerContext, ex);
                    }
                }, cancellationToken);
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 995) // ERROR_OPERATION_ABORTED
            {
                // Server was stopped, this is expected
                break;
            }
            catch (ObjectDisposedException)
            {
                // Listener was disposed, this is expected when stopping
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in server loop: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Handles server errors by sending a 500 response.
    /// </summary>
    /// <param name="listenerContext">The HTTP listener context.</param>
    /// <param name="exception">The exception that occurred.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task HandleServerError(HttpListenerContext listenerContext, Exception exception)
    {
        try
        {
            var response = listenerContext.Response;
            response.StatusCode = 500;
            response.ContentType = "text/html; charset=utf-8";

            var errorHtml = $@"
                <h1>500 - Internal Server Error</h1>
                <p>An error occurred while processing your request.</p>
                <details>
                    <summary>Error Details</summary>
                    <pre>{exception.Message}</pre>
                </details>";

            var buffer = System.Text.Encoding.UTF8.GetBytes(errorHtml);
            response.ContentLength64 = buffer.Length;
            
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
        catch
        {
            // If we can't send an error response, there's nothing more we can do
        }
    }

    /// <summary>
    /// Disposes the HTTP server resources.
    /// </summary>
    public void Dispose()
    {
        Stop();
        _listener?.Close();
        _cancellationTokenSource?.Dispose();
    }
}
