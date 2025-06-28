using Microsoft.Extensions.Caching.Memory;

namespace SecureOps.Services.CacheServices;
/// <summary>
/// Provides a cached implementation of permission management, reducing redundant calls to the underlying data store.
/// </summary>
/// <remarks>This service caches user-specific and global permissions to improve performance by minimizing
/// repeated access to the underlying <see cref="IPermissionStore"/>. Cached permissions are automatically invalidated
/// when changes are made, ensuring consistency.</remarks>
internal class CachedPermissionService : IPermissionService
{
    /// <summary>
    /// Represents a memory cache used for storing and retrieving data in memory.
    /// </summary>
    /// <remarks>This field is a readonly instance of <see cref="IMemoryCache"/>, which provides methods for 
    /// caching data in memory. It is intended for internal use and cannot be modified after initialization.</remarks>
    private readonly IMemoryCache _cache;

    /// <summary>
    /// Represents the underlying store used to manage permissions.
    /// </summary>
    /// <remarks>This field is read-only and is intended to provide access to the implementation of  <see
    /// cref="IPermissionStore"/> for managing permission-related data.</remarks>
    private readonly IPermissionStore _store;

    /// <summary>
    /// Represents the configuration options for cache entries, including expiration settings.
    /// </summary>
    /// <remarks>This instance is initialized with a sliding expiration of 30 minutes, meaning the cache
    /// entry's  expiration time is reset whenever it is accessed. Use this configuration to ensure cache entries 
    /// remain valid as long as they are actively used.</remarks>
    private readonly MemoryCacheEntryOptions _cacheOptions =
        new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(30) };

    /// <summary>
    /// Initializes a new instance of the <see cref="CachedPermissionService"/> class, which provides permission caching
    /// functionality.
    /// </summary>
    /// <param name="cache">The memory cache used to store permission data for improved performance.</param>
    /// <param name="store">The underlying permission store used to retrieve permissions when they are not available in the cache.</param>
    public CachedPermissionService(IMemoryCache cache, IPermissionStore store)
    {
        _cache = cache;
        _store = store;
    }

    /// <summary>
    /// Generates a cache key for storing or retrieving user permissions.
    /// </summary>
    /// <param name="userId">The unique identifier of the user. Cannot be null or empty.</param>
    /// <returns>A string representing the cache key for the specified user's permissions.</returns>
    private string GetCacheKey(string userId) => $"permissions_{userId}";

    /// <summary>
    /// Determines whether the specified user has the given permission.
    /// </summary>
    /// <remarks>This method retrieves the user's permissions asynchronously and checks whether the specified
    /// permission exists.</remarks>
    /// <param name="userId">The unique identifier of the user whose permissions are being checked. Cannot be <see langword="null"/> or
    /// empty.</param>
    /// <param name="permission">The name of the permission to check. Cannot be <see langword="null"/> or empty.</param>
    /// <returns><see langword="true"/> if the user has the specified permission; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> HasPermissionAsync(string userId, string permission)
    {
        var permissions = await GetUserPermissionsAsync(userId);
        return permissions.Contains(permission);
    }

    /// <summary>
    /// Adds a specified permission to the user asynchronously.
    /// </summary>
    /// <remarks>This method updates the underlying data store to associate the specified permission with the
    /// user. After the operation, the user's cached data is invalidated to ensure subsequent reads reflect the updated
    /// permissions.</remarks>
    /// <param name="userId">The unique identifier of the user to whom the permission will be added. Cannot be null or empty.</param>
    /// <param name="permission">The permission to add to the user. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task AddPermissionToUserAsync(string userId, string permission)
    {
        await _store.AddPermissionToUserAsync(userId, permission);
        _cache.Remove(GetCacheKey(userId));
    }

    /// <summary>
    /// Removes a specified permission from a user asynchronously.
    /// </summary>
    /// <remarks>This method updates the underlying data store to remove the specified permission from the
    /// user and clears the associated cache entry for the user.</remarks>
    /// <param name="userId">The unique identifier of the user from whom the permission will be removed. Cannot be null or empty.</param>
    /// <param name="permission">The name of the permission to remove. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task RemovePermissionFromUserAsync(string userId, string permission)
    {
        await _store.RemovePermissionFromUserAsync(userId, permission);
        _cache.Remove(GetCacheKey(userId));
    }

    /// <summary>
    /// Retrieves the list of permissions associated with a specific user.
    /// </summary>
    /// <remarks>This method combines the user's specific permissions with global permissions. Results are
    /// cached to improve performance for subsequent calls.</remarks>
    /// <param name="userId">The unique identifier of the user whose permissions are being retrieved. Must not be <see langword="null"/> or
    /// empty.</param>
    /// <returns>A list of strings representing the user's permissions. If the user has no permissions,  an empty list is
    /// returned.</returns>
    public async Task<List<string>> GetUserPermissionsAsync(string userId)
    {
        var key = GetCacheKey(userId);
        if (!_cache.TryGetValue(key, out List<string>? cached))
        {
            var perms = await _store.GetPermissionsForUserAsync(userId);
            var globalPerms = await _store.GetAllPermissionsAsync();
            perms.UnionWith(globalPerms);
            cached = perms.ToList();
            _cache.Set(key, cached, _cacheOptions);
        }

        return cached ?? new();
    }

    /// <summary>
    /// Asynchronously retrieves a list of all available permissions.
    /// </summary>
    /// <remarks>This method is useful for scenarios where you need to enumerate all permissions in the
    /// system,  such as for access control or user role management.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of strings,  where each
    /// string represents a permission. If no permissions are available, the list will be empty.</returns>
    public Task<List<string>> GetAllPermissionsAsync() => _store.GetAllPermissionsAsync();

    /// <summary>
    /// Adds a global permission to the underlying store asynchronously.
    /// </summary>
    /// <param name="permission">The name of the permission to add. Cannot be <see langword="null"/> or empty.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task AddGlobalPermissionAsync(string permission) => _store.AddGlobalPermissionAsync(permission);

    /// <summary>
    /// Removes a global permission from the system.
    /// </summary>
    /// <remarks>This method removes the specified global permission from the underlying store. Ensure that
    /// the permission exists before calling this method to avoid unexpected behavior.</remarks>
    /// <param name="permission">The name of the permission to remove. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task RemoveGlobalPermissionAsync(string permission) => _store.RemoveGlobalPermissionAsync(permission);
}
