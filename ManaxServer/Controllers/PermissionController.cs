using ManaxLibrary.DTO.User;
using ManaxServer.Services.Permission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManaxServer.Controllers;

[Route("api/permission")]
[ApiController]
public class PermissionController(IPermissionService permissionService) : ControllerBase
{
    // POST: api/SavePoint
    [HttpPost("{userId}")]
    [Authorize("admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<long>> SetPermissions(long userId, List<Permission> permissions)
    {
        await permissionService.SetUserPermissionsAsync(userId, permissions);
        return Ok();
    }
    
    public static Permission[] GetDefaultPermissionsForRole(UserRole role)
    {
        Permission[] user =
        [
            Permission.ReadSeries,
            Permission.ReadChapters,
            Permission.ReadSelfStats,
            Permission.ReadLibrary,
            Permission.ReadRanks,
            Permission.ReadTags,
            
            Permission.WriteIssues,
            Permission.SetMyRank,
            Permission.MarkChapterAsRead
        ];
        
        Permission[] admin = user.Concat([
            Permission.ReadUsers,
            Permission.ReadServerStats,
            
            Permission.WriteSeries,
            Permission.UploadChapter,
            Permission.WriteIssues,
            Permission.WriteRanks,
            Permission.SetSerieTags,
            
            Permission.DeleteIssues,
            Permission.DeleteRanks
        ]).ToArray();
        
        Permission[] owner = admin.Concat([
            Permission.ReadSavePoints,
            Permission.ReadServerSettings,
            
            Permission.WriteUsers,
            Permission.WriteServerSettings,
            Permission.WriteSavePoints,
            Permission.WriteLibrary,
            Permission.WriteTags,
            
            Permission.DeleteTags,
            Permission.DeleteSeries,
            Permission.DeleteChapters,
            Permission.DeleteLibrary,
            Permission.DeleteUsers
        ]).ToArray();
        
        return role switch
        {
            UserRole.Owner => owner,
            UserRole.Admin => admin,
            UserRole.User => user,
            _ => []
        };
    }
}