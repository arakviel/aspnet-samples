using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Auth2Factors.Data;

/// <summary>
/// ApplicationDbContext наслідує IdentityDbContext для автоматичного створення таблиць Identity
/// Це дає нам готові таблиці для користувачів, ролей, токенів тощо
/// </summary>
public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Тут можна додати додаткові налаштування моделі
        // Наприклад, змінити назви таблиць або додати індекси
        
        // Identity автоматично створює таблиці:
        // - AspNetUsers (користувачі)
        // - AspNetRoles (ролі) 
        // - AspNetUserRoles (зв'язок користувач-роль)
        // - AspNetUserClaims (claims користувачів)
        // - AspNetUserLogins (зовнішні логіни)
        // - AspNetUserTokens (токени користувачів - тут зберігаються TOTP ключі!)
        // - AspNetRoleClaims (claims ролей)
    }
}
