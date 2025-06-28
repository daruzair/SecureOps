using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SecureOps.Services;
using SecureOps.Services.Cache.Enums;
using SecureOps.Services.Cache.Options;
using SecureOps.Services.CacheServices;

namespace SecureOps;

/// <summary>
/// Provides extension methods for configuring secure operations in an application.
/// </summary>
/// <remarks>The <c>SecureOpsExtensions</c> class includes methods to configure authentication, authorization, 
/// and permission management for secure operations. These methods allow customization of authentication  schemes,
/// caching strategies, and permission store implementations.</remarks>
public static class SecureOpsExtensions
{
    /// <summary>
    /// Configures secure operations for the application by setting up authentication, authorization,  and permission
    /// services with optional caching mechanisms.
    /// </summary>
    /// <remarks>This method registers the necessary services for secure operations, including authentication,
    /// authorization, and permission management. It supports both in-memory and Redis-based caching for  permission
    /// services, depending on the <see cref="SecureOpsOptions.CacheMode"/> configuration.  If <paramref
    /// name="configure"/> is provided, it allows customization of authentication options,  caching mode, and other
    /// settings. By default, in-memory caching is used unless Redis is explicitly  configured.  Example usage: <code>
    /// services.AddSecureOps(options => {     options.CacheMode = CacheMode.Redis;     options.AuthenticationOptions =
    /// new AuthenticationOptions     {         DefaultScheme = "MyScheme",         DefaultChallengeScheme =
    /// "MyChallengeScheme"     }; }); </code></remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the secure operations services are added.</param>
    /// <param name="configure">An optional delegate to configure <see cref="SecureOpsOptions"/>. If not provided, default options are used.</param>
    /// <returns>An <see cref="AuthenticationBuilder"/> that can be used to further configure authentication schemes.</returns>
    public static AuthenticationBuilder AddSecureOps(this IServiceCollection services, Action<SecureOpsOptions>? configure = null)
    {
        var options = new SecureOpsOptions();
        configure?.Invoke(options);
        
        services.TryAddSingleton<IPermissionStore, InMemoryPermissionStore>();

        if (options.CacheMode == CacheMode.Redis)
        {
            services.AddScoped<IPermissionService, RedisCachedPermissionService>();
        }
        else
        {
            services.AddMemoryCache();
            services.AddScoped<IPermissionService, CachedPermissionService>();
        }
        // Register Authentication
        var builder = services.AddAuthentication(auth =>
        {
            if (options.AuthenticationOptions != null)
            {
                auth.DefaultScheme = options.AuthenticationOptions.DefaultScheme;
                auth.DefaultChallengeScheme = options.AuthenticationOptions.DefaultChallengeScheme;
                auth.DefaultAuthenticateScheme = options.AuthenticationOptions.DefaultAuthenticateScheme;
                auth.DefaultForbidScheme = options.AuthenticationOptions.DefaultForbidScheme;
                auth.DefaultSignInScheme = options.AuthenticationOptions.DefaultSignInScheme;
                auth.DefaultSignOutScheme = options.AuthenticationOptions.DefaultSignOutScheme;
            }
        });

        services.AddAuthorization();
        services.AddSingleton(options);
        return builder;
    }
    
    /// <summary>
    /// Adds SecureOps authentication to the specified service collection.
    /// </summary>
    /// <remarks>This method configures SecureOps authentication with the specified default scheme. Use this
    /// method to integrate SecureOps authentication into your application's service collection.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the SecureOps authentication is added.</param>
    /// <param name="DefaultScheme">The default authentication scheme to be used. This value cannot be null or empty.</param>
    /// <returns>An <see cref="AuthenticationBuilder"/> that can be used to further configure authentication.</returns>
    public static AuthenticationBuilder AddSecureOps(this IServiceCollection services, string DefaultScheme)
    {
        return services.AddSecureOps(configure =>
        {
            configure.AuthenticationOptions.DefaultScheme = DefaultScheme;
        });
    }
    
    /// <summary>
    /// Adds SecureOps authentication and authorization services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>This method registers the specified <typeparamref name="TStore"/> as the implementation of
    /// <see cref="IPermissionStore"/> in the service collection. It also sets up SecureOps authentication and
    /// authorization using the provided configuration.</remarks>
    /// <typeparam name="TStore">The type of the permission store to be used for managing permissions. Must implement <see
    /// cref="IPermissionStore"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the SecureOps services will be added.</param>
    /// <param name="configure">A delegate to configure the <see cref="SecureOpsOptions"/> for SecureOps.</param>
    /// <returns>An <see cref="AuthenticationBuilder"/> that can be used to further configure authentication.</returns>
    public static AuthenticationBuilder AddSecureOps<TStore>(this IServiceCollection services,Action<SecureOpsOptions> configure)
        where TStore : class, IPermissionStore
    {
        services.AddScoped<IPermissionStore, TStore>();

        return services.AddSecureOps(configure);
    }
    
    /// <summary>
    /// Adds secure operations authentication and authorization services to the specified service collection.
    /// </summary>
    /// <remarks>This method registers the specified <typeparamref name="TStore"/> as the implementation of 
    /// <see cref="IPermissionStore"/> in the service collection. It also configures secure operations  authentication
    /// using the provided default scheme.</remarks>
    /// <typeparam name="TStore">The type of the permission store to be used for managing permissions.  Must implement <see
    /// cref="IPermissionStore"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the secure operations services will be added.</param>
    /// <param name="DefaultScheme">The default authentication scheme to be used for secure operations.</param>
    /// <returns>An <see cref="AuthenticationBuilder"/> that can be used to further configure authentication.</returns>
    public static AuthenticationBuilder AddSecureOps<TStore>(this IServiceCollection services,string DefaultScheme)
        where TStore : class, IPermissionStore
    {
        services.AddScoped<IPermissionStore, TStore>();

        return services.AddSecureOps(DefaultScheme);
    }
}
