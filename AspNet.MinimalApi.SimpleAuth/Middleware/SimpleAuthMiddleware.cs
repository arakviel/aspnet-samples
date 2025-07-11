namespace AspNet.MinimalApi.SimpleAuth.Middleware;

public class SimpleAuthMiddleware
{
    private readonly RequestDelegate _next;
    private const string AuthCookieName = "SimpleAuth";

    public SimpleAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Перевіряємо чи є cookie аутентифікації
        var authCookie = context.Request.Cookies[AuthCookieName];
        
        if (!string.IsNullOrEmpty(authCookie))
        {
            // Додаємо інформацію про користувача в контекст
            context.Items["IsAuthenticated"] = true;
            context.Items["Username"] = authCookie;
        }
        else
        {
            context.Items["IsAuthenticated"] = false;
        }

        await _next(context);
    }
}

// Extension method для зручного додавання middleware
public static class SimpleAuthMiddlewareExtensions
{
    public static IApplicationBuilder UseSimpleAuth(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SimpleAuthMiddleware>();
    }
}
