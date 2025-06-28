using Microsoft.AspNetCore.Authentication;
using SecureOps.Services.Cache.Enums;
using System.Security.Claims;

namespace SecureOps.Services.Cache.Options;

/// <summary>
/// Represents configuration options for secure operations, including caching and authentication settings.
/// </summary>
/// <remarks>This class provides options to configure caching behavior and authentication settings for secure
/// operations. Use the <see cref="CacheMode"/> property to specify the caching strategy, and the <see
/// cref="AuthenticationOptions"/>  property to configure authentication-related settings. The <see
/// cref="UserIdClaimType"/> property determines the  claim type used to identify the user.</remarks>
public class SecureOpsOptions
{

    /// <summary>
    /// Gets or sets the caching mode used by the application.
    /// </summary>
    /// <remarks>This property specifies the caching mode to be used for storing and retrieving data.
    /// and can be set to <see cref="CacheMode.Memory"/> for in-memory caching or <see cref="CacheMode.Redis"/> for distributed caching using Redis.</remarks>
    public CacheMode CacheMode { get; set; } = CacheMode.Memory;

    /// <summary>
    /// Gets or sets the authentication options used to configure authentication behavior.
    /// </summary>
    /// <remarks>This property allows you to specify various authentication settings, such as the default scheme 
    /// the supported schemes for authentication. It is used to configure how the application handles user authentication.</remarks>
    public AuthenticationOptions AuthenticationOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets the claim type used to identify the user ID in authentication and authorization processes.
    /// </summary>
    /// <remarks>This property specifies the claim type that will be used to retrieve the user ID from the claims principal during authentication and authorization operations. 
    /// The default value is <see cref="ClaimTypes.Name"/>, which corresponds to the user's name claim.</remarks>
    public string UserIdClaimType { get; set; } = ClaimTypes.Name;

    /// <summary>
    /// Configures the application to use Redis as the caching mechanism.
    /// </summary>
    /// <remarks>This method sets the <see cref="CacheMode"/> property to <see cref="CacheMode.Redis"/>, 
    /// enabling Redis-based caching. Ensure that Redis is properly configured and accessible  in the application's
    /// environment before calling this method.</remarks>
    public void UseRedis() => CacheMode = CacheMode.Redis;

    /// <summary>
    /// Configures the cache mode to use in-memory caching.
    /// </summary>
    /// <remarks>This method sets the <see cref="CacheMode"/> property to <see cref="CacheMode.Memory"/>,
    /// enabling in-memory caching for subsequent operations. Use this method when you want to optimize performance by
    /// storing data in memory instead of other caching mechanisms.</remarks>
    public void UseMemory() => CacheMode = CacheMode.Memory;
    
}

