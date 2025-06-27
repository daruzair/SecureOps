using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace SecureOps.Services.CacheServices;
internal class RedisCachedPermissionService : IPermissionService
{
    private readonly IPermissionStore _store;
    private readonly IDistributedCache _redis;
    private const string Prefix = "perm:";

    public RedisCachedPermissionService(IPermissionStore store, IDistributedCache redis)
    {
        _store = store;
        _redis = redis;
    }

    private string GetKey(string userId) => $"{Prefix}{userId}";

    public async Task<List<string>> GetUserPermissionsAsync(string userId)
    {
        var key = GetKey(userId);
        var json = await _redis.GetStringAsync(key);

        if (json != null)
            return JsonSerializer.Deserialize<List<string>>(json) ?? new();

        var perms = (await _store.GetPermissionsForUserAsync(userId)).ToList();
        await _redis.SetStringAsync(key, JsonSerializer.Serialize(perms));
        return perms;
    }

    public async Task AddPermissionToUserAsync(string userId, string permission)
    {
        await _store.AddPermissionToUserAsync(userId, permission);
        await _redis.RemoveAsync(GetKey(userId));
    }

    public async Task RemovePermissionFromUserAsync(string userId, string permission)
    {
        await _store.RemovePermissionFromUserAsync(userId, permission);
        await _redis.RemoveAsync(GetKey(userId));
    }

    public async Task<bool> HasPermissionAsync(string userId, string permission)
    {
        var perms = await GetUserPermissionsAsync(userId);
        return perms.Contains(permission);
    }

    public Task AddGlobalPermissionAsync(string permission) =>
        _store.AddGlobalPermissionAsync(permission);

    public Task RemoveGlobalPermissionAsync(string permission) =>
        _store.RemoveGlobalPermissionAsync(permission);

    public Task<List<string>> GetAllPermissionsAsync() =>
        _store.GetAllPermissionsAsync();
}