using Microsoft.EntityFrameworkCore;
using AspNet.MinimalApi.CustomJwtAuth.Models;

namespace AspNet.MinimalApi.CustomJwtAuth.Data;

/// <summary>
/// Контекст бази даних для системи аутентифікації.
/// Використовує Entity Framework Core для роботи з SQLite базою даних.
/// </summary>
public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Таблиця користувачів.
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// Конфігурація моделей бази даних.
    /// </summary>
    /// <param name="modelBuilder">Будівельник моделей</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Конфігурація таблиці Users
        modelBuilder.Entity<User>(entity =>
        {
            // Первинний ключ
            entity.HasKey(u => u.Id);

            // Автоінкремент для Id
            entity.Property(u => u.Id)
                .ValueGeneratedOnAdd();

            // Username - обов'язковий та унікальний
            entity.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(u => u.Username)
                .IsUnique();

            // Email - обов'язковий та унікальний
            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(u => u.Email)
                .IsUnique();

            // PasswordHash - обов'язковий
            entity.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            // Role - обов'язковий з значенням за замовчуванням
            entity.Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("User");

            // CreatedAt - обов'язковий з значенням за замовчуванням
            entity.Property(u => u.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("datetime('now')");

            // LastLoginAt - необов'язковий
            entity.Property(u => u.LastLoginAt)
                .IsRequired(false);

            // IsActive - обов'язковий з значенням за замовчуванням
            entity.Property(u => u.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
        });

        // Додаємо початкові дані (seed data)
        SeedData(modelBuilder);
    }

    /// <summary>
    /// Додає початкові дані до бази даних.
    /// </summary>
    /// <param name="modelBuilder">Будівельник моделей</param>
    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Хешуємо паролі за допомогою BCrypt
        var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
        var userPasswordHash = BCrypt.Net.BCrypt.HashPassword("user123");

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = adminPasswordHash,
                Role = "Admin",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new User
            {
                Id = 2,
                Username = "user",
                Email = "user@example.com",
                PasswordHash = userPasswordHash,
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new User
            {
                Id = 3,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123"),
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        );
    }

    /// <summary>
    /// Забезпечує створення бази даних та застосування міграцій.
    /// </summary>
    public async Task EnsureDatabaseCreatedAsync()
    {
        try
        {
            await Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            // Логуємо помилку, але не кидаємо виняток, щоб не зупинити додаток
            Console.WriteLine($"Error creating database: {ex.Message}");
        }
    }
}
