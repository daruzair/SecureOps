using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SecureOps.Endpoints.Model;
using SecureOps.Endpoints.Options;
using SecureOps.Services;

namespace SecureOps.Endpoints;
public static class SecureOpsAppBuilderExtensions
{
    /// <summary>
    /// Registers SecureOps-related endpoints into the routing system using the provided configuration.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <param name="configure">The delegate used to configure endpoints like permission APIs.</param>
    /// <returns>The original web application (for chaining).</returns>
    public static WebApplication UseSecureOps(
        this WebApplication app,
        Action<SecureOpsOptions>? configure = null)
    {
        var ops = new SecureOpsOptions();
        configure?.Invoke(ops);
        
        if (ops.ApiOptions is not null)
        {
            MapPermissionEndpoints(app,options =>
            {
                options.RoutePrefix = ops.ApiOptions.RoutePrefix;
                options.PermissionClaim = ops.ApiOptions.PermissionClaim;
                options.EnableUserPermissionManagement = ops.ApiOptions.EnableUserPermissionManagement;
                options.EnableGlobalPermissionManagement = ops.ApiOptions.EnableGlobalPermissionManagement;
                options.EnableListingAllPermissions = ops.ApiOptions.EnableListingAllPermissions;
            });
        }
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    private static void MapPermissionEndpoints(
        IEndpointRouteBuilder app,
        Action<SecureOpsApiOptions>? configure = null)
    {
        var options = new SecureOpsApiOptions();
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

    }
}
