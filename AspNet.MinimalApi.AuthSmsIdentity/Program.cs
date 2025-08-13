using System.Text;
using AspNet.MinimalApi.AuthSmsIdentity.Data;
using AspNet.MinimalApi.AuthSmsIdentity.Endpoints;
using AspNet.MinimalApi.AuthSmsIdentity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Додаємо сервіси
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SMS Authentication API with Native Identity",
        Version = "v1",
        Description = "ASP.NET Core Minimal API using native Identity features for SMS authentication"
    });

    // Додаємо підтримку cookie authentication в Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Cookie-based authentication",
        Name = "Cookie",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Cookie"
    });
});

// Налаштування бази даних (використовуємо InMemory для демо)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    //options.UseInMemoryDatabase("AuthSmsIdentityDb"));
    options.UseSqlite("Data Source=auth.db"));

// Налаштування нативного Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Налаштування паролів
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Налаштування користувача
    options.User.RequireUniqueEmail = false;
    options.User.AllowedUserNameCharacters = "0123456789+()-";

    // Налаштування входу
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false; // Дозволяємо вхід без підтвердження телефону

    // Налаштування блокування
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Налаштування токенів
    options.Tokens.ChangePhoneNumberTokenProvider = TokenOptions.DefaultPhoneProvider;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Налаштування JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "MyVeryLongSecretKeyForJWTTokenGeneration123456789";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "AspNet.MinimalApi.AuthSmsIdentity",
        ValidAudience = jwtSettings["Audience"] ?? "AspNet.MinimalApi.AuthSmsIdentity.Client",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };

    // Читаємо JWT з cookies
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Спочатку перевіряємо Authorization header
            if (string.IsNullOrEmpty(context.Token))
            {
                // Якщо немає в header, читаємо з cookie
                context.Token = context.Request.Cookies["jwt"];
            }
            return Task.CompletedTask;
        }
    };
});

// Налаштування cookie authentication (для сумісності з Identity)
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

// Реєстрація SMS сервісів
builder.Services.AddTransient<SmsSender>(); // Demo SMS service
builder.Services.AddTransient<TwilioSmsSender>(); // Twilio SMS service

// Вибір SMS провайдера на основі конфігурації
var smsProvider = builder.Configuration.GetValue<string>("SmsSettings:Provider", "Demo");
if (smsProvider.Equals("Twilio", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddTransient<ISmsSender, TwilioSmsSender>();
    builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Information);
}
else
{
    builder.Services.AddTransient<ISmsSender, SmsSender>(); // Demo service
}

// Додаємо авторизацію
builder.Services.AddAuthorization();

// Реєстрація JWT сервісу
builder.Services.AddScoped<IJwtService, JwtService>();

// CORS для розробки
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Налаштування pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SMS Authentication API v1");
        c.RoutePrefix = string.Empty; // Swagger UI на root URL
    });
    app.UseCors("AllowAll");
}

// Ініціалізація бази даних
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

// Важливо: порядок middleware має значення!
app.UseAuthentication(); // Спочатку аутентифікація
app.UseAuthorization();  // Потім авторизація

// Мапимо endpoints
app.MapAuthEndpoints();

// Тестовий endpoint
app.MapGet("/", () => new {
    Message = "SMS Authentication API with Native Identity is running!",
    Timestamp = DateTime.UtcNow,
    Features = new[] {
        "Native ASP.NET Core Identity",
        "JWT in HttpOnly cookies (hybrid approach)",
        "SMS phone confirmation",
        "SMS-only passwordless login",
        "Traditional password + SMS login",
        "Two-factor authentication via SMS",
        "Recovery codes",
        "Account lockout protection",
        "SPA-friendly authentication"
    },
    Endpoints = new[] {
        "POST /api/auth/register",
        "POST /api/auth/login",
        "POST /api/auth/send-sms-login-code",
        "POST /api/auth/sms-login",
        "POST /api/auth/logout",
        "POST /api/auth/send-phone-confirmation",
        "POST /api/auth/confirm-phone",
        "POST /api/auth/enable-2fa-sms",
        "POST /api/auth/disable-2fa",
        "GET /api/auth/me",
        "POST /api/auth/generate-recovery-codes"
    }
})
.WithName("GetRoot")
.WithSummary("API Information");

app.Run();