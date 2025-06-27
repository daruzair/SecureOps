using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SecureOps.Services;
using SecureOps.Services.Cache.Enums;
using SecureOps.Services.Cache.Options;
using SecureOps.Services.CacheServices;

namespace SecureOps;

public static class SecureOpsExtensions
{
    public static IServiceCollection AddSecureOps(
         this IServiceCollection services,
         Action<SecureOpsOptions>? configure = null)
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

        return services;
    }

    public static IServiceCollection AddSecureOps<TStore>(
        this IServiceCollection services,
        Action<SecureOpsOptions> configure)
        where TStore : class, IPermissionStore
    {
        services.AddScoped<IPermissionStore, TStore>();

        return services.AddSecureOps(configure);
    }
}
