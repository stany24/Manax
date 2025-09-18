using ManaxLibrary.DTO.User;
using ManaxServer.Services.Permission;

namespace ManaxTests.Server.Mocks;

public class MockPermissionService : IPermissionService
{
    private readonly Dictionary<long, IEnumerable<Permission>> _permissions = [];

    public bool UserHasPermission(long userId, Permission permission)
    {
        return _permissions.TryGetValue(userId, out IEnumerable<Permission>? permission1) &&
               permission1.Contains(permission);
    }

    public Task<bool> UserHasPermissionAsync(long userId, Permission permission)
    {
        return Task.FromResult(UserHasPermission(userId, permission));
    }

    public IEnumerable<Permission> GetUserPermissions(long userId)
    {
        return _permissions.TryGetValue(userId, out IEnumerable<Permission>? permission1) ? permission1 : [];
    }

    public Task<IEnumerable<Permission>> GetUserPermissionsAsync(long userId)
    {
        return Task.FromResult(GetUserPermissions(userId));
    }

    public void SetUserPermissions(long userId, IEnumerable<Permission> permissions)
    {
        _permissions[userId] = permissions;
    }

    public Task SetUserPermissionsAsync(long userId, IEnumerable<Permission> permissions)
    {
        SetUserPermissions(userId, permissions);
        return Task.CompletedTask;
    }
}