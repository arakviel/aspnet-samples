using System.ComponentModel.DataAnnotations;

namespace AspNet.MinimalApi.AuthSms.Models;

public class SmsVerificationCode
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime ExpiresAt { get; set; }
    
    public bool IsUsed { get; set; }
    
    public string? UserId { get; set; }
    
    public SmsVerificationPurpose Purpose { get; set; }
}

public enum SmsVerificationPurpose
{
    Registration,
    Login,
    PasswordReset,
    PhoneVerification
}
