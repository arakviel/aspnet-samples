using System.Reflection;
using System.Text;
using AspNet.MinimalApi.ExtendedJwtAuth.Data;
using AspNet.MinimalApi.ExtendedJwtAuth.Models;
using AspNet.MinimalApi.ExtendedJwtAuth.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace AspNet.MinimalApi.ExtendedJwtAuth.Extensions;

/// <summary>
///     Розширення для налаштування сервісів
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    ///     Додає налаштування бази даних
    /// </summary>
    /// <param name="services">Колекція сервісів</param>
    /// <param name="configuration">Конфігурація</param>
    /// <returns>Колекція сервісів</returns>
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                   ?? "Data Source=detailed_jwt_auth.db";
            options.UseSqlite(connectionString);
        });

        return services;
    }

    /// <summary>
    ///     Додає налаштування Identity
    /// </summary>
    /// <param name="services">Колекція сервісів</param>
    /// <returns>Колекція сервісів</returns>
    public static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services.AddIdentity<User, IdentityRole>(options =>
            {
                // Налаштування паролів
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Налаштування користувача
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                // Налаштування блокування
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // Налаштування підтвердження
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    /// <summary>
    ///     Додає налаштування JWT аутентифікації
    /// </summary>
    /// <param name="services">Колекція сервісів</param>
    /// <param name="configuration">Конфігурація</param>
    /// <returns>Колекція сервісів</returns>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ??
                        throw new InvalidOperationException("JWT SecretKey не налаштований");
        var issuer = jwtSettings["Issuer"] ?? "AspNet.MinimalApi.ExtendedJwtAuth";
        var audience = jwtSettings["Audience"] ?? "AspNet.MinimalApi.ExtendedJwtAuth.Client";

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero // Не додаємо додатковий час до терміну дії токена
                };

                // Налаштування для отримання токена з різних джерел
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Спробуємо отримати токен з query параметра (для SignalR тощо)
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub"))
                            context.Token = accessToken;

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            context.Response.Headers.Append("Token-Expired", "true");
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    /// <summary>
    ///     Додає кастомні сервіси
    /// </summary>
    /// <param name="services">Колекція сервісів</param>
    /// <returns>Колекція сервісів</returns>
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services.AddScoped<JwtService>();
        services.AddScoped<AuthService>();

        return services;
    }

    /// <summary>
    ///     Додає налаштування Swagger з підтримкою JWT
    /// </summary>
    /// <param name="services">Колекція сервісів</param>
    /// <returns>Колекція сервісів</returns>
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ASP.NET Core Detailed JWT Auth API",
                Version = "v1",
                Description = "Детальна реалізація JWT аутентифікації з використанням UserManager та Identity",
                Contact = new OpenApiContact
                {
                    Name = "Розробник",
                    Email = "developer@example.com"
                }
            });

            // Додаємо підтримку JWT в Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description =
                    "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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

            // Додаємо XML коментарі, якщо файл існує
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
        });

        return services;
    }

    /// <summary>
    ///     Застосовує міграції бази даних
    /// </summary>
    /// <param name="app">Додаток</param>
    /// <returns>Додаток</returns>
    public static async Task<WebApplication> ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            await context.Database.MigrateAsync();
            app.Logger.LogInformation("Міграції бази даних успішно застосовані");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Помилка при застосуванні міграцій");
            throw;
        }

        return app;
    }

    /// <summary>
    ///     Очищає застарілі refresh токени
    /// </summary>
    /// <param name="app">Додаток</param>
    /// <returns>Додаток</returns>
    public static WebApplication AddTokenCleanupService(this WebApplication app)
    {
        // Запускаємо фонову задачу для очищення застарілих токенів
        _ = Task.Run(async () =>
        {
            while (!app.Lifetime.ApplicationStopping.IsCancellationRequested)
            {
                try
                {
                    using var scope = app.Services.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var expiredTokens = await context.RefreshTokens
                        .Where(rt => rt.ExpiresAt < DateTime.UtcNow || rt.IsRevoked)
                        .ToListAsync();

                    if (expiredTokens.Any())
                    {
                        context.RefreshTokens.RemoveRange(expiredTokens);
                        await context.SaveChangesAsync();
                        app.Logger.LogInformation("Видалено {Count} застарілих токенів", expiredTokens.Count);
                    }
                }
                catch (Exception ex)
                {
                    app.Logger.LogError(ex, "Помилка при очищенні застарілих токенів");
                }

                // Очищаємо кожну годину
                await Task.Delay(TimeSpan.FromHours(1), app.Lifetime.ApplicationStopping);
            }
        });

        return app;
    }

    /// <summary>
    ///     Створює базу даних, якщо вона не існує (альтернатива міграціям для розробки)
    /// </summary>
    /// <param name="app">Додаток</param>
    public static void EnsureDatabaseCreated(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Створюємо базу даних, якщо вона не існує
        context.Database.EnsureCreated();
    }
}