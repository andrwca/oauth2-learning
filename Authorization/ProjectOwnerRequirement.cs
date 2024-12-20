using Microsoft.AspNetCore.Authorization;

namespace WebApiAuth.Authorization
{
    internal class ProjectOwnerRequirement : IAuthorizationRequirement
    {
    }
}