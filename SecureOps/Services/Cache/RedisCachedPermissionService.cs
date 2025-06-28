﻿using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace SecureOps.Services.CacheServices;

/// <summary>
/// Provides permission management functionality with caching support using Redis.
/// </summary>
/// <remarks>This service combines permissions retrieved from an underlying <see cref="IPermissionStore"/> with
/// caching provided by an <see cref="IDistributedCache"/> to improve performance. Permissions are cached per user and
/// invalidated when changes are made to the user's permissions.</remarks>
internal class RedisCachedPermissionService : IPermissionService
{
    /// <summary>
    /// Represents the underlying store used to manage permissions.
    /// </summary>
    /// <remarks>This field is read-only and intended for internal use to interact with the permission storage
    /// mechanism.</remarks>
    private readonly IPermissionStore _store;

    /// <summary>
    /// Represents a distributed cache instance used for storing and retrieving data.
    /// </summary>
    /// <remarks>This field is intended to hold a reference to an implementation of <see
    /// cref="IDistributedCache"/>,  which provides distributed caching functionality. It is typically used for caching
    /// data across  multiple servers in a distributed system.</remarks>
    private readonly IDistributedCache _redis;

    /// <summary>
    /// Represents the prefix used to identify permission-related strings.
    /// </summary>
    private const string Prefix = "perm:";

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisCachedPermissionService"/> class,  which provides permission
    /// management with caching support using Redis.
    /// </summary>
    /// <remarks>This service combines the functionality of a permission store with Redis caching to improve 
    /// performance by reducing direct access to the permission store. Ensure that both the  <paramref name="store"/>
    /// and <paramref name="redis"/> instances are properly configured  before using this service.</remarks>
    /// <param name="store">The underlying permission store used to retrieve and manage permissions.</param>
    /// <param name="redis">The Redis distributed cache used to store and retrieve cached permission data.</param>
    public RedisCachedPermissionService(IPermissionStore store, IDistributedCache redis)
    {
        _store = store;
        _redis = redis;
    }

    /// <summary>
    /// Generates a unique key for the specified user based on the configured prefix.
    /// </summary>
    /// <param name="userId">The identifier of the user for whom the key is generated. Cannot be null or empty.</param>
    /// <returns>A string representing the unique key for the user.</returns>
    private string GetKey(string userId) => $"{Prefix}{userId}";

    /// <summary>
    /// Asynchronously retrieves the list of permissions associated with a specific user.
    /// </summary>
    /// <remarks>This method retrieves permissions from a cache if available; otherwise, it fetches them from
    /// the underlying data store. Global permissions are also included in the result. The retrieved permissions are
    /// cached for future use.</remarks>
    /// <param name="userId">The unique identifier of the user whose permissions are being retrieved. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of strings representing the
    /// user's permissions. If no permissions are found, an empty list is returned.</returns>
    public async Task<List<string>> GetUserPermissionsAsync(string userId)
    {
        var key = GetKey(userId);
        var json = await _redis.GetStringAsync(key);

        if (json != null)
            return JsonSerializer.Deserialize<List<string>>(json) ?? new();

        var perms = await _store.GetPermissionsForUserAsync(userId);
        var globalPerms = await _store.GetAllPermissionsAsync();
        perms.UnionWith(globalPerms);

        var permsList = perms.ToList();
        await _redis.SetStringAsync(key, JsonSerializer.Serialize(permsList));
        return permsList;
    }

    /// <summary>
    /// Adds a specified permission to the user identified by the given user ID.
    /// </summary>
    /// <remarks>This method updates the underlying data store to associate the specified permission with the
    /// user. It also removes any cached data related to the user to ensure consistency.</remarks>
    /// <param name="userId">The unique identifier of the user to whom the permission will be added. Cannot be null or empty.</param>
    /// <param name="permission">The permission to add to the user. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task AddPermissionToUserAsync(string userId, string permission)
    {
        await _store.AddPermissionToUserAsync(userId, permission);
        await _redis.RemoveAsync(GetKey(userId));
    }

    /// <summary>
    /// Removes a specified permission from a user asynchronously.
    /// </summary>
    /// <remarks>This method removes the specified permission from the user in the underlying data store and
    /// updates the cache. Ensure that both <paramref name="userId"/> and <paramref name="permission"/> are valid and
    /// non-empty.</remarks>
    /// <param name="userId">The unique identifier of the user from whom the permission will be removed. Cannot be null or empty.</param>
    /// <param name="permission">The name of the permission to remove. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task RemovePermissionFromUserAsync(string userId, string permission)
    {
        await _store.RemovePermissionFromUserAsync(userId, permission);
        await _redis.RemoveAsync(GetKey(userId));
    }

    /// <summary>
    /// Determines whether the specified user has the given permission.
    /// </summary>
    /// <remarks>This method retrieves the user's permissions asynchronously and checks if the specified
    /// permission exists in the user's permission set.</remarks>
    /// <param name="userId">The unique identifier of the user whose permissions are being checked. Cannot be null or empty.</param>
    /// <param name="permission">The permission to check for. Cannot be null or empty.</param>
    /// <returns><see langword="true"/> if the user has the specified permission; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> HasPermissionAsync(string userId, string permission)
    {
        var perms = await GetUserPermissionsAsync(userId);
        return perms.Contains(permission);
    }

    /// <summary>
    /// Adds a global permission to the system asynchronously.
    /// </summary>
    /// <param name="permission">The name of the permission to add. This value cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task AddGlobalPermissionAsync(string permission) =>
        _store.AddGlobalPermissionAsync(permission);

    /// <summary>
    /// Removes a global permission from the system.
    /// </summary>
    /// <param name="permission">The name of the permission to remove. Cannot be <see langword="null"/> or empty.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task RemoveGlobalPermissionAsync(string permission) =>
        _store.RemoveGlobalPermissionAsync(permission);

    /// <summary>
    /// Asynchronously retrieves a list of all available permissions.
    /// </summary>
    /// <remarks>This method queries the underlying data store to fetch all permissions. The caller can use
    /// the returned  list to determine the available permissions for further processing or validation.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of strings,  where each
    /// string represents a permission name. If no permissions are available, the list will be empty.</returns>
    public Task<List<string>> GetAllPermissionsAsync() =>
        _store.GetAllPermissionsAsync();
}