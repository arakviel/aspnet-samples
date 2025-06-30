using System.Text.RegularExpressions;

namespace AspNet.NativeHttpServer.Http;

/// <summary>
/// Delegate for handling HTTP requests.
/// </summary>
/// <param name="context">The HTTP context.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public delegate Task RequestHandler(HttpContext context);

/// <summary>
/// Represents a route definition with pattern matching and handler.
/// </summary>
public class Route
{
    /// <summary>
    /// Gets the HTTP method for this route.
    /// </summary>
    public string Method { get; }

    /// <summary>
    /// Gets the route pattern.
    /// </summary>
    public string Pattern { get; }

    /// <summary>
    /// Gets the compiled regex for pattern matching.
    /// </summary>
    public Regex Regex { get; }

    /// <summary>
    /// Gets the parameter names extracted from the pattern.
    /// </summary>
    public string[] ParameterNames { get; }

    /// <summary>
    /// Gets the request handler for this route.
    /// </summary>
    public RequestHandler Handler { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Route"/> class.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="pattern">The route pattern (e.g., "/products/{id}").</param>
    /// <param name="handler">The request handler.</param>
    public Route(string method, string pattern, RequestHandler handler)
    {
        Method = method ?? throw new ArgumentNullException(nameof(method));
        Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        Handler = handler ?? throw new ArgumentNullException(nameof(handler));

        (Regex, ParameterNames) = CompilePattern(pattern);
    }

    /// <summary>
    /// Compiles a route pattern into a regex and extracts parameter names.
    /// </summary>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A tuple containing the compiled regex and parameter names.</returns>
    private static (Regex regex, string[] parameterNames) CompilePattern(string pattern)
    {
        var parameterNames = new List<string>();

        // First, extract parameter names and replace them with placeholders
        var regexPattern = Regex.Replace(pattern, @"\{(\w+)\}", match =>
        {
            var paramName = match.Groups[1].Value;
            parameterNames.Add(paramName);
            return $"__PARAM_{parameterNames.Count - 1}__";
        });

        // Escape the pattern for regex
        regexPattern = Regex.Escape(regexPattern);

        // Replace placeholders with actual regex groups
        for (int i = 0; i < parameterNames.Count; i++)
        {
            regexPattern = regexPattern.Replace($"__PARAM_{i}__", $"(?<{parameterNames[i]}>[^/]+)");
        }

        regexPattern = "^" + regexPattern + "$";

        var regex = new Regex(regexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        return (regex, parameterNames.ToArray());
    }
}

/// <summary>
/// Simple HTTP router for handling requests based on patterns.
/// </summary>
public class Router
{
    private readonly List<Route> _routes = new();

    /// <summary>
    /// Adds a GET route.
    /// </summary>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="handler">The request handler.</param>
    public void Get(string pattern, RequestHandler handler)
    {
        AddRoute("GET", pattern, handler);
    }

    /// <summary>
    /// Adds a POST route.
    /// </summary>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="handler">The request handler.</param>
    public void Post(string pattern, RequestHandler handler)
    {
        AddRoute("POST", pattern, handler);
    }

    /// <summary>
    /// Adds a PUT route.
    /// </summary>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="handler">The request handler.</param>
    public void Put(string pattern, RequestHandler handler)
    {
        AddRoute("PUT", pattern, handler);
    }

    /// <summary>
    /// Adds a DELETE route.
    /// </summary>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="handler">The request handler.</param>
    public void Delete(string pattern, RequestHandler handler)
    {
        AddRoute("DELETE", pattern, handler);
    }

    /// <summary>
    /// Adds a route for the specified HTTP method.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="handler">The request handler.</param>
    public void AddRoute(string method, string pattern, RequestHandler handler)
    {
        _routes.Add(new Route(method, pattern, handler));
    }

    /// <summary>
    /// Handles an HTTP request by finding a matching route.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleRequestAsync(HttpContext context)
    {
        var route = FindMatchingRoute(context.Method, context.Path);
        
        if (route != null)
        {
            ExtractRouteParameters(route, context);
            await route.Handler(context);
        }
        else
        {
            await HandleNotFound(context);
        }
    }

    /// <summary>
    /// Finds a matching route for the given method and path.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="path">The request path.</param>
    /// <returns>The matching route or null if not found.</returns>
    private Route? FindMatchingRoute(string method, string path)
    {
        return _routes.FirstOrDefault(route => 
            string.Equals(route.Method, method, StringComparison.OrdinalIgnoreCase) &&
            route.Regex.IsMatch(path));
    }

    /// <summary>
    /// Extracts route parameters from the URL path.
    /// </summary>
    /// <param name="route">The matched route.</param>
    /// <param name="context">The HTTP context.</param>
    private static void ExtractRouteParameters(Route route, HttpContext context)
    {
        var match = route.Regex.Match(context.Path);
        
        foreach (var paramName in route.ParameterNames)
        {
            if (match.Groups[paramName].Success)
            {
                context.RouteParameters[paramName] = match.Groups[paramName].Value;
            }
        }
    }

    /// <summary>
    /// Handles requests that don't match any route.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task HandleNotFound(HttpContext context)
    {
        await context.WriteResponseAsync(
            "<h1>404 - Not Found</h1><p>The requested resource was not found.</p>",
            System.Net.HttpStatusCode.NotFound);
    }
}
