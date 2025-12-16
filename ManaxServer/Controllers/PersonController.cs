using ManaxLibrary;
using ManaxLibrary.DTO.Person;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Models;
using ManaxServer.Models.Person;
using ManaxServer.Services.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/person")]
[ApiController]
public class PersonController(ManaxContext context, INotificationService notificationService)
    : ControllerBase
{
    [HttpGet("/api/persons")]
    [RequirePermission(Permission.ReadPersons)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PersonDto>>> GetPersons()
    {
        return await context.Persons.Include(p => p.Role).Select(person => person.ToDto()).ToListAsync();
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(Permission.DeletePersons)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePerson(long id)
    {
        Person? person = await context.Persons.FindAsync(id);
        if (person == null) return NotFound(ErrorCode.PersonDoesNotExist);
        context.Persons.Remove(person);
        await context.SaveChangesAsync();
        notificationService.NotifyPersonDeletedAsync(id);
        return Ok();
    }

    [HttpPost]
    [RequirePermission(Permission.WritePersons)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<PersonDto>> CreatePerson(PersonCreateDto personCreateDto)
    {
        Role? role = await context.Roles.FindAsync(personCreateDto.RoleId);
        if (role == null) return BadRequest(ErrorCode.RoleDoesNotExist);

        Person person = Person.Create(personCreateDto, context);
        person.Role = role;
        context.Persons.Add(person);
        await context.SaveChangesAsync();
        notificationService.NotifyPersonCreatedAsync(person.ToDto());
        return Ok();
    }

    [HttpPut("{id:long}")]
    [RequirePermission(Permission.WritePersons)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePerson(long id, PersonUpdateDto personUpdateDto)
    {
        Person? person = await context.Persons.FindAsync(id);
        if (person == null) return NotFound(ErrorCode.PersonDoesNotExist);

        Role? role = await context.Roles.FindAsync(personUpdateDto.RoleId);
        if (role == null) return BadRequest(ErrorCode.RoleDoesNotExist);

        person.Update(personUpdateDto, role);
        await context.SaveChangesAsync();
        notificationService.NotifyPersonUpdatedAsync(person.ToDto());
        return Ok();
    }
}