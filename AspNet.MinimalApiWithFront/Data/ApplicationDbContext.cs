using AspNet.MinimalApiWithFront.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNet.MinimalApiWithFront.Data;

/// <summary>
/// Контекст бази даних для роботи з товарами
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Ініціалізує новий екземпляр ApplicationDbContext
    /// </summary>
    /// <param name="options">Опції конфігурації контексту</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Набір товарів у базі даних
    /// </summary>
    public DbSet<Product> Products { get; set; }

    /// <summary>
    /// Конфігурує модель даних під час створення
    /// </summary>
    /// <param name="modelBuilder">Будівельник моделі</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Конфігурація для Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Description)
                .HasMaxLength(500);
            
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18,2)");
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("datetime('now')");
            
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("datetime('now')");
        });

        // Додавання початкових даних
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "Ноутбук ASUS",
                Description = "Потужний ноутбук для роботи та ігор",
                Price = 25000.00m,
                Stock = 10,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 2,
                Name = "Смартфон Samsung",
                Description = "Сучасний смартфон з великим екраном",
                Price = 15000.00m,
                Stock = 25,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 3,
                Name = "Навушники Sony",
                Description = "Бездротові навушники з шумозаглушенням",
                Price = 3500.00m,
                Stock = 50,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
    }
}
