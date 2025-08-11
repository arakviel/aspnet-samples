using AspNet.MinimalApi.BlogWithFront.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AspNet.MinimalApi.BlogWithFront.Authorization;

public class MinimumRegistrationDaysHandler(UserManager<ApplicationUser> userManager) : AuthorizationHandler<MinimumRegistrationDaysRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, MinimumRegistrationDaysRequirement requirement)
    {
        var userId = context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        if (string.IsNullOrEmpty(userId)) return;
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return;
        if ((DateTime.UtcNow - user.RegistrationDate).TotalDays >= requirement.Days)
        {
            context.Succeed(requirement);
        }
    }
}

