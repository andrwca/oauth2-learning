using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WebApiAuth.Models;

namespace WebApiAuth.Authorization
{
    internal class ProjectOwnerHandler : AuthorizationHandler<ProjectOwnerRequirement, Project>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ProjectOwnerRequirement requirement, Project resource)
        {
            string? oid = context.User.FindFirstValue("oid");
            if (string.IsNullOrEmpty(oid))
            {
                // User can't be verified as an an owner.
                return Task.CompletedTask;
            }

            if (!resource.Owners.Contains(oid))
            {
                // User is not an owner.
                return Task.CompletedTask;
            }

            // User is an owner.
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}