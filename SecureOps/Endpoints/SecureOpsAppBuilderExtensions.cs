using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SecureOps.Endpoints.Model;
using SecureOps.Endpoints.Options;
using SecureOps.Services;

namespace SecureOps.Endpoints;

/// <summary>
/// Configures SecureOps endpoints in the web application, enabling features such as user and global  permission
/// management, and listing all permissions.
/// </summary>
/// <remarks>This method registers SecureOps endpoints based on the provided configuration. It applies 
/// authentication and authorization middleware to ensure secure access to the endpoints.   Example usage: <code> var
/// builder = WebApplication.CreateBuilder(args); var app = builder.Build();  app.UseSecureOps(options => {    
/// options.SecureOpsEndpointsOptions.RoutePrefix = "/secure-ops";    
/// options.SecureOpsEndpointsOptions.EnableUserPermissionManagement = true;    
/// options.SecureOpsEndpointsOptions.EnableGlobalPermissionManagement = true; });  app.Run(); </code></remarks>
public static class SecureOpsAppBuilderExtensions
{
    /// <summary>
    /// Configures the application to use secure operations middleware, including authentication,  authorization, and
    /// optional permission management endpoints.
    /// </summary>
    /// <remarks>This method sets up authentication and authorization middleware for the application. If 
    /// <paramref name="configure"/> is provided and the <see cref="SecureOpsMiddlewareOptions"/>  include endpoint
    /// options, permission management endpoints will be mapped to the application.  Use this method to enable secure
    /// operations in your application, including features such as  user and global permission management, and listing
    /// all permissions. Ensure that the application  has appropriate authentication and authorization mechanisms
    /// configured.</remarks>
    /// <param name="app">The <see cref="WebApplication"/> instance to configure.</param>
    /// <param name="configure">An optional delegate to configure <see cref="SecureOpsMiddlewareOptions"/>. If provided,  this delegate allows
    /// customization of secure operations settings, such as endpoint routing  and permission management options.</param>
    /// <returns>The configured <see cref="WebApplication"/> instance.</returns>
    public static WebApplication UseSecureOps(
        this WebApplication app,
        Action<SecureOpsMiddlewareOptions>? configure = null)
    {
        var ops = new SecureOpsMiddlewareOptions();
        configure?.Invoke(ops);
        
        if (ops.SecureOpsEndpointsOptions is not null)
        {
            MapPermissionEndpoints(app,options =>
            {
                options.RoutePrefix = ops.SecureOpsEndpointsOptions.RoutePrefix;
                options.PermissionClaim = ops.SecureOpsEndpointsOptions.PermissionClaim;
                options.EnableUserPermissionManagement = ops.SecureOpsEndpointsOptions.EnableUserPermissionManagement;
                options.EnableGlobalPermissionManagement = ops.SecureOpsEndpointsOptions.EnableGlobalPermissionManagement;
                options.EnableListingAllPermissions = ops.SecureOpsEndpointsOptions.EnableListingAllPermissions;
            });
        }
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
    /// <summary>
    /// Configures and maps permission-related endpoints to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <remarks>This method maps endpoints for managing user and global permissions based on the
    /// configuration provided. The following endpoints may be mapped, depending on the options: <list type="bullet">
    /// <item> <description>User permission management endpoints, including adding, removing, and retrieving permissions
    /// for a specific user.</description> </item> <item> <description>Global permission management endpoints, including
    /// adding and removing global permissions.</description> </item> <item> <description>An endpoint for listing all
    /// permissions.</description> </item> </list> Authorization requirements can be applied to the endpoints based on
    /// the <see cref="SecureOpsEndpointsOptions.PermissionClaim"/>.</remarks>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> used to define the endpoints.</param>
    /// <param name="configure">An optional delegate to configure <see cref="SecureOpsEndpointsOptions"/> for customizing endpoint behavior. If
    /// not provided, default options are used.</param>
    private static void MapPermissionEndpoints(
        IEndpointRouteBuilder app,
        Action<SecureOpsEndpointsOptions>? configure = null)
    {
        var options = new SecureOpsEndpointsOptions();
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
