namespace SecureOps.Services;
public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string userId, string permission);
    Task AddPermissionToUserAsync(string userId, string permission);
    Task RemovePermissionFromUserAsync(string userId, string permission);
    Task<List<string>> GetUserPermissionsAsync(string userId);
    Task<List<string>> GetAllPermissionsAsync();
    Task AddGlobalPermissionAsync(string permission);
    Task RemoveGlobalPermissionAsync(string permission);
}
