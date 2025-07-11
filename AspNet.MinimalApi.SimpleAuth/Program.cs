using AspNet.MinimalApi.SimpleAuth.Middleware;
using AspNet.MinimalApi.SimpleAuth.Models;
using AspNet.MinimalApi.SimpleAuth.Services;

var builder = WebApplication.CreateBuilder(args);

// Реєструємо сервіси
builder.Services.AddSingleton<IUserService, UserService>();

var app = builder.Build();

// Налаштування статичних файлів
app.UseStaticFiles();

// Додаємо наш кастомний middleware для аутентифікації
app.UseSimpleAuth();

// Головна сторінка - перенаправляємо на index.html
app.MapGet("/", () => Results.Redirect("/index.html"));

// Ендпоінт для реєстрації
app.MapPost("/register", async (RegisterRequest request, IUserService userService) =>
{
    var result = await userService.RegisterAsync(request);

    if (!result.Success)
    {
        return Results.BadRequest(result);
    }

    return Results.Ok(result);
});

// Ендпоінт для логіну
app.MapPost("/login", async (LoginRequest request, HttpContext context, IUserService userService) =>
{
    var result = await userService.LoginAsync(request);

    if (!result.Success)
    {
        return Results.BadRequest(result);
    }

    // Встановлюємо cookie після успішної аутентифікації
    var cookieOptions = new CookieOptions
    {
        HttpOnly = true,
        Secure = false, // В продакшені має бути true для HTTPS
        SameSite = SameSiteMode.Lax,
        Expires = DateTimeOffset.UtcNow.AddHours(24)
    };

    context.Response.Cookies.Append("SimpleAuth", result.Username!, cookieOptions);

    return Results.Ok(result);
});

// Ендпоінт для логауту
app.MapPost("/logout", (HttpContext context) =>
{
    context.Response.Cookies.Delete("SimpleAuth");
    return Results.Ok("Успішно вийшли з системи");
});

// Захищений ендпоінт
app.MapGet("/protected", (HttpContext context) =>
{
    var isAuthenticated = context.Items["IsAuthenticated"] as bool? ?? false;

    if (!isAuthenticated)
    {
        return Results.Unauthorized();
    }

    var username = context.Items["Username"] as string;
    return Results.Ok($"Привіт, {username}! Це захищений ресурс.");
});

// Ендпоінт для перевірки статусу аутентифікації
app.MapGet("/auth/status", (HttpContext context) =>
{
    var isAuthenticated = context.Items["IsAuthenticated"] as bool? ?? false;
    var username = context.Items["Username"] as string;

    return Results.Ok(new {
        IsAuthenticated = isAuthenticated,
        Username = username ?? "Гість"
    });
});

// Ендпоінт для перегляду всіх користувачів (захищений)
app.MapGet("/users", async (HttpContext context, IUserService userService) =>
{
    var isAuthenticated = context.Items["IsAuthenticated"] as bool? ?? false;

    if (!isAuthenticated)
    {
        return Results.Unauthorized();
    }

    var users = await userService.GetAllUsersAsync();
    return Results.Ok(users);
});

app.Run();
