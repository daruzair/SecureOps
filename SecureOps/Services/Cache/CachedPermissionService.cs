using Microsoft.Extensions.Caching.Memory;

namespace SecureOps.Services.CacheServices;
internal class CachedPermissionService : IPermissionService
{
    private readonly IMemoryCache _cache;
    private readonly IPermissionStore _store;

    private readonly MemoryCacheEntryOptions _cacheOptions =
        new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(30) };

    public CachedPermissionService(IMemoryCache cache, IPermissionStore store)
    {
        _cache = cache;
        _store = store;
    }

    private string GetCacheKey(string userId) => $"permissions_{userId}";

    public async Task<bool> HasPermissionAsync(string userId, string permission)
    {
        var permissions = await GetUserPermissionsAsync(userId);
        return permissions.Contains(permission);
    }

    public async Task AddPermissionToUserAsync(string userId, string permission)
    {
        await _store.AddPermissionToUserAsync(userId, permission);
        _cache.Remove(GetCacheKey(userId));
    }

    public async Task RemovePermissionFromUserAsync(string userId, string permission)
    {
        await _store.RemovePermissionFromUserAsync(userId, permission);
        _cache.Remove(GetCacheKey(userId));
    }

    public async Task<List<string>> GetUserPermissionsAsync(string userId)
    {
        var key = GetCacheKey(userId);
        if (!_cache.TryGetValue(key, out List<string>? cached))
        {
            var perms = await _store.GetPermissionsForUserAsync(userId);
            cached = perms.ToList();
            _cache.Set(key, cached, _cacheOptions);
        }

        return cached ?? new();
    }

    public Task<List<string>> GetAllPermissionsAsync() => _store.GetAllPermissionsAsync();

    public Task AddGlobalPermissionAsync(string permission) => _store.AddGlobalPermissionAsync(permission);

    public Task RemoveGlobalPermissionAsync(string permission) => _store.RemoveGlobalPermissionAsync(permission);
}
