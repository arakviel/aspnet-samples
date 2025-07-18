using Microsoft.AspNetCore.Authorization;

namespace AspNet.MinimalApi.Blog.Authorization;

public class MinimumRegistrationDaysRequirement : IAuthorizationRequirement
{
    public MinimumRegistrationDaysRequirement(int minimumDays)
    {
        MinimumDays = minimumDays;
    }

    public int MinimumDays { get; }
}