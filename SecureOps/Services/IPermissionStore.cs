namespace SecureOps.Services;

public interface IPermissionStore
{
    Task<HashSet<string>> GetPermissionsForUserAsync(string userId);
    Task AddPermissionToUserAsync(string userId, string permission);
    Task RemovePermissionFromUserAsync(string userId, string permission);
    Task<List<string>> GetAllPermissionsAsync();
    Task AddGlobalPermissionAsync(string permission);
    Task RemoveGlobalPermissionAsync(string permission);
}
