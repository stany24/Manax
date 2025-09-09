using ManaxServer.Models;
using ManaxServer.Models.User;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Services.Permission;

public class PermissionService(IServiceScopeFactory serviceScopeFactory) : Service, IPermissionService
{
    public bool UserHasPermission(long userId, ManaxLibrary.DTO.User.Permission permission)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        ManaxContext context = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        
        return context.UserPermissions
            .Any(up => up.UserId == userId && up.Permission == permission);
    }

    public async Task<bool> UserHasPermissionAsync(long userId, ManaxLibrary.DTO.User.Permission permission)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        ManaxContext context = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        
        return await context.UserPermissions
            .AnyAsync(up => up.UserId == userId && up.Permission == permission);
    }

    public IEnumerable<ManaxLibrary.DTO.User.Permission> GetUserPermissions(long userId)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        ManaxContext context = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        
        return context.UserPermissions
            .Where(up => up.UserId == userId)
            .Select(up => up.Permission)
            .ToList();
    }

    public async Task<IEnumerable<ManaxLibrary.DTO.User.Permission>> GetUserPermissionsAsync(long userId)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        ManaxContext context = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        
        return await context.UserPermissions
            .Where(up => up.UserId == userId)
            .Select(up => up.Permission)
            .ToListAsync();
    }

    public void SetUserPermissions(long userId, IEnumerable<ManaxLibrary.DTO.User.Permission> permissions)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        ManaxContext context = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        
        List<UserPermission> existingPermissions = context.UserPermissions
            .Where(up => up.UserId == userId)
            .ToList();
        
        context.UserPermissions.RemoveRange(existingPermissions);
        
        IEnumerable<UserPermission> newPermissions = permissions.Select(p => new UserPermission
        {
            UserId = userId,
            Permission = p
        });
        
        context.UserPermissions.AddRange(newPermissions);
        context.SaveChanges();
    }

    public async Task SetUserPermissionsAsync(long userId, IEnumerable<ManaxLibrary.DTO.User.Permission> permissions)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        ManaxContext context = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        
        List<UserPermission> existingPermissions = await context.UserPermissions
            .Where(up => up.UserId == userId)
            .ToListAsync();
        
        context.UserPermissions.RemoveRange(existingPermissions);
        
        IEnumerable<UserPermission> newPermissions = permissions.Select(p => new UserPermission
        {
            UserId = userId,
            Permission = p
        });
        
        context.UserPermissions.AddRange(newPermissions);
        await context.SaveChangesAsync();
    }
}
