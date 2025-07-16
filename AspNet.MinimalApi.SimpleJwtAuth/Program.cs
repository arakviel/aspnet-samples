using System.Security.Claims;
using AspNet.MinimalApi.SimpleJwtAuth;

var builder = WebApplication.CreateBuilder(args);

// Простий JWT сервіс
var jwtSecret = "MySimpleSecretKeyForJwtTokenGeneration123456789";
var jwt = new SimpleJwt(jwtSecret); // 14 днів
builder.Services.AddSingleton(jwt);

// Додаємо CORS для фронтенду
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// JWT middleware
app.UseMiddleware<SimpleJwtMiddleware>();
app.UseCors();

app.Use(async (context, next) =>
{
    context.Response.Headers.Add(
        "Content-Security-Policy",
        "default-src 'self'; connect-src 'self' http://localhost:5000");
    await next();
});

// Простий in-memory список користувачів
var users = new List<User>
{
    new(1, "admin", BCrypt.Net.BCrypt.HashPassword("admin123"), "Admin"),
    new(2, "user", BCrypt.Net.BCrypt.HashPassword("user123"))
};

// === ЕНДПОІНТИ ===

// Головна сторінка
app.MapGet("/", () => new
{
    Message = "Simple JWT Auth API",
    Endpoints = new
    {
        Login = "POST /login",
        Register = "POST /register",
        Profile = "GET /profile",
        Protected = "GET /protected",
        Admin = "GET /admin"
    }
});

// Логін
app.MapPost("/login", (LoginRequest request) =>
{
    var user = users.FirstOrDefault(u => u.Username == request.Username);

    if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        return Results.BadRequest(new AuthResponse(false, "Невірний логін або пароль"));

    var token = jwt.GenerateToken(user.Id, user.Username, user.Role);
    var userInfo = new UserInfo(user.Id, user.Username, user.Role);

    return Results.Ok(new AuthResponse(true, "Успішний вхід", token, userInfo));
});

// Реєстрація
app.MapPost("/register", (RegisterRequest request) =>
{
    if (users.Any(u => u.Username == request.Username))
        return Results.BadRequest(new AuthResponse(false, "Користувач вже існує"));

    var newUser = new User(
        users.Count + 1,
        request.Username,
        BCrypt.Net.BCrypt.HashPassword(request.Password)
    );

    users.Add(newUser);

    var token = jwt.GenerateToken(newUser.Id, newUser.Username, newUser.Role);
    var userInfo = new UserInfo(newUser.Id, newUser.Username, newUser.Role);

    return Results.Ok(new AuthResponse(true, "Користувач створений", token, userInfo));
});

// Профіль (потрібна аутентифікація)
app.MapGet("/profile", (HttpContext context) =>
{
    if (context.User.Identity?.IsAuthenticated != true)
        return Results.Problem("Необхідна аутентифікація", statusCode: 401);

    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var username = context.User.FindFirst(ClaimTypes.Name)?.Value;
    var role = context.User.FindFirst(ClaimTypes.Role)?.Value;

    return Results.Ok(new
    {
        UserId = userId,
        Username = username,
        Role = role,
        Message = "Ваш профіль"
    });
});

// Захищений ресурс
app.MapGet("/protected", (HttpContext context) =>
{
    if (context.User.Identity?.IsAuthenticated != true)
        return Results.Problem("Необхідна аутентифікація", statusCode: 401);

    var username = context.User.FindFirst(ClaimTypes.Name)?.Value;

    return Results.Ok(new
    {
        Message = $"Привіт, {username}! Це захищений ресурс.",
        Timestamp = DateTime.UtcNow
    });
});

// Тільки для адміністраторів
app.MapGet("/admin", (HttpContext context) =>
{
    if (context.User.Identity?.IsAuthenticated != true)
        return Results.Problem("Необхідна аутентифікація", statusCode: 401);

    var role = context.User.FindFirst(ClaimTypes.Role)?.Value;

    if (role != "Admin") return Results.Problem("Доступ заборонено", statusCode: 403);

    return Results.Ok(new
    {
        Message = "Адміністративна панель",
        Users = users.Select(u => new
        {
            u.Id,
            u.Username,
            u.Role
        })
    });
});

app.Run();