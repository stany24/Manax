using ManaxLibrary.DTO.Role;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.Person;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/role")]
[ApiController]
public class RoleController(ManaxContext context, IMapper mapper, INotificationService notificationService)
    : ControllerBase
{
    // GET: api/roles
    [HttpGet("/api/roles")]
    [RequirePermission(Permission.ReadPeople)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
    {
        return await context.Roles.Select(role => mapper.Map<RoleDto>(role)).ToListAsync();
    }
    
    // DELETE: api/role/5
    [HttpDelete("{id:long}")]
    [RequirePermission(Permission.DeleteRoles)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRole(long id)
    {
        Role? role = await context.Roles.FindAsync(id);
        if (role == null) return NotFound(Localizer.RoleNotFound(id));
        context.Roles.Remove(role);
        await context.SaveChangesAsync();
        notificationService.NotifyRoleDeletedAsync(id);
        return Ok();
    }
    
    // POST: api/role
    [HttpPost]
    [RequirePermission(Permission.WriteRoles)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<RoleDto>> CreateRole(RoleCreateDto roleCreateDto)
    {
        Role role = mapper.Map<Role>(roleCreateDto);
        context.Roles.Add(role);
        await context.SaveChangesAsync();
        RoleDto roleDto = mapper.Map<RoleDto>(role);
        notificationService.NotifyRoleCreatedAsync(roleDto);
        return Ok();
    }
    
    // PUT: api/role/5
    [HttpPut("{id:long}")]
    [RequirePermission(Permission.WriteRoles)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole(long id, RoleUpdateDto roleUpdateDto)
    {
        Role? role = await context.Roles.FindAsync(id);
        if (role == null) return NotFound(Localizer.RoleNotFound(id));
        mapper.Map(roleUpdateDto, role);
        await context.SaveChangesAsync();
        RoleDto roleDto = mapper.Map<RoleDto>(role);
        notificationService.NotifyRoleUpdatedAsync(roleDto);
        return Ok();
    }
}