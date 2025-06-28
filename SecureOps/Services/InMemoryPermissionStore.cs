using System.Collections.Concurrent;

namespace SecureOps.Services;

/// <summary>
/// Provides an in-memory implementation of the <see cref="IPermissionStore"/> interface for managing user-specific and
/// global permissions.
/// </summary>
/// <remarks>This class stores permissions in memory using thread-safe collections. It is suitable for scenarios
/// where persistence is not required, such as testing or lightweight applications. Permissions are stored per user and
/// globally, with methods to add, remove, and retrieve them. Note that this implementation does not persist data across
/// application restarts.</remarks>
internal class InMemoryPermissionStore : IPermissionStore
{
    /// <summary>
    /// Represents a thread-safe collection that maps user identifiers to their associated permissions.
    /// </summary>
    /// <remarks>This dictionary is used to store and manage permissions for users in a concurrent
    /// environment. Each key is a user identifier (e.g., a username or user ID), and the associated value is a set of
    /// permissions assigned to that user. The use of <see cref="ConcurrentDictionary{TKey, TValue}"/> ensures
    /// thread-safe operations for adding, removing, and updating user permissions.</remarks>
    private readonly ConcurrentDictionary<string, HashSet<string>> _userPermissions = new();

    /// <summary>
    /// Represents a collection of global permissions.
    /// </summary>
    /// <remarks>This field is used to store a set of permissions that apply globally across the application.
    /// It is initialized as an empty set and is intended for internal use only.</remarks>
    private readonly HashSet<string> _globalPermissions = new();

    /// <summary>
    /// Adds a permission to the specified user's set of permissions.
    /// </summary>
    /// <remarks>This method is thread-safe and ensures that the user's permissions are updated atomically. If
    /// the user does not already have a set of permissions, a new set will be created.</remarks>
    /// <param name="userId">The unique identifier of the user to whom the permission will be added. Cannot be null or empty.</param>
    /// <param name="permission">The permission to add to the user's set of permissions. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task AddPermissionToUserAsync(string userId, string permission)
    {
        var perms = _userPermissions.GetOrAdd(userId, _ => new HashSet<string>());
        lock (perms)
        {
            perms.Add(permission);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes a specified permission from the user's permission list.
    /// </summary>
    /// <remarks>If the user does not have the specified permission or the user ID is not found, no action is
    /// taken. This method is thread-safe and ensures proper synchronization when modifying the user's permission
    /// list.</remarks>
    /// <param name="userId">The unique identifier of the user whose permission is to be removed. Cannot be null or empty.</param>
    /// <param name="permission">The permission to remove from the user's list. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task RemovePermissionFromUserAsync(string userId, string permission)
    {
        if (_userPermissions.TryGetValue(userId, out var perms))
        {
            lock (perms)
            {
                perms.Remove(permission);
            }
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves the set of permissions associated with the specified user.
    /// </summary>
    /// <remarks>This method is thread-safe and ensures that the returned set of permissions is a snapshot of
    /// the user's current permissions. Changes to the user's permissions after the method is called will not affect the
    /// returned set.</remarks>
    /// <param name="userId">The unique identifier of the user whose permissions are to be retrieved. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="HashSet{T}"/> of
    /// strings representing the user's permissions. If the user has no permissions, an empty set is returned.</returns>
    public Task<HashSet<string>> GetPermissionsForUserAsync(string userId)
    {
        if (_userPermissions.TryGetValue(userId, out var perms))
        {
            lock (perms)
            {
                return Task.FromResult(new HashSet<string>(perms));
            }
        }
        return Task.FromResult(new HashSet<string>());
    }

    /// <summary>
    /// Retrieves a list of all global permissions.
    /// </summary>
    /// <remarks>The method returns a snapshot of the current global permissions as a list of strings. The
    /// returned list is a copy, ensuring that modifications to the result do not affect the original collection. This
    /// method is thread-safe.</remarks>
    /// <returns>A task representing the asynchronous operation. The task result contains a list of strings representing the
    /// global permissions.</returns>
    public Task<List<string>> GetAllPermissionsAsync()
    {
        lock (_globalPermissions)
        {
            return Task.FromResult(_globalPermissions.ToList());
        }
    }

    /// <summary>
    /// Adds a global permission to the collection.
    /// </summary>
    /// <remarks>This method is thread-safe and ensures that the permission is added to the global collection.
    /// If the specified permission already exists in the collection, it will not be added again.</remarks>
    /// <param name="permission">The permission to add. Cannot be <see langword="null"/> or empty.</param>
    /// <returns>A completed <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task AddGlobalPermissionAsync(string permission)
    {
        lock (_globalPermissions)
        {
            _globalPermissions.Add(permission);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes a global permission from the collection of permissions.
    /// </summary>
    /// <remarks>This method is thread-safe and ensures that the permission is removed from the global
    /// permissions collection. If the specified permission does not exist in the collection, no action is
    /// taken.</remarks>
    /// <param name="permission">The name of the permission to remove. Cannot be <see langword="null"/> or empty.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task RemoveGlobalPermissionAsync(string permission)
    {
        lock (_globalPermissions)
        {
            _globalPermissions.Remove(permission);
        }
        return Task.CompletedTask;
    }

}