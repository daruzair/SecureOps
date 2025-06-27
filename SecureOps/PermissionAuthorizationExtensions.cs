using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SecureOps.Services;
using SecureOps.Services.Cache.Enums;
using SecureOps.Services.Cache.Options;
using SecureOps.Services.CacheServices;

namespace SecureOps;

public static class PermissionAuthorizationExtensions
{
    public static IServiceCollection AddPermissionAuthorization(
        this IServiceCollection services,
        Action<PermissionAuthorizationOptions> configure)
    {
        var options = new PermissionAuthorizationOptions();
        configure(options);

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

        return services;
    }

    public static IServiceCollection AddPermissionAuthorization<TStore>(
        this IServiceCollection services,
        Action<PermissionAuthorizationOptions> configure)
        where TStore : class, IPermissionStore
    {
        services.AddScoped<IPermissionStore, TStore>();

        return services.AddPermissionAuthorization(configure);
    }
}
