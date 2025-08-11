using Microsoft.AspNetCore.Authorization;

namespace AspNet.MinimalApi.BlogWithFront.Authorization;

public class MinimumRegistrationDaysRequirement(int days) : IAuthorizationRequirement
{
    public int Days { get; } = days;
}

