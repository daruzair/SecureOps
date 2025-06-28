using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SecureOps.Services;
using SecureOps.Services.Cache.Enums;
using SecureOps.Services.Cache.Options;
using SecureOps.Services.CacheServices;

namespace SecureOps;

public static class SecureOpsExtensions
{
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
    public static AuthenticationBuilder AddSecureOps(this IServiceCollection services, string DefaultScheme)
    {
        return services.AddSecureOps(configure =>
        {
            configure.AuthenticationOptions.DefaultScheme = DefaultScheme;
        });
    }
    public static AuthenticationBuilder AddSecureOps<TStore>(this IServiceCollection services,Action<SecureOpsOptions> configure)
        where TStore : class, IPermissionStore
    {
        services.AddScoped<IPermissionStore, TStore>();

        return services.AddSecureOps(configure);
    }
    public static AuthenticationBuilder AddSecureOps<TStore>(this IServiceCollection services,string DefaultScheme)
        where TStore : class, IPermissionStore
    {
        services.AddScoped<IPermissionStore, TStore>();

        return services.AddSecureOps(DefaultScheme);
    }
}
