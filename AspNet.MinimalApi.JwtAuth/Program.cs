using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using AspNet.MinimalApi.JwtAuth;

var builder = WebApplication.CreateBuilder(args);

// Конфігурація JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
var issuer = jwtSettings["Issuer"] ?? "AspNet.MinimalApi.JwtAuth";
var audience = jwtSettings["Audience"] ?? "AspNet.MinimalApi.JwtAuth.Users";

// Додаємо сервіси
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<UserService>();

// Налаштування JWT аутентифікації (вбудована в ASP.NET Core)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        // Логування для debugging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("JWT Authentication failed: {Exception}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                var username = context.Principal?.FindFirst(ClaimTypes.Name)?.Value;
                logger.LogDebug("JWT Token validated for user: {Username}", username);
                return Task.CompletedTask;
            }
        };
    });

// Додаємо авторизацію
builder.Services.AddAuthorization(options =>
{
    // Політика для адміністраторів
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Admin"));
    
    // Політика для менеджерів та адміністраторів
    options.AddPolicy("ManagerOrAdmin", policy => 
        policy.RequireRole("Manager", "Admin"));
});

var app = builder.Build();

// Використовуємо аутентифікацію та авторизацію
app.UseAuthentication();
app.UseAuthorization();

// === ЕНДПОІНТИ ===

// Головна сторінка
app.MapGet("/", () => new
{
    Message = "ASP.NET Core JWT Authentication API",
    Info = "Використовує вбудовані можливості ASP.NET Core",
    Endpoints = new
    {
        Login = "POST /auth/login",
        Register = "POST /auth/register",
        Profile = "GET /auth/profile",
        Protected = "GET /protected",
        Admin = "GET /admin",
        Manager = "GET /manager",
        TokenInfo = "GET /auth/token-info"
    },
    Features = new[]
    {
        "Microsoft.AspNetCore.Authentication.JwtBearer",
        "TokenValidationParameters",
        "Authorization Policies",
        "Claims-based Authorization",
        "Automatic Token Validation"
    }
});

// Логін
app.MapPost("/auth/login", async (LoginRequest request, UserService userService, JwtService jwtService) =>
{
    var user = userService.Authenticate(request.Username, request.Password);
    
    if (user == null)
    {
        return Results.BadRequest(new AuthResponse(false, "Невірний логін або пароль"));
    }

    var token = jwtService.GenerateToken(user);
    var userInfo = new UserInfo(user.Id, user.Username, user.Email ?? "", user.Role);
    var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.GetValue<int>("ExpirationMinutes", 60));

    return Results.Ok(new TokenResponse(token, "Bearer", expiresAt, userInfo));
});

// Реєстрація
app.MapPost("/auth/register", async (RegisterRequest request, UserService userService, JwtService jwtService) =>
{
    var user = userService.Register(request.Username, request.Password, request.Email);
    
    if (user == null)
    {
        return Results.BadRequest(new AuthResponse(false, "Користувач з таким ім'ям або email вже існує"));
    }

    var token = jwtService.GenerateToken(user);
    var userInfo = new UserInfo(user.Id, user.Username, user.Email ?? "", user.Role);
    var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.GetValue<int>("ExpirationMinutes", 60));

    return Results.Ok(new TokenResponse(token, "Bearer", expiresAt, userInfo));
});

// Профіль користувача (потрібна аутентифікація)
app.MapGet("/auth/profile", [Authorize] (ClaimsPrincipal user) =>
{
    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var username = user.FindFirst(ClaimTypes.Name)?.Value;
    var email = user.FindFirst(ClaimTypes.Email)?.Value;
    var role = user.FindFirst(ClaimTypes.Role)?.Value;

    return Results.Ok(new
    {
        UserId = userId,
        Username = username,
        Email = email,
        Role = role,
        Message = "Ваш профіль",
        AuthMethod = "ASP.NET Core JWT Bearer"
    });
});

// Інформація про токен
app.MapGet("/auth/token-info", [Authorize] (ClaimsPrincipal user, JwtService jwtService, HttpContext context) =>
{
    var token = context.Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer ", "");

    if (string.IsNullOrEmpty(token))
    {
        return Results.BadRequest("Токен не знайдено");
    }

    var jwtToken = jwtService.ReadToken(token);

    if (jwtToken == null)
    {
        return Results.BadRequest("Недійсний токен");
    }

    return Results.Ok(new
    {
        Header = jwtToken.Header,
        Payload = jwtToken.Payload,
        ValidFrom = jwtToken.ValidFrom,
        ValidTo = jwtToken.ValidTo,
        Issuer = jwtToken.Issuer,
        Audiences = jwtToken.Audiences,
        Claims = jwtToken.Claims.Select(c => new { c.Type, c.Value })
    });
});

// Захищений ресурс (будь-який аутентифікований користувач)
app.MapGet("/protected", [Authorize] (ClaimsPrincipal user) =>
{
    var username = user.FindFirst(ClaimTypes.Name)?.Value;
    var role = user.FindFirst(ClaimTypes.Role)?.Value;

    return Results.Ok(new
    {
        Message = $"Привіт, {username}! Це захищений ресурс.",
        Role = role,
        AuthMethod = "ASP.NET Core JWT Bearer",
        Timestamp = DateTime.UtcNow
    });
});

// Тільки для адміністраторів (використовує політику)
app.MapGet("/admin", [Authorize(Policy = "AdminOnly")] (ClaimsPrincipal user, UserService userService) =>
{
    var username = user.FindFirst(ClaimTypes.Name)?.Value;
    var users = userService.GetAllUsers();

    return Results.Ok(new
    {
        Message = $"Адміністративна панель - {username}",
        AuthMethod = "ASP.NET Core JWT Bearer + Policy",
        Users = users,
        TotalUsers = users.Count
    });
});

// Для менеджерів та адміністраторів (використовує політику)
app.MapGet("/manager", [Authorize(Policy = "ManagerOrAdmin")] (ClaimsPrincipal user) =>
{
    var username = user.FindFirst(ClaimTypes.Name)?.Value;
    var role = user.FindFirst(ClaimTypes.Role)?.Value;

    return Results.Ok(new
    {
        Message = $"Панель менеджера - {username}",
        Role = role,
        AuthMethod = "ASP.NET Core JWT Bearer + Policy",
        Features = new[]
        {
            "Перегляд звітів",
            "Управління користувачами",
            "Налаштування системи"
        }
    });
});

// Тільки для конкретної ролі (альтернативний спосіб)
app.MapGet("/user-only", [Authorize(Roles = "User")] (ClaimsPrincipal user) =>
{
    var username = user.FindFirst(ClaimTypes.Name)?.Value;

    return Results.Ok(new
    {
        Message = $"Ресурс тільки для звичайних користувачів - {username}",
        AuthMethod = "ASP.NET Core JWT Bearer + Roles",
        Info = "Цей ендпоінт доступний тільки користувачам з роллю 'User'"
    });
});

// Публічний ендпоінт (без аутентифікації)
app.MapGet("/public", () => new
{
    Message = "Це публічний ендпоінт",
    Info = "Доступний без аутентифікації",
    Timestamp = DateTime.UtcNow
});

// Статус аутентифікації
app.MapGet("/auth/status", (ClaimsPrincipal user) =>
{
    if (user.Identity?.IsAuthenticated == true)
    {
        var username = user.FindFirst(ClaimTypes.Name)?.Value;
        var role = user.FindFirst(ClaimTypes.Role)?.Value;

        return Results.Ok(new
        {
            IsAuthenticated = true,
            Username = username,
            Role = role,
            AuthMethod = "ASP.NET Core JWT Bearer",
            Message = "Користувач аутентифікований"
        });
    }

    return Results.Ok(new
    {
        IsAuthenticated = false,
        Message = "Користувач не аутентифікований",
        AuthMethod = "ASP.NET Core JWT Bearer"
    });
});

app.Run();
