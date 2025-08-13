using Auth2Factors.Data;
using Auth2Factors.Endpoints;
using Auth2Factors.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ===== НАЛАШТУВАННЯ БАЗИ ДАНИХ =====
// Використовуємо InMemory базу для демо
// В production використовуйте SQL Server, PostgreSQL тощо
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=auth.db"));

// ===== НАЛАШТУВАННЯ IDENTITY =====
// Identity автоматично налаштовує всі необхідні сервіси для 2FA
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Налаштування паролів
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = true;

    // Налаштування користувача
    options.User.RequireUniqueEmail = true;

    // Налаштування блокування акаунту
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Налаштування підтвердження email
    options.SignIn.RequireConfirmedEmail = false; // Для демо вимкнено

    // ===== НАЛАШТУВАННЯ 2FA =====
    // Identity автоматично налаштовує TOTP провайдери
    options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders(); // Додає TOTP, Email, Phone провайдери

// ===== НАЛАШТУВАННЯ COOKIE AUTHENTICATION =====
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/api/auth/login";
    options.LogoutPath = "/api/auth/logout";
    options.AccessDeniedPath = "/api/auth/access-denied";
    options.SlidingExpiration = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// ===== РЕЄСТРАЦІЯ СЕРВІСІВ =====
// Email сервіс (демо версія)
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Авторизація
builder.Services.AddAuthorization();

// ===== SWAGGER НАЛАШТУВАННЯ =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "2FA Authentication API",
        Version = "v1",
        Description = "Демонстрація 2FA через Email та TOTP з нативним ASP.NET Core Identity"
    });

    // Додаємо підтримку авторизації в Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Cookie-based authentication",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });
});

var app = builder.Build();

// ===== НАЛАШТУВАННЯ PIPELINE =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "2FA Auth API v1");
        c.RoutePrefix = string.Empty; // Swagger на root URL
    });
}

// Middleware порядок важливий!
app.UseAuthentication(); // Спочатку аутентифікація
app.UseAuthorization();  // Потім авторизація

// ===== РЕЄСТРАЦІЯ ENDPOINTS =====
app.MapAuthEndpoints();

// Головна сторінка з інформацією
app.MapGet("/info", () => new
{
    Title = "2FA Authentication API with Native Identity",
    Description = "Демонстрація Email та TOTP двофакторної аутентифікації",
    Features = new[] {
        "Native ASP.NET Core Identity",
        "Email 2FA codes",
        "TOTP Authenticator (Google Authenticator, Authy, etc.)",
        "Recovery codes",
        "Account lockout protection",
        "QR code generation for TOTP setup"
    },
    Endpoints = new[] {
        "POST /api/auth/register - Register user",
        "POST /api/auth/login - Login (may require 2FA)",
        "POST /api/auth/verify-2fa - Verify 2FA code",
        "POST /api/auth/setup-totp - Setup TOTP authenticator",
        "POST /api/auth/verify-totp - Verify and enable TOTP",
        "POST /api/auth/disable-2fa - Disable 2FA",
        "POST /api/auth/generate-recovery-codes - Generate recovery codes",
        "POST /api/auth/use-recovery-code - Login with recovery code",
        "GET /api/auth/me - Get user info"
    },
    SwaggerUI = "/",
    DemoFlow = new[] {
        "1. Register user",
        "2. Login (will work without 2FA initially)",
        "3. Setup TOTP authenticator",
        "4. Verify TOTP to enable 2FA",
        "5. Logout and login again (now requires 2FA)",
        "6. Use authenticator app or recovery code"
    }
})
.WithName("GetInfo")
.WithSummary("API Information");

// Створюємо базу даних при старті
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.Run();