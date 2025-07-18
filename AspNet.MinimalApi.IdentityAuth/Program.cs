using System.Security.Claims;
using AspNet.MinimalApi.IdentityAuth.Data;
using AspNet.MinimalApi.IdentityAuth.Extensions;
using AspNet.MinimalApi.IdentityAuth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

// Створюємо конструктор додатку
var builder = WebApplication.CreateBuilder(args);

// === НАЛАШТУВАННЯ СЕРВІСІВ ===

// Додаємо Swagger для документації API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ASP.NET Core Identity Auth API",
        Version = "v1",
        Description = "Приклад аутентифікації через ASP.NET Core Identity в Minimal API"
    });
});

// Налаштування бази даних
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Використовуємо SQLite для простоти (у продакшені краще PostgreSQL/SQL Server)
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                           ?? "Data Source=identity.db";
    options.UseSqlite(connectionString);
});

// === НАЛАШТУВАННЯ ASP.NET CORE IDENTITY ===

// Додаємо авторизацію
builder.Services.AddAuthorization();

// Налаштування аутентифікації
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
// === COOKIE АУТЕНТИФІКАЦІЯ (ЗАКОМЕНТОВАНО) ===
// Розкоментуйте наступний блок для активації cookie аутентифікації для веб-додатків:
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        // Налаштування cookie для веб-додатків
        options.LoginPath = "/auth/login"; // Сторінка логіну
        options.LogoutPath = "/auth/logout"; // Сторінка виходу
        options.AccessDeniedPath = "/access-denied"; // Сторінка відмови в доступі
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Термін дії cookie
        options.SlidingExpiration = true; // Продовжувати термін дії при активності
        options.Cookie.HttpOnly = true; // Захист від XSS атак
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS у продакшені
        options.Cookie.SameSite = SameSiteMode.Lax; // Захист від CSRF
    });
/*builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = IdentityConstants.BearerScheme;
        options.DefaultChallengeScheme = IdentityConstants.BearerScheme;
    })
    .AddBearerToken(IdentityConstants.BearerScheme, options =>
    {
        // Налаштування Bearer токенів для API
        options.BearerTokenExpiration = TimeSpan.FromHours(1); // Термін дії токена
        options.RefreshTokenExpiration = TimeSpan.FromDays(7); // Термін дії refresh токена
    });*/

// Налаштування ASP.NET Core Identity
builder.Services.AddIdentityCore<User>(options =>
    {
        // Налаштування паролів
        options.Password.RequireDigit = true; // Вимагати цифри
        options.Password.RequireLowercase = true; // Вимагати малі літери
        options.Password.RequireUppercase = true; // Вимагати великі літери
        options.Password.RequireNonAlphanumeric = false; // Не вимагати спеціальні символи
        options.Password.RequiredLength = 6; // Мінімальна довжина пароля

        // Налаштування користувача
        options.User.RequireUniqueEmail = true; // Унікальна електронна пошта

        // Налаштування блокування
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // Час блокування
        options.Lockout.MaxFailedAccessAttempts = 5; // Максимум невдалих спроб
        options.Lockout.AllowedForNewUsers = true; // Дозволити блокування нових користувачів

        // Налаштування підтвердження
        options.SignIn.RequireConfirmedEmail = false; // Не вимагати підтвердження email
        options.SignIn.RequireConfirmedPhoneNumber = false; // Не вимагати підтвердження телефону
    })
    .AddEntityFrameworkStores<ApplicationDbContext>() // Використовуємо Entity Framework
    .AddApiEndpoints(); // Додаємо стандартні API ендпоінти Identity

// Створюємо додаток
var app = builder.Build();

// === НАЛАШТУВАННЯ MIDDLEWARE ===

// Застосовуємо міграції в режимі розробки
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Auth API v1");
        c.RoutePrefix = "swagger"; // Swagger буде доступний за адресою /swagger
    });

    // Автоматично застосовуємо міграції
    app.EnsureDatabaseCreated();
}

// Використовуємо HTTPS редирект
//app.UseHttpsRedirection();

// Використовуємо аутентифікацію та авторизацію
app.UseAuthentication();
app.UseAuthorization();

// === ЕНДПОІНТИ ===

// Головна сторінка з інформацією про API
app.MapGet("/", () => new
    {
        Message = "ASP.NET Core Identity Authentication API",
        Description = "Приклад використання ASP.NET Core Identity в Minimal API",
        Version = "1.0",
        Endpoints = new
        {
            // Стандартні ендпоінти Identity (автоматично створені)
            // Використовують Bearer Token аутентифікацію (Authorization: Bearer <token>)
            Register = "POST /register",
            Login = "POST /login",
            Refresh = "POST /refresh",
            ConfirmEmail = "GET /confirmEmail",
            ResendConfirmationEmail = "POST /resendConfirmationEmail",
            ForgotPassword = "POST /forgotPassword",
            ResetPassword = "POST /resetPassword",
            Manage_2fa = "POST /manage/2fa",
            Manage_Info = "GET /manage/info",

            // Кастомні ендпоінти
            UserProfile = "GET /auth/profile",
            AllUsers = "GET /auth/users",
            UpdateProfile = "PUT /auth/profile"
        },
        AuthenticationInfo = new
        {
            CurrentMode = "Bearer Token Only",
            Note = "Cookie аутентифікація закоментована. Для активації розкоментуйте .AddCookie() в Program.cs",
            BearerTokenUsage = "Додайте заголовок: Authorization: Bearer <your_token>",
            CookieUsage = "Після активації cookie auth, токени будуть автоматично зберігатися в cookies"
        },
        Features = new[]
        {
            "ASP.NET Core Identity",
            "Entity Framework Core",
            "SQLite Database",
            "Bearer Token Authentication",
            "Cookie Authentication",
            "Password Hashing",
            "User Management",
            "Automatic API Endpoints"
        }
    })
    .WithName("GetApiInfo")
    .WithSummary("Інформація про API")
    .WithDescription("Повертає інформацію про доступні ендпоінти та функції API");

// Мапимо стандартні ендпоінти Identity
// Це автоматично створює: /register, /login, /refresh, /confirmEmail тощо
//app.MapIdentityApi<User>();

// Реєстрація користувача
app.MapPost("/auth/register", async (UserManager<User> userManager, RegisterRequest request) =>
    {
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (result.Succeeded) return Results.Ok(new { Message = "Реєстрація успішна" });

        return Results.BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
    })
    .WithName("Register")
    .WithSummary("Реєстрація нового користувача")
    .WithDescription("Створює нового користувача та повертає результат");

// Логін користувача
app.MapPost("/auth/login", async (SignInManager<User> signInManager, LoginRequest request) =>
    {
        var user = await signInManager.UserManager.FindByEmailAsync(request.Email);
        if (user == null) return Results.Unauthorized();

        var result = await signInManager.PasswordSignInAsync(user, request.Password, true, true);
        if (result.Succeeded) return Results.Ok(new { Message = "Логін успішний" });

        return Results.Unauthorized();
    })
    .WithName("Login")
    .WithSummary("Логін користувача")
    .WithDescription("Аутентифікує користувача та створює cookie");

// Вихід користувача
app.MapPost("/auth/logout", async (SignInManager<User> signInManager) =>
    {
        await signInManager.SignOutAsync();
        return Results.Ok(new { Message = "Вихід успішний" });
    })
    .WithName("Logout")
    .WithSummary("Вихід користувача")
    .WithDescription("Завершує сесію користувача та видаляє cookie");

// === КАСТОМНІ ЕНДПОІНТИ ===

// Отримання профілю поточного користувача
app.MapGet("/auth/profile", async (ClaimsPrincipal claims, ApplicationDbContext context) =>
    {
        // Отримуємо ID користувача з claims
        var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        // Знаходимо користувача в базі даних
        var user = await context.Users.FindAsync(userId);

        if (user == null) return Results.NotFound(new { Message = "Користувача не знайдено" });

        // Повертаємо профіль користувача (без чутливих даних)
        return Results.Ok(new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.CreatedAt,
            user.LastLoginAt,
            user.EmailConfirmed,
            user.PhoneNumber,
            user.PhoneNumberConfirmed
        });
    })
    .RequireAuthorization() // Вимагає аутентифікації
    .WithName("GetUserProfile")
    .WithSummary("Отримати профіль користувача")
    .WithDescription("Повертає профіль поточного аутентифікованого користувача");

// Оновлення профілю користувача
app.MapPut("/auth/profile", async (
        ClaimsPrincipal claims,
        ApplicationDbContext context,
        UserProfileUpdateRequest request) =>
    {
        var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        var user = await context.Users.FindAsync(userId);

        if (user == null) return Results.NotFound(new { Message = "Користувача не знайдено" });

        // Оновлюємо дані користувача
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;

        await context.SaveChangesAsync();

        return Results.Ok(new { Message = "Профіль успішно оновлено" });
    })
    .RequireAuthorization()
    .WithName("UpdateUserProfile")
    .WithSummary("Оновити профіль користувача")
    .WithDescription("Оновлює дані профілю поточного користувача");

// Отримання списку всіх користувачів (для демонстрації)
app.MapGet("/auth/users", async (ApplicationDbContext context) =>
    {
        var users = await context.Users
            .Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.FirstName,
                u.LastName,
                u.FullName,
                u.CreatedAt,
                u.LastLoginAt,
                u.EmailConfirmed
            })
            .ToListAsync();

        return Results.Ok(users);
    })
    .RequireAuthorization()
    .WithName("GetAllUsers")
    .WithSummary("Отримати список користувачів")
    .WithDescription("Повертає список всіх зареєстрованих користувачів");

// Запуск додатку
app.Run();

// === МОДЕЛІ ЗАПИТІВ ===

/// <summary>
///     Модель для оновлення профілю користувача
/// </summary>
/// <param name="FirstName">Ім'я</param>
/// <param name="LastName">Прізвище</param>
/// <param name="PhoneNumber">Номер телефону</param>
public record UserProfileUpdateRequest(
    string? FirstName,
    string? LastName,
    string? PhoneNumber
);