using Microsoft.AspNetCore.Identity;

namespace AspNet.MinimalApi.Blog.Models;

public class ApplicationUser : IdentityUser
{
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
}