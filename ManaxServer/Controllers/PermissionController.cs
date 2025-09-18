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
    [HttpPost("{userId:long}")]
    [Authorize("admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<long>> SetPermissions(long userId, List<Permission> permissions)
    {
        await permissionService.SetUserPermissionsAsync(userId, permissions);
        return Ok();
    }
    
    // GET: api/Permission/self
    [HttpGet("self")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Permission>>> GetMyPermissions()
    {
        long? currentUserId = UserController.GetCurrentUserId(HttpContext);
        if (currentUserId == null) return Unauthorized();
        IEnumerable<Permission> permissions = await permissionService.GetUserPermissionsAsync((long)currentUserId);
        return Ok(permissions);
    }

    public static Permission[] GetDefaultPermissionsForRole(UserRole role)
    {
        Permission[] user =
        [
            Permission.ReadSeries,
            Permission.ReadChapters,
            Permission.ReadSelfStats,
            Permission.ReadLibraries,
            Permission.ReadRanks,
            Permission.ReadTags,

            Permission.WriteIssues,
            Permission.SetMyRank,
            Permission.MarkChapterAsRead
        ];

        Permission[] admin = user.Concat([
            Permission.ReadAllIssues,
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
            Permission.WriteLibraries,
            Permission.WriteTags,

            Permission.DeleteTags,
            Permission.DeleteSeries,
            Permission.DeleteChapters,
            Permission.DeleteLibraries,
            Permission.DeleteUsers,
            Permission.ResetPasswords
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