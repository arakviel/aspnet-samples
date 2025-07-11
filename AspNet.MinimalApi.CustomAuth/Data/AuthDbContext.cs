using AspNet.MinimalApi.CustomAuth.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNet.MinimalApi.CustomAuth.Data;

/// <summary>
/// Контекст бази даних для системи аутентифікації.
/// Відповідає за налаштування та управління з'єднанням з базою даних SQLite.
/// </summary>
public class AuthDbContext : DbContext
{
    /// <summary>
    /// Набір користувачів у базі даних.
    /// Представляє таблицю Users та надає методи для роботи з користувачами.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Конструктор контексту бази даних.
    /// Приймає опції конфігурації для налаштування з'єднання з базою даних.
    /// </summary>
    /// <param name="options">Опції конфігурації DbContext</param>
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Налаштування моделі бази даних.
    /// Викликається Entity Framework для конфігурації таблиць та їх відносин.
    /// </summary>
    /// <param name="modelBuilder">Конструктор моделі для налаштування схеми бази даних</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Налаштування таблиці Users
        modelBuilder.Entity<User>(entity =>
        {
            // Встановлення первинного ключа
            entity.HasKey(u => u.Id);

            // Налаштування властивості Email
            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            // Створення унікального індексу для Email
            entity.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            // Налаштування властивості Password
            entity.Property(u => u.Password)
                .IsRequired()
                .HasMaxLength(255);

            // Налаштування властивості CreatedAt
            entity.Property(u => u.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("datetime('now')");
        });

        // Додавання початкових даних (seed data)
        SeedData(modelBuilder);
    }

    /// <summary>
    /// Додавання початкових даних до бази даних.
    /// Створює тестових користувачів для демонстрації роботи системи.
    /// </summary>
    /// <param name="modelBuilder">Конструктор моделі для додавання seed даних</param>
    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Email = "tom@itstep.org",
                Password = "12345",
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 2,
                Email = "bob@itstep.org",
                Password = "55555",
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
