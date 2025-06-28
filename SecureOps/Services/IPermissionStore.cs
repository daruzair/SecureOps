namespace SecureOps.Services;

/// <summary>
/// Defines a contract for managing user-specific and global permissions.
/// </summary>
/// <remarks>This interface provides methods to retrieve, add, and remove permissions for individual users,  as
/// well as manage global permissions that apply to all users. Implementations of this interface  should ensure thread
/// safety and consistent permission management across concurrent operations.</remarks>
public interface IPermissionStore
{
    /// <summary>
    /// Asynchronously retrieves the set of permissions associated with the specified user.
    /// </summary>
    /// <remarks>This method is typically used to determine the actions or resources a user is authorized to
    /// access.  The permissions are returned as unique strings, and the caller can use them for authorization
    /// checks.</remarks>
    /// <param name="userId">The unique identifier of the user whose permissions are being retrieved. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="HashSet{T}"/> of
    /// strings,  where each string represents a permission assigned to the user. The set will be empty if the user has
    /// no permissions.</returns>
    Task<HashSet<string>> GetPermissionsForUserAsync(string userId);

    /// <summary>
    /// Asynchronously adds a specified permission to the user identified by the given user ID.
    /// </summary>
    /// <remarks>This method does not verify whether the user already has the specified permission.  Ensure
    /// that the user ID and permission are valid before calling this method.</remarks>
    /// <param name="userId">The unique identifier of the user to whom the permission will be added. Cannot be null or empty.</param>
    /// <param name="permission">The name of the permission to add. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddPermissionToUserAsync(string userId, string permission);

    /// <summary>
    /// Removes a specified permission from a user asynchronously.
    /// </summary>
    /// <param name="userId">The unique identifier of the user from whom the permission will be removed. Cannot be null or empty.</param>
    /// <param name="permission">The name of the permission to remove. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RemovePermissionFromUserAsync(string userId, string permission);

    /// <summary>
    /// Asynchronously retrieves a list of all available permissions.
    /// </summary>
    /// <remarks>The method returns a collection of permission names as strings. The list may be empty if no
    /// permissions are available.</remarks>
    /// <returns>A task representing the asynchronous operation. The task result contains a list of strings, where each string
    /// represents a permission name.</returns>
    Task<List<string>> GetAllPermissionsAsync();

    /// <summary>
    /// Adds a global permission to the system.
    /// </summary>
    /// <remarks>This method adds a permission that applies globally across the system. Ensure that the
    /// permission name is valid and does not conflict with existing permissions.</remarks>
    /// <param name="permission">The name of the permission to add. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddGlobalPermissionAsync(string permission);

    /// <summary>
    /// Removes a global permission from the system.
    /// </summary>
    /// <remarks>This method removes the specified global permission, if it exists. If the permission does not
    /// exist,  the operation completes successfully without making any changes.</remarks>
    /// <param name="permission">The name of the permission to remove. This must be a non-null, non-empty string.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RemoveGlobalPermissionAsync(string permission);
}
