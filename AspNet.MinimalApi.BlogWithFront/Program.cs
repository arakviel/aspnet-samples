using System.Text;
using AspNet.MinimalApi.BlogWithFront.Authorization;
using AspNet.MinimalApi.BlogWithFront.Common;
using AspNet.MinimalApi.BlogWithFront.Data;
using AspNet.MinimalApi.BlogWithFront.Domain;
using AspNet.MinimalApi.BlogWithFront.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// DbContext
var connectionString = builder.Configuration["POSTGRES_CONNECTION"] ??
                       builder.Configuration.GetConnectionString("Postgres") ??
                       "Host=localhost;Port=5432;Database=blogdb;Username=postgres;Password=postgres";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Identity
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// AuthZ policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("MinimumRegistrationDays", policy =>
        policy.Requirements.Add(new MinimumRegistrationDaysRequirement(1)));
});

builder.Services.AddScoped<IAuthorizationHandler, MinimumRegistrationDaysHandler>();

// Реєструємо сервіс для роботи з рефреш токенами
builder.Services.AddScoped<RefreshTokenService>();

// Реєструємо Email сервіс
builder.Services.AddScoped<IEmailService, EmailService>();

// JWT Auth
var jwtSection = builder.Configuration.GetSection("Jwt");
var issuer = jwtSection["Issuer"] ?? "blog-api";
var audience = jwtSection["Audience"] ?? "blog-api-client";
var key = jwtSection["Key"] ?? "dev-secret-change-this-is-a-very-long-key-for-jwt-signing-purposes-minimum-256-bits";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddCors(policy =>
{
    policy.AddDefaultPolicy(p => p
        .WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:5175", "http://localhost:3000") // Vite та інші dev сервери
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Blog API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Apply migrations / Ensure DB
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var db = sp.GetRequiredService<AppDbContext>();
    // Застосовуємо міграції автоматично при старті
    db.Database.Migrate();

    var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roleNames = { "Admin", "User" };
    foreach (var role in roleNames)
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

    var adminEmail = "admin@example.com";
    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin is null)
    {
        admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true, RegistrationDate = DateTime.UtcNow.AddDays(-10) };
        await userManager.CreateAsync(admin, "Admin@123");
        await userManager.AddToRoleAsync(admin, "Admin");
    }

    // Seed posts
    if (!db.Posts.Any())
    {
        var posts = new[]
        {
            new Post
            {
                Title = "Ласкаво просимо до нашого блогу!",
                Content = "Це перший пост у нашому блозі. Тут ви знайдете цікаві статті про технології, програмування та багато іншого. Ми раді вітати вас у нашій спільноті!",
                Summary = "Вітальний пост для нових читачів блогу",
                Tags = "вітання,блог,спільнота",
                AuthorId = admin.Id,
                CreatedDate = DateTime.UtcNow.AddDays(-5),
                IsPublished = true
            },
            new Post
            {
                Title = "Введення в ASP.NET Core Minimal API",
                Content = "ASP.NET Core Minimal API - це новий підхід до створення веб-API з мінімальною кількістю коду. У цій статті ми розглянемо основні концепції та переваги використання Minimal API для створення сучасних веб-додатків. Minimal API дозволяє швидко створювати легкі та продуктивні API з мінімальними налаштуваннями.",
                Summary = "Огляд можливостей ASP.NET Core Minimal API",
                Tags = "aspnet,minimal-api,веб-розробка",
                AuthorId = admin.Id,
                CreatedDate = DateTime.UtcNow.AddDays(-3),
                IsPublished = true
            },
            new Post
            {
                Title = "Сучасний фронтенд з TypeScript",
                Content = "TypeScript революціонізував розробку фронтенду, надавши статичну типізацію JavaScript. У цій статті ми розглянемо, як TypeScript допомагає створювати більш надійні та масштабовані додатки. Ми також обговоримо кращі практики використання TypeScript у сучасних фронтенд проектах.",
                Summary = "Переваги використання TypeScript у фронтенд розробці",
                Tags = "typescript,frontend,javascript",
                AuthorId = admin.Id,
                CreatedDate = DateTime.UtcNow.AddDays(-1),
                IsPublished = true
            }
        };

        db.Posts.AddRange(posts);
        await db.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blog API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Static files for simple front-end
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api", () => Results.Ok(new { Name = "Blog API", Version = "1.0" }));

// Map slice endpoints
app.MapEndpoints();

app.Run();