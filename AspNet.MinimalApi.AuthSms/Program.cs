using System.Text;
using AspNet.MinimalApi.AuthSms.Configuration;
using AspNet.MinimalApi.AuthSms.Data;
using AspNet.MinimalApi.AuthSms.Endpoints;
using AspNet.MinimalApi.AuthSms.Models;
using AspNet.MinimalApi.AuthSms.Services;
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
        Title = "SMS Authentication API",
        Version = "v1",
        Description = "ASP.NET Core Minimal API with SMS Authentication using Identity"
    });

    // Додаємо підтримку JWT в Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Налаштування бази даних (використовуємо InMemory для демо)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("AuthSmsDb"));

// Налаштування Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Налаштування паролів
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Налаштування користувача
    options.User.RequireUniqueEmail = false;
    options.User.AllowedUserNameCharacters = "0123456789+";

    // Налаштування входу
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = true;
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
        ValidIssuer = jwtSettings["Issuer"] ?? "AspNet.MinimalApi.AuthSms",
        ValidAudience = jwtSettings["Audience"] ?? "AspNet.MinimalApi.AuthSms.Client",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Реєстрація конфігурацій
builder.Services.Configure<TwilioSettings>(
    builder.Configuration.GetSection(TwilioSettings.SectionName));
builder.Services.Configure<SmsSettings>(
    builder.Configuration.GetSection(SmsSettings.SectionName));

// Реєстрація SMS сервісів
builder.Services.AddScoped<SmsService>(); // Demo SMS service (фейк)
builder.Services.AddScoped<TwilioSmsService>(); // Реальний Twilio SMS service

// Вибір SMS провайдера на основі конфігурації
var smsProvider = builder.Configuration.GetValue<string>("SmsSettings:Provider", "Demo");
if (smsProvider.Equals("Twilio", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<ISmsService, TwilioSmsService>();
}
else
{
    builder.Services.AddScoped<ISmsService, SmsService>(); // Demo service
}

builder.Services.AddScoped<IAuthService, AuthService>();

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

app.UseAuthentication();
app.UseAuthorization();

// Мапимо endpoints
app.MapAuthEndpoints();

// Тестовий endpoint
app.MapGet("/", () => new {
    Message = "SMS Authentication API is running!",
    Timestamp = DateTime.UtcNow,
    Endpoints = new[] {
        "POST /api/auth/send-registration-code",
        "POST /api/auth/send-login-code",
        "POST /api/auth/register",
        "POST /api/auth/login",
        "POST /api/auth/refresh",
        "POST /api/auth/logout",
        "GET /api/auth/me"
    }
})
.WithName("GetRoot")
.WithSummary("API Information");

app.Run();