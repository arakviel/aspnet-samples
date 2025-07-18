using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AspNet.MinimalApi.Blog.Authorization;
using AspNet.MinimalApi.Blog.Data;
using AspNet.MinimalApi.Blog.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Configure DbContext with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        options.User.RequireUniqueEmail = true;

        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

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
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireModeratorRole", policy => policy.RequireRole("Moderator"));
    options.AddPolicy("RequireEmailConfirmation", policy => policy.RequireClaim("EmailVerified", "true"));
    options.AddPolicy("RequireAuthenticatedUser", policy => policy.RequireAuthenticatedUser());
    options.AddPolicy("MinimumRegistrationDays",
        policy => policy.Requirements.Add(new MinimumRegistrationDaysRequirement(3)));
});

builder.Services.AddScoped<IAuthorizationHandler, MinimumRegistrationDaysHandler>();

var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();

    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Seed roles
    string[] roleNames = { "Admin", "User" };
    foreach (var roleName in roleNames)
        if (!roleManager.RoleExistsAsync(roleName).Result)
            roleManager.CreateAsync(new IdentityRole(roleName)).Wait();

    // Seed admin user
    var adminUser = new ApplicationUser
    {
        UserName = "admin@example.com", Email = "admin@example.com", EmailConfirmed = true,
        RegistrationDate = DateTime.UtcNow.AddDays(-10)
    };
    if (userManager.FindByNameAsync("admin@example.com").Result == null)
    {
        userManager.CreateAsync(adminUser, "Admin@123").Wait();
        userManager.AddToRoleAsync(adminUser, "Admin").Wait();
    }

    // Seed regular user
    var regularUser = new ApplicationUser
    {
        UserName = "user@example.com", Email = "user@example.com", EmailConfirmed = true,
        RegistrationDate = DateTime.UtcNow.AddDays(-5)
    };
    if (userManager.FindByNameAsync("user@example.com").Result == null)
    {
        userManager.CreateAsync(regularUser, "User@123").Wait();
        userManager.AddToRoleAsync(regularUser, "User").Wait();
    }

    // Seed unconfirmed user
    var unconfirmedUser = new ApplicationUser
    {
        UserName = "unconfirmed@example.com", Email = "unconfirmed@example.com", EmailConfirmed = false,
        RegistrationDate = DateTime.UtcNow.AddDays(-7)
    };
    if (userManager.FindByNameAsync("unconfirmed@example.com").Result == null)
    {
        userManager.CreateAsync(unconfirmedUser, "Unconfirmed@123").Wait();
        userManager.AddToRoleAsync(unconfirmedUser, "User").Wait();
    }

    // Seed new user (for registration days policy testing)
    var newUser = new ApplicationUser
    {
        UserName = "newuser@example.com", Email = "newuser@example.com", EmailConfirmed = true,
        RegistrationDate = DateTime.UtcNow.AddHours(-1)
    };
    if (userManager.FindByNameAsync("newuser@example.com").Result == null)
    {
        userManager.CreateAsync(newUser, "NewUser@123").Wait();
        userManager.AddToRoleAsync(newUser, "User").Wait();
    }

    // Seed posts
    if (!context.Posts.Any())
    {
        var post1 = new Post
        {
            Title = "First Blog Post",
            Content = "This is the content of the first blog post.",
            AuthorId = adminUser.Id,
            Author = adminUser,
            CreatedDate = DateTime.UtcNow.AddDays(-9),
            Comments = new List<Comment>()
        };
        var post2 = new Post
        {
            Title = "Second Blog Post",
            Content = "This is the content of the second blog post.",
            AuthorId = regularUser.Id,
            Author = regularUser,
            CreatedDate = DateTime.UtcNow.AddDays(-4),
            Comments = new List<Comment>()
        };
        context.Posts.AddRange(post1, post2);
        context.SaveChanges();

        // Seed comments
        var comment1 = new Comment
        {
            Content = "Great first post!",
            PostId = post1.Id,
            Post = post1,
            AuthorId = regularUser.Id,
            Author = regularUser,
            CreatedDate = DateTime.UtcNow.AddDays(-3)
        };
        var comment2 = new Comment
        {
            Content = "Interesting read.",
            PostId = post1.Id,
            Post = post1,
            AuthorId = adminUser.Id,
            Author = adminUser,
            CreatedDate = DateTime.UtcNow.AddDays(-2)
        };
        var comment3 = new Comment
        {
            Content = "Looking forward to more!",
            PostId = post2.Id,
            Post = post2,
            AuthorId = regularUser.Id,
            Author = regularUser,
            CreatedDate = DateTime.UtcNow.AddDays(-1)
        };
        context.Comments.AddRange(comment1, comment2, comment3);
        context.SaveChanges();
    }
}

app.UseAuthentication();
app.UseAuthorization();

// Public endpoint
app.MapGet("/", () => "Welcome to the Blog API!");

//app.MapIdentityApi<ApplicationUser>();

// Authentication endpoints
app.MapPost("/register", async (UserManager<ApplicationUser> userManager, RegisterRequest request) =>
{
    var user = new ApplicationUser { UserName = request.UserName, Email = request.Email, EmailConfirmed = false };
    var result = await userManager.CreateAsync(user, request.Password);
    if (result.Succeeded)
    {
        await userManager.AddToRoleAsync(user, "User"); // Assign default role
        // In a real app, send confirmation email here
        return Results.Ok(new { Message = "User registered successfully. Please confirm your email." });
    }

    return Results.BadRequest(result.Errors);
});

app.MapPost("/login",
    async (UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
        IConfiguration config, LoginRequest request) =>
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null) return Results.Unauthorized();

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded) return Results.Unauthorized();

        var roles = await userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new("EmailVerified", user.EmailConfirmed.ToString().ToLower())
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var jwtSettings = config.GetSection("JwtSettings");
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));
        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            jwtSettings["Issuer"],
            jwtSettings["Audience"],
            claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials);

        return Results.Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
    });

app.MapGet("/confirm-email", async (UserManager<ApplicationUser> userManager, string userId, string token) =>
{
    var user = await userManager.FindByIdAsync(userId);
    if (user == null) return Results.BadRequest("User not found.");

    var result = await userManager.ConfirmEmailAsync(user, token);
    if (result.Succeeded) return Results.Ok("Email confirmed successfully.");
    return Results.BadRequest(result.Errors);
});

// Protected endpoints
app.MapGet("/admin",
        (ClaimsPrincipal user) => $"Hello Admin! Your ID: {user.FindFirstValue(ClaimTypes.NameIdentifier)}")
    .RequireAuthorization("RequireAdminRole");

app.MapGet("/authenticated",
        (ClaimsPrincipal user) =>
            $"Hello Authenticated User! Your ID: {user.FindFirstValue(ClaimTypes.NameIdentifier)}")
    .RequireAuthorization("RequireAuthenticatedUser");

app.MapGet("/email-confirmed",
        (ClaimsPrincipal user) =>
            $"Hello Email Confirmed User! Your ID: {user.FindFirstValue(ClaimTypes.NameIdentifier)}")
    .RequireAuthorization("RequireEmailConfirmation");

app.MapGet("/posts", () => Results.Ok(new List<Post>()))
    .RequireAuthorization("RequireAuthenticatedUser");

app.MapPost("/posts",
        async (Post post, ClaimsPrincipal user, AppDbContext dbContext, UserManager<ApplicationUser> userManager) =>
        {
            var applicationUser = await userManager.FindByIdAsync(user.FindFirstValue(ClaimTypes.NameIdentifier));
            if (applicationUser == null) return Results.Unauthorized();

            post.AuthorId = applicationUser.Id;
            post.Author = applicationUser;
            post.CreatedDate = DateTime.UtcNow;
            post.Comments = new List<Comment>(); // Initialize comments as empty

            dbContext.Posts.Add(post);
            await dbContext.SaveChangesAsync();
            return Results.Created($"/posts/{post.Id}", post);
        })
    .RequireAuthorization("RequireAdminRole");

app.MapPost("/posts/{postId}/comments", async (int postId, Comment comment, ClaimsPrincipal user,
        AppDbContext dbContext, UserManager<ApplicationUser> userManager) =>
    {
        var applicationUser = await userManager.FindByIdAsync(user.FindFirstValue(ClaimTypes.NameIdentifier));
        if (applicationUser == null) return Results.Unauthorized();

        comment.PostId = postId;
        comment.AuthorId = applicationUser.Id;
        comment.CreatedDate = DateTime.UtcNow;
        dbContext.Comments.Add(comment);
        await dbContext.SaveChangesAsync();
        return Results.Created($"/posts/{postId}/comments/{comment.Id}", comment);
    })
    .RequireAuthorization("MinimumRegistrationDays");

app.Run();