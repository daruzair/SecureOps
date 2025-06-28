using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using SecureOps.Services;
using SecureOps.Services.Cache.Options;
using System.Security.Claims;

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
        ClaimsPrincipal user = context.HttpContext.User;

        SecureOpsOptions secureOpsOptions = context.HttpContext.RequestServices
            .GetRequiredService<SecureOpsOptions>();

        var userId = user.Claims.FirstOrDefault(c => c.Type == secureOpsOptions.UserIdClaimType)?.Value;

        if (!(user?.Identity?.IsAuthenticated ?? false))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

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