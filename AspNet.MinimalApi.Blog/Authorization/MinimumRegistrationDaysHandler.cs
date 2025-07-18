using System.Security.Claims;
using AspNet.MinimalApi.Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AspNet.MinimalApi.Blog.Authorization;

public class MinimumRegistrationDaysHandler : AuthorizationHandler<MinimumRegistrationDaysRequirement>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public MinimumRegistrationDaysHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        MinimumRegistrationDaysRequirement requirement)
    {
        if (context.User.Identity == null || !context.User.Identity.IsAuthenticated)
        {
            context.Fail();
            return;
        }

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            context.Fail();
            return;
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            context.Fail();
            return;
        }

        if ((DateTime.UtcNow - user.RegistrationDate).TotalDays >= requirement.MinimumDays)
            context.Succeed(requirement);
        else
            context.Fail();
    }
}