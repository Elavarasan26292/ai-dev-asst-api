using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ai_dev_asst_api.Models;

namespace ai_dev_asst_api.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class PermissionAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _permissionId;

    public PermissionAuthorizeAttribute(string permissionId)
    {
        if (!Guid.TryParse(permissionId, out _))
            throw new ArgumentException("Permission ID must be a valid GUID.", nameof(permissionId));

        _permissionId = permissionId;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Authentication required." });
            return;
        }

        var permissions = user.Claims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value)
            .ToList();

        if (!permissions.Contains(_permissionId, StringComparer.OrdinalIgnoreCase))
        {
            context.Result = new ObjectResult(new { message = "You do not have permission to perform this action." })
            {
                StatusCode = 403
            };
        }
    }
}