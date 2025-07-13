using AspNet.MinimalApi.CustomJwtAuth.Data;
using AspNet.MinimalApi.CustomJwtAuth.Extensions;
using AspNet.MinimalApi.CustomJwtAuth.Models;
using AspNet.MinimalApi.CustomJwtAuth.Services;

var builder = WebApplication.CreateBuilder(args);

// Додавання кастомної JWT аутентифікації
builder.Services.AddCustomJwtAuthentication(builder.Configuration);

var app = builder.Build();

// Ініціалізація бази даних
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await context.Database.EnsureCreatedAsync();
}

// Додавання кастомного JWT middleware
app.UseCustomJwtAuthentication();

// Головна сторінка
app.MapGet("/", () => new
{
    Message = "ASP.NET Minimal API - Custom JWT Authentication",
    Version = "1.0.0",
    Documentation = "/swagger",
    Endpoints = new
    {
        Register = "POST /auth/register",
        Login = "POST /auth/login",
        Refresh = "POST /auth/refresh",
        Profile = "GET /auth/profile",
        Users = "GET /users (Admin only)",
        Protected = "GET /protected"
    }
});

// === ЕНДПОІНТИ АУТЕНТИФІКАЦІЇ ===

// Реєстрація нового користувача
app.MapPost("/auth/register", async (RegisterRequest request, IUserService userService) =>
    {
        var result = await userService.RegisterAsync(request);

        if (!result.Success)
        {
            return Results.BadRequest(result);
        }

        return Results.Ok(result);
    })
    .WithName("Register")
    .WithSummary("Реєстрація нового користувача")
    .WithDescription("Створює новий обліковий запис користувача та повертає JWT токени");

// Вхід в систему
app.MapPost("/auth/login", async (LoginRequest request, IUserService userService) =>
    {
        var result = await userService.LoginAsync(request);

        if (!result.Success)
        {
            return Results.BadRequest(result);
        }

        return Results.Ok(result);
    })
    .WithName("Login")
    .WithSummary("Вхід в систему")
    .WithDescription("Аутентифікує користувача та повертає JWT токени");

// Оновлення токена
app.MapPost("/auth/refresh", async (RefreshTokenRequest request, IUserService userService) =>
    {
        var result = await userService.RefreshTokenAsync(request.RefreshToken);

        if (!result.Success)
        {
            return Results.BadRequest(result);
        }

        return Results.Ok(result);
    })
    .WithName("RefreshToken")
    .WithSummary("Оновлення токена доступу")
    .WithDescription("Оновлює токен доступу за допомогою токена оновлення");

// Профіль користувача (захищений ендпоінт)
app.MapGet("/auth/profile", async (HttpContext context, IUserService userService) =>
    {
        if (!context.IsAuthenticated())
        {
            return JwtAuthenticationExtensions.Unauthorized();
        }

        var userIdStr = context.GetUserId();
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
        {
            return JwtAuthenticationExtensions.Unauthorized("Невірний токен");
        }

        var user = await userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return JwtAuthenticationExtensions.Unauthorized("Користувач не знайдений");
        }

        var userInfo = new UserInfo(
            user.Id,
            user.Username,
            user.Email,
            user.Role,
            user.CreatedAt,
            user.LastLoginAt
        );

        return Results.Ok(userInfo);
    })
    .WithName("GetProfile")
    .WithSummary("Отримання профілю користувача")
    .WithDescription("Повертає інформацію про аутентифікованого користувача");

// === ЗАХИЩЕНІ ЕНДПОІНТИ ===

// Простий захищений ендпоінт
app.MapGet("/protected", (HttpContext context) =>
    {
        if (!context.IsAuthenticated())
        {
            return JwtAuthenticationExtensions.Unauthorized();
        }

        var username = context.GetUsername();
        var role = context.GetUserRole();

        return Results.Ok(new
        {
            Message = $"Привіт, {username}! Це захищений ресурс.",
            Role = role,
            Timestamp = DateTime.UtcNow
        });
    })
    .WithName("Protected")
    .WithSummary("Захищений ресурс")
    .WithDescription("Демонструє роботу JWT аутентифікації");

// Список всіх користувачів (тільки для адміністраторів)
app.MapGet("/users", async (HttpContext context, IUserService userService) =>
    {
        if (!context.IsAuthenticated())
        {
            return JwtAuthenticationExtensions.Unauthorized();
        }

        if (!context.IsAdmin())
        {
            return JwtAuthenticationExtensions.Forbidden("Тільки адміністратори можуть переглядати список користувачів");
        }

        var users = await userService.GetAllUsersAsync();
        return Results.Ok(users);
    })
    .WithName("GetAllUsers")
    .WithSummary("Список всіх користувачів")
    .WithDescription("Повертає список всіх користувачів (тільки для адміністраторів)");

// Ендпоінт для перевірки статусу аутентифікації
app.MapGet("/auth/status", (HttpContext context) =>
    {
        var isAuthenticated = context.IsAuthenticated();

        if (!isAuthenticated)
        {
            return Results.Ok(new
            {
                IsAuthenticated = false,
                Message = "Користувач не аутентифікований"
            });
        }

        return Results.Ok(new
        {
            IsAuthenticated = true,
            UserId = context.GetUserId(),
            Username = context.GetUsername(),
            Role = context.GetUserRole(),
            Message = "Користувач аутентифікований"
        });
    })
    .WithName("AuthStatus")
    .WithSummary("Статус аутентифікації")
    .WithDescription("Перевіряє статус аутентифікації поточного користувача");

// === АДМІНІСТРАТИВНІ ЕНДПОІНТИ ===

// Ендпоінт тільки для адміністраторів
app.MapGet("/admin/dashboard", (HttpContext context) =>
    {
        if (!context.IsAuthenticated())
        {
            return JwtAuthenticationExtensions.Unauthorized();
        }

        if (!context.IsAdmin())
        {
            return JwtAuthenticationExtensions.Forbidden("Доступ тільки для адміністраторів");
        }

        return Results.Ok(new
        {
            Message = "Ласкаво просимо до адміністративної панелі!",
            AdminUser = context.GetUsername(),
            ServerTime = DateTime.UtcNow,
            Features = new[]
            {
                "Управління користувачами",
                "Перегляд логів",
                "Налаштування системи"
            }
        });
    })
    .WithName("AdminDashboard")
    .WithSummary("Адміністративна панель")
    .WithDescription("Доступ тільки для користувачів з роллю Admin");

// === ДЕМОНСТРАЦІЙНІ ЕНДПОІНТИ ===

// Публічний ендпоінт (без аутентифікації)
app.MapGet("/public", () => new
    {
        Message = "Це публічний ендпоінт, доступний без аутентифікації",
        Timestamp = DateTime.UtcNow,
        Info = "Для доступу до захищених ресурсів потрібна аутентифікація"
    })
    .WithName("Public")
    .WithSummary("Публічний ендпоінт")
    .WithDescription("Демонструє публічний ресурс без аутентифікації");

// Ендпоінт для тестування різних ролей
app.MapGet("/roles/test", (HttpContext context) =>
    {
        if (!context.IsAuthenticated())
        {
            return JwtAuthenticationExtensions.Unauthorized();
        }

        var role = context.GetUserRole();
        var message = role switch
        {
            "Admin" => "Ви маєте повний доступ до системи",
            "Moderator" => "Ви можете модерувати контент",
            "User" => "Ви маєте базовий доступ",
            _ => "Невідома роль"
        };

        return Results.Ok(new
        {
            Role = role,
            Message = message,
            Permissions = GetPermissionsForRole(role)
        });
    })
    .WithName("TestRoles")
    .WithSummary("Тестування ролей")
    .WithDescription("Демонструє роботу з різними ролями користувачів");

app.Run();

// Допоміжний метод для отримання дозволів за роллю
static string[] GetPermissionsForRole(string? role) => role switch
{
    "Admin" => new[]
    {
        "read",
        "write",
        "delete",
        "admin"
    },
    "Moderator" => new[]
    {
        "read",
        "write",
        "moderate"
    },
    "User" => new[]
    {
        "read"
    },
    _ => Array.Empty<string>()
};
