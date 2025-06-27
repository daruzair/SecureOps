using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SecureOps.Endpoints.Model;
using SecureOps.Endpoints.Options;
using SecureOps.Services;

namespace SecureOps.Endpoints;
public static class PermissionApiEndpointExtensions
{
    public static IEndpointRouteBuilder MapPermissionEndpoints(
        this IEndpointRouteBuilder app,
        Action<PermissionApiOptions>? configure = null)
    {
        var options = new PermissionApiOptions();
        configure?.Invoke(options);

        var group = app.MapGroup(options.RoutePrefix).WithTags("Permissions");

        if (!string.IsNullOrWhiteSpace(options.PermissionClaim))
        {
            group.RequireAuthorization(policy =>
                policy.RequireClaim(options.PermissionClaim));
        }

        if (options.EnableUserPermissionManagement)
        {
            group.MapPost("user/{userId}/add", async (
                IPermissionService service,
                string userId,
                PermissionRequest req) =>
            {
                await service.AddPermissionToUserAsync(userId, req.Permission);
                return Results.Ok();
            });

            group.MapPost("user/{userId}/remove", async (
                IPermissionService service,
                string userId,
                PermissionRequest req) =>
            {
                await service.RemovePermissionFromUserAsync(userId, req.Permission);
                return Results.Ok();
            });

            group.MapGet("user/{userId}", async (
                IPermissionService service,
                string userId) =>
            {
                var perms = await service.GetUserPermissionsAsync(userId);
                return Results.Ok(perms);
            });
        }

        // ✅ Global Permissions
        if (options.EnableGlobalPermissionManagement)
        {
            group.MapPost("global/add", async (
                IPermissionService service,
                PermissionRequest req) =>
            {
                await service.AddGlobalPermissionAsync(req.Permission);
                return Results.Ok();
            });

            group.MapPost("global/remove", async (
                IPermissionService service,
                PermissionRequest req) =>
            {
                await service.RemoveGlobalPermissionAsync(req.Permission);
                return Results.Ok();
            });
        }

        // ✅ List All
        if (options.EnableListingAllPermissions)
        {
            group.MapGet("all", async (IPermissionService service) =>
            {
                var all = await service.GetAllPermissionsAsync();
                return Results.Ok(all);
            });
        }

        return app;
    }
}
