using AspNet.MinimalApi.CustomAuth.Authentication;
using AspNet.MinimalApi.CustomAuth.Data;
using AspNet.MinimalApi.CustomAuth.Extensions;
using AspNet.MinimalApi.CustomAuth.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Налаштування бази даних SQLite
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Реєстрація сервісів
builder.Services.AddScoped<IUserService, UserService>();

// Налаштування кастомної аутентифікації
builder.Services.AddCustomAuthentication(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
    options.Cookie.HttpOnly = true; // Захист від доступу через JavaScript
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS у продакшені
    options.Cookie.SameSite = SameSiteMode.Strict; // Захист від CSRF
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Час життя куки
    options.SlidingExpiration = true; // Ковзаючий термін дії
});

var app = builder.Build();

// Створення бази даних при запуску
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    context.Database.EnsureCreated();
}

// Налаштування middleware
app.UseCustomAuthentication(); // Наш кастомний middleware аутентифікації

// Форма логіну
app.MapGet("/login", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    var loginForm = @"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <title>ITStep - Кастомна аутентифікація</title>
    <style>
        body { font-family: Arial, sans-serif; max-width: 400px; margin: 50px auto; padding: 20px; }
        .form-group { margin-bottom: 15px; }
        label { display: block; margin-bottom: 5px; font-weight: bold; }
        input[type='email'], input[type='password'] { width: 100%; padding: 8px; border: 1px solid #ddd; border-radius: 4px; }
        input[type='submit'] { background-color: #007bff; color: white; padding: 10px 20px; border: none; border-radius: 4px; cursor: pointer; }
        input[type='submit']:hover { background-color: #0056b3; }
        .info { background-color: #e7f3ff; padding: 15px; border-radius: 4px; margin-bottom: 20px; }
    </style>
</head>
<body>
    <h2>Вхід до системи</h2>
    <div class='info'>
        <strong>Тестові облікові дані:</strong><br>
        Email: tom@itstep.org, Пароль: 12345<br>
        Email: bob@itstep.org, Пароль: 55555
    </div>
    <form method='post'>
        <div class='form-group'>
            <label for='email'>Email</label>
            <input type='email' id='email' name='email' required />
        </div>
        <div class='form-group'>
            <label for='password'>Пароль</label>
            <input type='password' id='password' name='password' required />
        </div>
        <input type='submit' value='Увійти' />
    </form>
</body>
</html>";
    await context.Response.WriteAsync(loginForm);
});

// Обробка логіну
app.MapPost("/login", async (string? returnUrl, HttpContext context, IUserService userService) =>
{
    var form = context.Request.Form;
    if (!form.ContainsKey("email") || !form.ContainsKey("password"))
        return Results.BadRequest("Email або пароль не вказані");

    string email = form["email"]!;
    string password = form["password"]!;

    // Перевіряємо облікові дані через наш сервіс
    var user = await userService.ValidateUserAsync(email, password);
    if (user is null)
        return Results.Unauthorized();

    // Створюємо claims для користувача
    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, user.Email),
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new("UserId", user.Id.ToString())
    };

    // Створюємо ClaimsIdentity та ClaimsPrincipal
    var claimsIdentity = new ClaimsIdentity(claims, CustomCookieAuthenticationDefaults.AuthenticationScheme);
    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

    // Виконуємо вхід через наш кастомний метод
    await context.CustomSignInAsync(claimsPrincipal);

    return Results.Redirect(returnUrl ?? "/");
});

// Вихід
app.MapGet("/logout", async (HttpContext context) =>
{
    await context.CustomSignOutAsync();
    return Results.Redirect("/login");
});

// Сторінка відмови в доступі
app.MapGet("/access-denied", () => Results.Text("Доступ заборонено", statusCode: 403));

// Захищений ресурс - головна сторінка
app.MapGet("/", (HttpContext context) =>
{
    if (!context.IsAuthenticated())
    {
        return Results.Redirect("/login");
    }

    var userName = context.GetUserName() ?? "Анонім";
    var userId = context.GetUserClaim("UserId") ?? "Невідомо";

    var html = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <title>ITStep - Головна сторінка</title>
    <style>
        body {{ font-family: Arial, sans-serif; max-width: 600px; margin: 50px auto; padding: 20px; }}
        .welcome {{ background-color: #d4edda; padding: 15px; border-radius: 4px; margin-bottom: 20px; }}
        .user-info {{ background-color: #f8f9fa; padding: 15px; border-radius: 4px; margin-bottom: 20px; }}
        .logout-btn {{ background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px; }}
        .logout-btn:hover {{ background-color: #c82333; }}
    </style>
</head>
<body>
    <div class='welcome'>
        <h2>Вітаємо в системі кастомної аутентифікації!</h2>
        <p>Ви успішно увійшли в систему.</p>
    </div>

    <div class='user-info'>
        <h3>Інформація про користувача:</h3>
        <p><strong>Email:</strong> {userName}</p>
        <p><strong>ID користувача:</strong> {userId}</p>
        <p><strong>Статус аутентифікації:</strong> Аутентифікований</p>
    </div>

    <a href='/logout' class='logout-btn'>Вийти з системи</a>
</body>
</html>";

    return Results.Content(html, "text/html; charset=utf-8");
});

app.Run();
