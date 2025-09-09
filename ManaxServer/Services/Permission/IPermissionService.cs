namespace ManaxServer.Services.Permission;

public interface IPermissionService
{
    bool UserHasPermission(long userId, ManaxLibrary.DTO.User.Permission permission);
    Task<bool> UserHasPermissionAsync(long userId, ManaxLibrary.DTO.User.Permission permission);
    IEnumerable<ManaxLibrary.DTO.User.Permission> GetUserPermissions(long userId);
    Task<IEnumerable<ManaxLibrary.DTO.User.Permission>> GetUserPermissionsAsync(long userId);
    void SetUserPermissions(long userId, IEnumerable<ManaxLibrary.DTO.User.Permission> permissions);
    Task SetUserPermissionsAsync(long userId, IEnumerable<ManaxLibrary.DTO.User.Permission> permissions);
}
