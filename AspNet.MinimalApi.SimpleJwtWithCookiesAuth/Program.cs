using AspNet.MinimalApi.SimpleJwtWithCookiesAuth;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Простий JWT сервіс для cookies
var jwtSecret = "MySimpleSecretKeyForJwtCookiesGeneration123456789";
var jwt = new SimpleJwtCookies(jwtSecret, "SimpleJwtCookiesAuth", 14); // 14 днів
builder.Services.AddSingleton(jwt);

var app = builder.Build();

// JWT cookies middleware
app.UseMiddleware<SimpleJwtCookiesMiddleware>();

// Простий in-memory список користувачів
var users = new List<User>
{
    new(1, "admin", BCrypt.Net.BCrypt.HashPassword("admin123"), "Admin"),
    new(2, "user", BCrypt.Net.BCrypt.HashPassword("user123"), "User")
};

// === ЕНДПОІНТИ ===

// Головна сторінка
app.MapGet("/", () => new
{
    Message = "Simple JWT Auth with Cookies API",
    Info = "Токени зберігаються в HttpOnly cookies",
    Endpoints = new
    {
        Login = "POST /login",
        Register = "POST /register",
        Profile = "GET /profile",
        Protected = "GET /protected",
        Admin = "GET /admin",
        Logout = "POST /logout"
    }
});

// Логін - токен зберігається в cookie
app.MapPost("/login", (LoginRequest request, HttpContext context) =>
{
    var user = users.FirstOrDefault(u => u.Username == request.Username);

    if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
    {
        return Results.BadRequest(new AuthResponse(false, "Невірний логін або пароль"));
    }

    // Генеруємо токен та зберігаємо в HttpOnly cookie
    var token = jwt.GenerateToken(user.Id, user.Username, user.Role);
    context.SetJwtCookie(token, 14);

    var userInfo = new UserInfo(user.Id, user.Username, user.Role);
    return Results.Ok(new AuthResponse(true, "Успішний вхід", userInfo));
});

// Реєстрація - токен зберігається в cookie
app.MapPost("/register", (RegisterRequest request, HttpContext context) =>
{
    if (users.Any(u => u.Username == request.Username))
    {
        return Results.BadRequest(new AuthResponse(false, "Користувач вже існує"));
    }

    var newUser = new User(
        users.Count + 1,
        request.Username,
        BCrypt.Net.BCrypt.HashPassword(request.Password)
    );

    users.Add(newUser);

    // Генеруємо токен та зберігаємо в HttpOnly cookie
    var token = jwt.GenerateToken(newUser.Id, newUser.Username, newUser.Role);
    context.SetJwtCookie(token, 14);

    var userInfo = new UserInfo(newUser.Id, newUser.Username, newUser.Role);
    return Results.Ok(new AuthResponse(true, "Користувач створений", userInfo));
});

// Логаут - видаляємо cookie
app.MapPost("/logout", (HttpContext context) =>
{
    context.DeleteJwtCookie();
    return Results.Ok(new { Message = "Успішний вихід" });
});

// Профіль (потрібна аутентифікація)
app.MapGet("/profile", (HttpContext context) =>
{
    if (!context.IsAuthenticated())
    {
        return Results.Problem("Необхідна аутентифікація", statusCode: 401);
    }

    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var username = context.User.FindFirst(ClaimTypes.Name)?.Value;
    var role = context.User.FindFirst(ClaimTypes.Role)?.Value;

    return Results.Ok(new
    {
        UserId = userId,
        Username = username,
        Role = role,
        Message = "Ваш профіль",
        AuthMethod = "HttpOnly Cookie"
    });
});

// Захищений ресурс
app.MapGet("/protected", (HttpContext context) =>
{
    if (!context.IsAuthenticated())
    {
        return Results.Problem("Необхідна аутентифікація", statusCode: 401);
    }

    var username = context.User.FindFirst(ClaimTypes.Name)?.Value;

    return Results.Ok(new
    {
        Message = $"Привіт, {username}! Це захищений ресурс.",
        AuthMethod = "HttpOnly Cookie",
        Timestamp = DateTime.UtcNow
    });
});

// Тільки для адміністраторів
app.MapGet("/admin", (HttpContext context) =>
{
    if (!context.IsAuthenticated())
    {
        return Results.Problem("Необхідна аутентифікація", statusCode: 401);
    }

    if (!context.IsAdmin())
    {
        return Results.Problem("Доступ заборонено", statusCode: 403);
    }

    return Results.Ok(new
    {
        Message = "Адміністративна панель",
        AuthMethod = "HttpOnly Cookie",
        Users = users.Select(u => new { u.Id, u.Username, u.Role })
    });
});

// Статус аутентифікації
app.MapGet("/auth/status", (HttpContext context) =>
{
    if (!context.IsAuthenticated())
    {
        return Results.Ok(new
        {
            IsAuthenticated = false,
            Message = "Користувач не аутентифікований",
            AuthMethod = "HttpOnly Cookie"
        });
    }

    var username = context.User.FindFirst(ClaimTypes.Name)?.Value;
    var role = context.User.FindFirst(ClaimTypes.Role)?.Value;

    return Results.Ok(new
    {
        IsAuthenticated = true,
        Username = username,
        Role = role,
        Message = "Користувач аутентифікований",
        AuthMethod = "HttpOnly Cookie"
    });
});

app.Run();
