using ManaxLibrary.DTO.Person;
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
public class PersonController(ManaxContext context, IMapper mapper, INotificationService notificationService)
    : ControllerBase
{
    // GET: api/people
    [HttpGet("/api/people")]
    [RequirePermission(Permission.ReadPeople)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PersonDto>>> GetPeople()
    {
        return await context.People.Select(person => mapper.Map<PersonDto>(person)).ToListAsync();
    }
    
    // DELETE: api/person/5
    [HttpDelete("{id:long}")]
    [RequirePermission(Permission.DeletePeople)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePerson(long id)
    {
        Person? person = await context.People.FindAsync(id);
        if (person == null) return NotFound(Localizer.PersonNotFound(id));
        context.People.Remove(person);
        await context.SaveChangesAsync();
        notificationService.NotifyPersonDeletedAsync(id);
        return Ok();
    }
    
    // POST: api/person
    [HttpPost]
    [RequirePermission(Permission.WritePeople)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<PersonDto>> CreatePerson(PersonCreateDto personCreateDto)
    {
        Person person = mapper.Map<Person>(personCreateDto);
        context.People.Add(person);
        await context.SaveChangesAsync();
        PersonDto personDto = mapper.Map<PersonDto>(person);
        notificationService.NotifyPersonCreatedAsync(personDto);
        return Ok();
    }
    
    // PUT: api/person/5
    [HttpPut("{id:long}")]
    [RequirePermission(Permission.WritePeople)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePerson(long id, PersonUpdateDto personUpdateDto)
    {
        Person? person = await context.People.FindAsync(id);
        if (person == null) return NotFound(Localizer.PersonNotFound(id));
        mapper.Map(personUpdateDto, person);
        await context.SaveChangesAsync();
        PersonDto personDto = mapper.Map<PersonDto>(person);
        notificationService.NotifyPersonUpdatedAsync(personDto);
        return Ok();
    }
}