using AspNet.MinimalApi.AuthSms.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AspNet.MinimalApi.AuthSms.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<SmsVerificationCode> SmsVerificationCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure SmsVerificationCode
        builder.Entity<SmsVerificationCode>(entity =>
        {
            entity.HasIndex(e => e.PhoneNumber);
            entity.HasIndex(e => new { e.PhoneNumber, e.Code });
            entity.HasIndex(e => e.ExpiresAt);
            
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsRequired();
                
            entity.Property(e => e.Code)
                .HasMaxLength(6)
                .IsRequired();
        });

        // Configure ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName)
                .HasMaxLength(50);
                
            entity.Property(e => e.LastName)
                .HasMaxLength(50);
        });
    }
}
