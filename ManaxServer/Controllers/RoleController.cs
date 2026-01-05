using ManaxLibrary;
using ManaxLibrary.DTO.Role;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Models;
using ManaxServer.Models.Person;
using ManaxServer.Services.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/role")]
[ApiController]
public class RoleController(ManaxContext context, INotificationService notificationService)
    : ControllerBase
{
    [HttpGet("/api/roles")]
    [RequirePermission(Permission.ReadPersons)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
    {
        List<RoleDto> roles = await context.Roles
            .Select(role => role.ToDto())
            .ToListAsync();
        return Ok(roles);
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(Permission.DeleteRoles)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteRole(long id)
    {
        Role? role = await context.Roles.FindAsync(id);
        if (role == null) return NotFound(ErrorCode.RoleDoesNotExist);
        context.Roles.Remove(role);
        await context.SaveChangesAsync();
        notificationService.NotifyRoleDeletedAsync(id);
        return Ok();
    }

    [HttpPost]
    [RequirePermission(Permission.WriteRoles)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult> CreateRole(RoleCreateDto roleCreateDto)
    {
        Role role = Role.Create(roleCreateDto);
        context.Roles.Add(role);
        await context.SaveChangesAsync();
        notificationService.NotifyRoleCreatedAsync(role.ToDto());
        return Ok(role.Id);
    }

    [HttpPut("{id:long}")]
    [RequirePermission(Permission.WriteRoles)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateRole(long id, RoleUpdateDto roleUpdateDto)
    {
        Role? role = await context.Roles.FindAsync(id);
        if (role == null) return NotFound(ErrorCode.RoleDoesNotExist);
        role.Update(roleUpdateDto);
        await context.SaveChangesAsync();
        notificationService.NotifyRoleUpdatedAsync(role.ToDto());
        return Ok();
    }
}