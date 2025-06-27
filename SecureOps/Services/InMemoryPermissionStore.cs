using System.Collections.Concurrent;

namespace SecureOps.Services;

internal class InMemoryPermissionStore : IPermissionStore
{
    // Store user permissions
    private readonly ConcurrentDictionary<string, HashSet<string>> _userPermissions = new();

    // Optional: Global permission registry (e.g., for admin panel)
    private readonly HashSet<string> _globalPermissions = new();

    public Task AddPermissionToUserAsync(string userId, string permission)
    {
        var perms = _userPermissions.GetOrAdd(userId, _ => new HashSet<string>());
        lock (perms)
        {
            perms.Add(permission);
        }
        return Task.CompletedTask;
    }

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

    public Task<List<string>> GetAllPermissionsAsync()
    {
        lock (_globalPermissions)
        {
            return Task.FromResult(_globalPermissions.ToList());
        }
    }

    public Task AddGlobalPermissionAsync(string permission)
    {
        lock (_globalPermissions)
        {
            _globalPermissions.Add(permission);
        }
        return Task.CompletedTask;
    }

    public Task RemoveGlobalPermissionAsync(string permission)
    {
        lock (_globalPermissions)
        {
            _globalPermissions.Remove(permission);
        }
        return Task.CompletedTask;
    }

}