using System.Security.Claims;
using AspNet.MinimalApi.IdentityWithEmailConfirmationAuth.Data;
using AspNet.MinimalApi.IdentityWithEmailConfirmationAuth.Extensions;
using AspNet.MinimalApi.IdentityWithEmailConfirmationAuth.Models;
using AspNet.MinimalApi.IdentityWithEmailConfirmationAuth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

// Створюємо конструктор додатку
var builder = WebApplication.CreateBuilder(args);

// === НАЛАШТУВАННЯ СЕРВІСІВ ===

// Додаємо підтримку OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Identity with Email Confirmation API",
        Version = "v1",
        Description = "ASP.NET Core Identity з підтвердженням електронної пошти"
    });
});

// Налаштування бази даних
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Використовуємо SQLite для простоти (у продакшені краще PostgreSQL/SQL Server)
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                           ?? "Data Source=identity_email_confirmation.db";
    options.UseSqlite(connectionString);
});

// === НАЛАШТУВАННЯ ASP.NET CORE IDENTITY ===

// Додаємо авторизацію
builder.Services.AddAuthorization();

// Налаштування аутентифікації
// Можна використовувати як Bearer токени, так і Cookies
// Закоментуйте/розкоментуйте потрібний варіант

// Варіант 1: Тільки Bearer Token аутентифікація (для API)
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = IdentityConstants.BearerScheme;
        options.DefaultChallengeScheme = IdentityConstants.BearerScheme;
    })
    .AddBearerToken(IdentityConstants.BearerScheme, options =>
    {
        // Налаштування Bearer токенів для API
        options.BearerTokenExpiration = TimeSpan.FromHours(1); // Термін дії токена
        options.RefreshTokenExpiration = TimeSpan.FromDays(7); // Термін дії refresh токена
    });

// Варіант 2: Cookie + Bearer Token аутентифікація (для веб + API)
// Розкоментуйте цей блок, якщо потрібна підтримка cookies
// builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
//     .AddCookie(IdentityConstants.ApplicationScheme, options =>
//     {
//         options.LoginPath = "/login";
//         options.LogoutPath = "/logout";
//         options.ExpireTimeSpan = TimeSpan.FromDays(7);
//         options.SlidingExpiration = true;
//         options.Cookie.Name = "IdentityAuth";
//         options.Cookie.HttpOnly = true;
//         options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
//         options.Cookie.SameSite = SameSiteMode.Lax;
//     })
//     .AddBearerToken(IdentityConstants.BearerScheme, options =>
//     {
//         options.BearerTokenExpiration = TimeSpan.FromHours(1);
//         options.RefreshTokenExpiration = TimeSpan.FromDays(7);
//     });

// Налаштування Identity
builder.Services.AddIdentityCore<User>(options =>
    {
        // Налаштування паролів
        options.Password.RequireDigit = true; // Вимагати цифри
        options.Password.RequireLowercase = true; // Вимагати малі літери
        options.Password.RequireUppercase = true; // Вимагати великі літери
        options.Password.RequireNonAlphanumeric = false; // Не вимагати спеціальні символи
        options.Password.RequiredLength = 6; // Мінімальна довжина пароля

        // Налаштування користувача
        options.User.RequireUniqueEmail = true; // Email має бути унікальним

        // Налаштування блокування
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // Час блокування
        options.Lockout.MaxFailedAccessAttempts = 5; // Максимум невдалих спроб
        options.Lockout.AllowedForNewUsers = true; // Дозволити блокування нових користувачів

        // КЛЮЧОВЕ НАЛАШТУВАННЯ: Вимагати підтвердження email
        options.SignIn.RequireConfirmedEmail = true; // УВІМКНЕНО підтвердження email
        options.SignIn.RequireConfirmedPhoneNumber = false; // Не вимагати підтвердження телефону

        // Налаштування токенів
        options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
        options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>() // Використовуємо Entity Framework
    .AddApiEndpoints() // Додаємо стандартні API ендпоінти Identity
    .AddDefaultTokenProviders(); // Додаємо провайдери токенів для email підтвердження

// Додаємо email сервіс як Transient для сумісності з MapIdentityApi
builder.Services.AddScoped<IEmailService, EmailService>();

// Додаємо сервіс для обробки подій Identity
builder.Services.AddScoped<IdentityEventService>();

// Додаємо EmailSender для автоматичної відправки email через MapIdentityApi
// Реєструємо обидва інтерфейси як Transient, щоб уникнути проблем з DI
builder.Services.AddScoped<IEmailSender, IdentityEmailSender>();
builder.Services.AddScoped<IEmailSender<User>, IdentityEmailSender>();

// Створюємо додаток
var app = builder.Build();

// === НАЛАШТУВАННЯ MIDDLEWARE ===

// Увімкнути Swagger в режимі розробки
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Email Confirmation API v1");
        c.RoutePrefix = "swagger";
    });

    // Автоматично застосовуємо міграції в режимі розробки
    app.EnsureDatabaseCreated();
}

// Middleware для обробки помилок
app.UseExceptionHandler("/error");

// Middleware для аутентифікації та авторизації
app.UseAuthentication();
app.UseAuthorization();

// === ЕНДПОІНТИ ===

// Головна сторінка з інформацією про API
app.MapGet("/", () => new
    {
        Message = "ASP.NET Core Identity Authentication API with Email Confirmation",
        Description = "Приклад використання ASP.NET Core Identity в Minimal API з підтвердженням електронної пошти",
        Version = "1.0",
        Endpoints = new
        {
            // 🔥 ВБУДОВАНІ ендпоінти Identity (автоматично створені MapIdentityApi)
            // Використовують Bearer Token аутентифікацію (Authorization: Bearer <token>)
            // ✅ З АВТОМАТИЧНОЮ відправкою email підтвердження!
            Register = "POST /register - 📧 АВТОМАТИЧНО відправляє email підтвердження",
            Login = "POST /login - 🚫 Блокується до підтвердження email",
            Refresh = "POST /refresh",
            ConfirmEmail = "GET /confirmEmail - ✅ Підтверджує email за токеном",
            ResendConfirmationEmail = "POST /resendConfirmationEmail - 🔄 Повторна відправка",
            ForgotPassword = "POST /forgotPassword",
            ResetPassword = "POST /resetPassword",
            Manage_2fa = "POST /manage/2fa",
            Manage_Info = "GET /manage/info",

            // Кастомні ендпоінти
            UserProfile = "GET /auth/profile",
            AllUsers = "GET /auth/users",
            UpdateProfile = "PUT /auth/profile",
            EmailStatus = "GET /auth/email-status",

            // Тестові ендпоінти (тільки для розробки)
            DevGetToken = "GET /dev/get-confirmation-token/{email} - 🔧 DEV ONLY"
        },
        AuthenticationInfo = new
        {
            CurrentMode = "🔥 ВБУДОВАНА Identity з Email підтвердженням",
            EmailConfirmationRequired = true,
            AutomaticEmailSending = "✅ Автоматично через MapIdentityApi + IEmailSender",
            Note = "Користувачі повинні підтвердити email перед входом в систему",
            BearerTokenUsage = "Додайте заголовок: Authorization: Bearer <your_token>",
            EmailConfirmationFlow = new[]
            {
                "1. 📝 POST /register - автоматично відправляє email підтвердження",
                "2. 📧 Перевірте email та перейдіть за посиланням підтвердження",
                "3. ✅ GET /confirmEmail?userId=...&code=... - підтверджує email",
                "4. 🔓 POST /login - тепер дозволяється вхід"
            },
            BuiltInFeatures = new[]
            {
                "✅ Автоматична відправка email при реєстрації",
                "✅ Блокування входу до підтвердження email",
                "✅ Повторна відправка підтвердження",
                "✅ Красиві HTML шаблони email",
                "✅ Обробка помилок та валідація"
            }
        },
        Features = new[]
        {
            "ASP.NET Core Identity",
            "Entity Framework Core",
            "SQLite Database",
            "Bearer Token Authentication",
            "Email Confirmation Required",
            "SMTP Email Sending",
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
app.MapIdentityApi<User>();

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
            user.EmailConfirmedAt,
            user.PhoneNumber,
            user.PhoneNumberConfirmed
        });
    })
    .RequireAuthorization()
    .WithName("GetUserProfile")
    .WithSummary("Отримати профіль користувача")
    .WithDescription("Повертає профіль поточного авторизованого користувача");

// Оновлення профілю користувача
app.MapPut("/auth/profile", async (ClaimsPrincipal claims, ApplicationDbContext context,
        UserManager<User> userManager, UpdateProfileRequest request) =>
    {
        var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return Results.NotFound(new { Message = "Користувача не знайдено" });

        // Оновлюємо дані користувача
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;

        var result = await userManager.UpdateAsync(user);

        if (result.Succeeded) return Results.Ok(new { Message = "Профіль успішно оновлено" });

        return Results.BadRequest(new { Message = "Помилка при оновленні профілю", result.Errors });
    })
    .RequireAuthorization()
    .WithName("UpdateUserProfile")
    .WithSummary("Оновити профіль користувача")
    .WithDescription("Оновлює профіль поточного авторизованого користувача");

// Отримання списку всіх користувачів (тільки для демонстрації)
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
                u.EmailConfirmed,
                u.EmailConfirmedAt
            })
            .ToListAsync();

        return Results.Ok(users);
    })
    .RequireAuthorization()
    .WithName("GetAllUsers")
    .WithSummary("Отримати список користувачів")
    .WithDescription("Повертає список всіх користувачів (тільки для демонстрації)");

// Перевірка статусу підтвердження email
app.MapGet("/auth/email-status", async (ClaimsPrincipal claims, UserManager<User> userManager) =>
    {
        var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return Results.NotFound(new { Message = "Користувача не знайдено" });

        return Results.Ok(new
        {
            user.Email,
            user.EmailConfirmed,
            user.EmailConfirmedAt,
            CanResendConfirmation = !user.EmailConfirmed
        });
    })
    .RequireAuthorization()
    .WithName("GetEmailStatus")
    .WithSummary("Статус підтвердження email")
    .WithDescription("Повертає інформацію про статус підтвердження email користувача");

// Кастомний ендпоінт для повторної відправки email підтвердження
app.MapPost("/auth/resend-email-confirmation", async (UserManager<User> userManager,
        IEmailService emailService, ResendEmailRequest request) =>
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
            // Не розкриваємо, чи існує користувач з таким email
            return Results.Ok(new
                { Message = "Якщо користувач з таким email існує, лист підтвердження буде відправлено" });

        if (user.EmailConfirmed) return Results.BadRequest(new { Message = "Email вже підтверджено" });

        // Генеруємо токен підтвердження
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        // Створюємо посилання для підтвердження
        var confirmationLink =
            $"http://localhost:5121/auth/confirm-email?userId={user.Id}&code={Uri.EscapeDataString(token)}";

        // Відправляємо email
        await emailService.SendEmailConfirmationAsync(user.Email!, confirmationLink, user.FullName);

        return Results.Ok(new { Message = "Лист підтвердження відправлено" });
    })
    .WithName("ResendEmailConfirmation")
    .WithSummary("Повторна відправка email підтвердження")
    .WithDescription("Відправляє повторний лист підтвердження на вказаний email");

// ТЕСТОВИЙ ендпоінт для отримання токена підтвердження (тільки для розробки)
app.MapGet("/dev/get-confirmation-token/{email}", async (UserManager<User> userManager, string email) =>
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null) return Results.NotFound(new { Message = "Користувача не знайдено" });

        if (user.EmailConfirmed) return Results.BadRequest(new { Message = "Email вже підтверджено" });

        // Генеруємо токен підтвердження
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        // Створюємо посилання для підтвердження
        var confirmationLink =
            $"http://localhost:5121/auth/confirm-email?userId={user.Id}&code={Uri.EscapeDataString(token)}";

        return Results.Ok(new
        {
            UserId = user.Id,
            user.Email,
            Token = token,
            EncodedToken = Uri.EscapeDataString(token),
            ConfirmationLink = confirmationLink,
            Message = "⚠️ Цей ендпоінт тільки для розробки! Видаліть його в продакшені."
        });
    })
    .WithName("GetConfirmationTokenDev")
    .WithSummary("🔧 DEV: Отримати токен підтвердження")
    .WithDescription("⚠️ ТІЛЬКИ ДЛЯ РОЗРОБКИ! Повертає токен підтвердження для тестування");

// Обробка помилок
app.MapGet("/error", () => Results.Problem("Виникла помилка при обробці запиту"))
    .ExcludeFromDescription();

// Запуск додатку
app.Run();

// === МОДЕЛІ ЗАПИТІВ ===

// Модель для оновлення профілю
public record UpdateProfileRequest(string? FirstName, string? LastName, string? PhoneNumber);

// Модель для повторної відправки email підтвердження
public record ResendEmailRequest(string Email);