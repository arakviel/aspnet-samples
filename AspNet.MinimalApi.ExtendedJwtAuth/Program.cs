using System.Security.Claims;
using AspNet.MinimalApi.ExtendedJwtAuth.Data;
using AspNet.MinimalApi.ExtendedJwtAuth.Extensions;
using AspNet.MinimalApi.ExtendedJwtAuth.Models;
using AspNet.MinimalApi.ExtendedJwtAuth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// Створюємо конструктор додатку
var builder = WebApplication.CreateBuilder(args);

// === НАЛАШТУВАННЯ СЕРВІСІВ ===

// Додаємо базу даних
builder.Services.AddDatabaseServices(builder.Configuration);

// Додаємо Identity
builder.Services.AddIdentityServices();

// Додаємо JWT аутентифікацію
builder.Services.AddJwtAuthentication(builder.Configuration);

// Додаємо авторизацію
builder.Services.AddAuthorization();

// Додаємо кастомні сервіси
builder.Services.AddCustomServices();

// Додаємо Swagger з підтримкою JWT
builder.Services.AddSwaggerWithJwt();

// Створюємо додаток
var app = builder.Build();

// === НАЛАШТУВАННЯ MIDDLEWARE ===

// Застосовуємо міграції в режимі розробки
if (app.Environment.IsDevelopment())
{
    app.EnsureDatabaseCreated();
    //await app.ApplyMigrationsAsync();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Detailed JWT Auth API v1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
    });
}

// Використовуємо HTTPS редирект
app.UseHttpsRedirection();

// Використовуємо аутентифікацію та авторизацію
app.UseAuthentication();
app.UseAuthorization();

// Додаємо сервіс очищення токенів
app.AddTokenCleanupService();

// === ЕНДПОІНТИ ===

// Головна сторінка з інформацією про API
app.MapGet("/", () => new
    {
        Message = "ASP.NET Core Detailed JWT Authentication API",
        Description = "Детальна реалізація JWT аутентифікації з використанням UserManager та Identity",
        Version = "1.0",
        Features = new[]
        {
            "Детальна реалізація з UserManager<User>",
            "JWT Access токени",
            "Refresh токени з базою даних",
            "Автоматичне очищення застарілих токенів",
            "Детальна обробка помилок",
            "Логування всіх операцій",
            "Swagger документація з JWT підтримкою"
        },
        Endpoints = new
        {
            // Аутентифікація
            Register = "POST /auth/register",
            Login = "POST /auth/login",
            RefreshToken = "POST /auth/refresh",
            Logout = "POST /auth/logout",

            // Профіль користувача
            GetProfile = "GET /auth/profile",
            UpdateProfile = "PUT /auth/profile",
            ChangePassword = "PUT /auth/change-password",

            // Користувачі
            GetUsers = "GET /users"
        },
        AuthenticationInfo = new
        {
            Type = "JWT Bearer Token",
            Usage = "Додайте заголовок: Authorization: Bearer <your_access_token>",
            AccessTokenExpiration = "60 хвилин",
            RefreshTokenExpiration = "7 днів",
            Note = "Використовуйте /auth/refresh для оновлення токена"
        }
    })
    .WithName("GetApiInfo")
    .WithSummary("Інформація про API")
    .WithDescription("Повертає інформацію про доступні ендпоінти та функції API")
    .WithTags("Info");

// === ЕНДПОІНТИ АУТЕНТИФІКАЦІЇ ===

// Реєстрація користувача
app.MapPost("/auth/register", async (RegisterRequest request, AuthService authService) =>
    {
        var result = await authService.RegisterAsync(request);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    })
    .WithName("Register")
    .WithSummary("Реєстрація користувача")
    .WithDescription("Створює нового користувача та повертає JWT токени")
    .WithTags("Authentication")
    .Produces<ApiResponse<AuthResponse>>()
    .Produces<ApiResponse<AuthResponse>>(400);

// Вхід в систему
app.MapPost("/auth/login", async (LoginRequest request, AuthService authService) =>
    {
        var result = await authService.LoginAsync(request);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    })
    .WithName("Login")
    .WithSummary("Вхід в систему")
    .WithDescription("Аутентифікує користувача та повертає JWT токени")
    .WithTags("Authentication")
    .Produces<ApiResponse<AuthResponse>>()
    .Produces<ApiResponse<AuthResponse>>(400);

// Оновлення токена
app.MapPost("/auth/refresh", async (RefreshTokenRequest request, AuthService authService) =>
    {
        var result = await authService.RefreshTokenAsync(request);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    })
    .WithName("RefreshToken")
    .WithSummary("Оновлення токена")
    .WithDescription("Оновлює access токен за допомогою refresh токена")
    .WithTags("Authentication")
    .Produces<ApiResponse<AuthResponse>>()
    .Produces<ApiResponse<AuthResponse>>(400);

// Вихід з системи
app.MapPost("/auth/logout", async (RefreshTokenRequest request, AuthService authService) =>
    {
        var result = await authService.LogoutAsync(request.RefreshToken);
        return result.Success ? Results.Ok(result) : Results.BadRequest(result);
    })
    .WithName("Logout")
    .WithSummary("Вихід з системи")
    .WithDescription("Відкликає refresh токен")
    .WithTags("Authentication")
    .Produces<ApiResponse>()
    .Produces<ApiResponse>(400);

// === ЕНДПОІНТИ ПРОФІЛЮ ===

// Отримання профілю поточного користувача
app.MapGet("/auth/profile", async (ClaimsPrincipal claims, ApplicationDbContext context) =>
    {
        var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        var user = await context.Users.FindAsync(userId);
        if (user == null)
            return Results.NotFound(new ApiResponse { Success = false, Message = "Користувача не знайдено" });

        var userInfo = new UserInfo
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };

        return Results.Ok(new ApiResponse<UserInfo>
        {
            Success = true,
            Message = "Профіль користувача",
            Data = userInfo
        });
    })
    .RequireAuthorization()
    .WithName("GetProfile")
    .WithSummary("Отримати профіль користувача")
    .WithDescription("Повертає профіль поточного аутентифікованого користувача")
    .WithTags("Profile")
    .Produces<ApiResponse<UserInfo>>()
    .Produces(401);

// Оновлення профілю користувача
app.MapPut("/auth/profile", async (
        ClaimsPrincipal claims,
        UpdateProfileRequest request,
        UserManager<User> userManager) =>
    {
        var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return Results.NotFound(new ApiResponse { Success = false, Message = "Користувача не знайдено" });

        // Оновлюємо дані користувача
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;

        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return Results.BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Помилка при оновленні профілю",
                Errors = result.Errors.Select(e => e.Description).ToList()
            });

        return Results.Ok(new ApiResponse
        {
            Success = true,
            Message = "Профіль успішно оновлено"
        });
    })
    .RequireAuthorization()
    .WithName("UpdateProfile")
    .WithSummary("Оновити профіль користувача")
    .WithDescription("Оновлює дані профілю поточного користувача")
    .WithTags("Profile")
    .Produces<ApiResponse>()
    .Produces<ApiResponse>(400)
    .Produces(401);

// Зміна пароля
app.MapPut("/auth/change-password", async (
        ClaimsPrincipal claims,
        ChangePasswordRequest request,
        UserManager<User> userManager) =>
    {
        var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return Results.NotFound(new ApiResponse { Success = false, Message = "Користувача не знайдено" });

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
            return Results.BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Помилка при зміні пароля",
                Errors = result.Errors.Select(e => e.Description).ToList()
            });

        return Results.Ok(new ApiResponse
        {
            Success = true,
            Message = "Пароль успішно змінено"
        });
    })
    .RequireAuthorization()
    .WithName("ChangePassword")
    .WithSummary("Зміна пароля")
    .WithDescription("Змінює пароль поточного користувача")
    .WithTags("Profile")
    .Produces<ApiResponse>()
    .Produces<ApiResponse>(400)
    .Produces(401);

// === ЕНДПОІНТИ КОРИСТУВАЧІВ ===

// Отримання списку всіх користувачів
app.MapGet("/users", async (ApplicationDbContext context) =>
    {
        var users = await context.Users
            .Select(u => new UserInfo
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                UserName = u.UserName,
                FirstName = u.FirstName,
                LastName = u.LastName,
                FullName = u.FullName,
                EmailConfirmed = u.EmailConfirmed,
                PhoneNumber = u.PhoneNumber,
                PhoneNumberConfirmed = u.PhoneNumberConfirmed,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            })
            .ToListAsync();

        return Results.Ok(new ApiResponse<List<UserInfo>>
        {
            Success = true,
            Message = "Список користувачів",
            Data = users
        });
    })
    .RequireAuthorization()
    .WithName("GetUsers")
    .WithSummary("Отримати список користувачів")
    .WithDescription("Повертає список всіх зареєстрованих користувачів")
    .WithTags("Users")
    .Produces<ApiResponse<List<UserInfo>>>()
    .Produces(401);

// Запуск додатку
app.Run();