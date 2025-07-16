using AspNet.MinimalApi.IdentityAuth.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AspNet.MinimalApi.IdentityAuth.Data;

/// <summary>
/// Контекст бази даних для ASP.NET Core Identity
/// 
/// IdentityDbContext<User> автоматично створює таблиці:
/// - AspNetUsers - користувачі
/// - AspNetRoles - ролі
/// - AspNetUserRoles - зв'язок користувачів і ролей
/// - AspNetUserClaims - claims користувачів
/// - AspNetRoleClaims - claims ролей
/// - AspNetUserLogins - зовнішні логіни (Google, Facebook тощо)
/// - AspNetUserTokens - токени користувачів
/// </summary>
public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Налаштування моделей бази даних
    /// </summary>
    /// <param name="builder">Конструктор моделей</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Викликаємо базову конфігурацію Identity
        base.OnModelCreating(builder);

        // Налаштування додаткових полів користувача
        builder.Entity<User>(entity =>
        {
            // Налаштування довжини полів
            entity.Property(u => u.FirstName)
                .HasMaxLength(50);
                
            entity.Property(u => u.LastName)
                .HasMaxLength(50);
            
            // Налаштування значення за замовчуванням для CreatedAt
            entity.Property(u => u.CreatedAt)
                .HasDefaultValueSql("datetime('now')"); // SQLite синтаксис
        });

        // Можна додати початкові дані (seed data)
        SeedData(builder);
    }

    /// <summary>
    /// Додавання початкових даних до бази
    /// </summary>
    /// <param name="builder">Конструктор моделей</param>
    private static void SeedData(ModelBuilder builder)
    {
        // Тут можна додати початкових користувачів, ролі тощо
        // Наприклад:
        // builder.Entity<IdentityRole>().HasData(
        //     new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
        //     new IdentityRole { Id = "2", Name = "User", NormalizedName = "USER" }
        // );
    }
}
