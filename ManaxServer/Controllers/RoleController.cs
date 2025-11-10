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

[Route("api/person")]
[ApiController]
public class RoleController(ManaxContext context, IMapper mapper, INotificationService notificationService)
    : ControllerBase
{
    // GET: api/people
    [HttpGet("/api/people")]
    [RequirePermission(Permission.ReadPeople)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetRole()
    {
        return await context.Roles.Select(person => mapper.Map<RoleDto>(person)).ToListAsync();
    }
    
    // DELETE: api/person/5
    [HttpDelete("{id:long}")]
    [RequirePermission(Permission.DeleteRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRole(long id)
    {
        Role? person = await context.Roles.FindAsync(id);
        if (person == null) return NotFound(Localizer.RoleNotFound(id));
        context.Roles.Remove(person);
        await context.SaveChangesAsync();
        notificationService.NotifyRoleDeletedAsync(id);
        return Ok();
    }
    
    // POST: api/person
    [HttpPost]
    [RequirePermission(Permission.WriteRole)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<RoleDto>> CreateRole(RoleCreateDto personCreateDto)
    {
        Role person = mapper.Map<Role>(personCreateDto);
        context.Roles.Add(person);
        await context.SaveChangesAsync();
        RoleDto personDto = mapper.Map<RoleDto>(person);
        notificationService.NotifyRoleCreatedAsync(personDto);
        return Ok();
    }
    
    // PUT: api/person/5
    [HttpPut("{id:long}")]
    [RequirePermission(Permission.WriteRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole(long id, RoleUpdateDto personUpdateDto)
    {
        Role? person = await context.Roles.FindAsync(id);
        if (person == null) return NotFound(Localizer.RoleNotFound(id));
        mapper.Map(personUpdateDto, person);
        await context.SaveChangesAsync();
        RoleDto personDto = mapper.Map<RoleDto>(person);
        notificationService.NotifyRoleUpdatedAsync(personDto);
        return Ok();
    }
}