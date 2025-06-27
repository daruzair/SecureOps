using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using SecureOps.Services;

namespace SecureOps.Filters;
public class HasPermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _permission;

    public HasPermissionAttribute(string permission)
    {
        _permission = permission;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        var userId = user?.Identity?.Name;

        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var permissionService = context.HttpContext.RequestServices
            .GetRequiredService<IPermissionService>();

        bool hasPermission = await permissionService.HasPermissionAsync(userId, _permission);

        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}