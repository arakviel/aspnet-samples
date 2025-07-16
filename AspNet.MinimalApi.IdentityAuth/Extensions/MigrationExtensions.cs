using AspNet.MinimalApi.IdentityAuth.Data;
using Microsoft.EntityFrameworkCore;

namespace AspNet.MinimalApi.IdentityAuth.Extensions;

/// <summary>
/// Розширення для автоматичного застосування міграцій бази даних
/// </summary>
public static class MigrationExtensions
{
    /// <summary>
    /// Застосовує всі незастосовані міграції до бази даних
    /// </summary>
    /// <param name="app">Додаток</param>
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        using ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Застосовуємо міграції
        context.Database.Migrate();
    }
    
    /// <summary>
    /// Створює базу даних, якщо вона не існує (альтернатива міграціям для розробки)
    /// </summary>
    /// <param name="app">Додаток</param>
    public static void EnsureDatabaseCreated(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        using ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Створюємо базу даних, якщо вона не існує
        context.Database.EnsureCreated();
    }
}
