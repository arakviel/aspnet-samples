using System.Net;
using System.Text;

namespace AspNet.NativeHttpServer.Http;

/// <summary>
/// Represents the HTTP context for a request, providing access to request and response data.
/// </summary>
public class HttpContext
{
    /// <summary>
    /// Gets the HTTP listener context.
    /// </summary>
    public HttpListenerContext ListenerContext { get; }

    /// <summary>
    /// Gets the HTTP request.
    /// </summary>
    public HttpListenerRequest Request => ListenerContext.Request;

    /// <summary>
    /// Gets the HTTP response.
    /// </summary>
    public HttpListenerResponse Response => ListenerContext.Response;

    /// <summary>
    /// Gets the request path without query string.
    /// </summary>
    public string Path => Request.Url?.AbsolutePath ?? "/";

    /// <summary>
    /// Gets the HTTP method (GET, POST, etc.).
    /// </summary>
    public string Method => Request.HttpMethod;

    /// <summary>
    /// Gets the query string parameters.
    /// </summary>
    public Dictionary<string, string> QueryParameters { get; }

    /// <summary>
    /// Gets the route parameters extracted from the URL path.
    /// </summary>
    public Dictionary<string, string> RouteParameters { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpContext"/> class.
    /// </summary>
    /// <param name="listenerContext">The HTTP listener context.</param>
    public HttpContext(HttpListenerContext listenerContext)
    {
        ListenerContext = listenerContext ?? throw new ArgumentNullException(nameof(listenerContext));
        QueryParameters = ParseQueryString(Request.Url?.Query);
    }

    /// <summary>
    /// Reads the request body as a string.
    /// </summary>
    /// <returns>The request body content.</returns>
    public async Task<string> ReadRequestBodyAsync()
    {
        using var reader = new StreamReader(Request.InputStream, Request.ContentEncoding ?? Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    /// <summary>
    /// Writes a response with the specified content and status code.
    /// </summary>
    /// <param name="content">The response content.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="contentType">The content type.</param>
    public async Task WriteResponseAsync(string content, HttpStatusCode statusCode = HttpStatusCode.OK, string contentType = "text/html; charset=utf-8")
    {
        var buffer = Encoding.UTF8.GetBytes(content);
        
        Response.StatusCode = (int)statusCode;
        Response.ContentType = contentType;
        Response.ContentLength64 = buffer.Length;
        
        await Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        Response.OutputStream.Close();
    }

    /// <summary>
    /// Writes a JSON response.
    /// </summary>
    /// <param name="content">The JSON content.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    public async Task WriteJsonResponseAsync(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        await WriteResponseAsync(content, statusCode, "application/json; charset=utf-8");
    }

    /// <summary>
    /// Parses query string parameters.
    /// </summary>
    /// <param name="queryString">The query string.</param>
    /// <returns>A dictionary of query parameters.</returns>
    private static Dictionary<string, string> ParseQueryString(string? queryString)
    {
        var parameters = new Dictionary<string, string>();
        
        if (string.IsNullOrEmpty(queryString))
            return parameters;

        queryString = queryString.TrimStart('?');
        var pairs = queryString.Split('&');
        
        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=', 2);
            if (keyValue.Length == 2)
            {
                var key = Uri.UnescapeDataString(keyValue[0]);
                var value = Uri.UnescapeDataString(keyValue[1]);
                parameters[key] = value;
            }
        }
        
        return parameters;
    }
}
