using ManaxLibrary.DTO.Library;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.Library;
using ManaxServer.Models.Serie;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/library")]
[ApiController]
public class LibraryController(ManaxContext context, IMapper mapper, INotificationService notificationService)
    : ControllerBase
{
    // GET: api/Library
    [HttpGet("/api/libraries")]
    [RequirePermission(Permission.ReadLibraries)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<long>>> GetLibraries()
    {
        return await context.Libraries.Select(t => t.Id).ToListAsync();
    }

    // GET: api/library/{id}
    [HttpGet("{id:long}")]
    [RequirePermission(Permission.ReadLibraries)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LibraryDto>> GetLibrary(long id)
    {
        Library? library = await context.Libraries
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);

        if (library == null) return NotFound(Localizer.LibraryNotFound(id));

        return mapper.Map<LibraryDto>(library);
    }

    // PUT: api/Library/5
    [HttpPut("{id:long}")]
    [RequirePermission(Permission.WriteLibraries)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PutLibrary(long id, LibraryUpdateDto libraryUpdate)
    {
        Library? library = await context.Libraries.FindAsync(id);

        if (library == null) return NotFound(Localizer.LibraryNotFound(id));

        if (string.IsNullOrWhiteSpace(libraryUpdate.Name))
            return BadRequest(Localizer.LibraryNameRequired());
        if (await context.Libraries.AnyAsync(l => l.Name == libraryUpdate.Name && l.Id != id))
            return Conflict(Localizer.LibraryNameExists(libraryUpdate.Name));

        mapper.Map(libraryUpdate, library);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!context.Libraries.Any(e => e.Id == id)) return NotFound(Localizer.LibraryAlreadyCreated());
            throw;
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("constraint") ?? false)
        {
            return Conflict(Localizer.LibraryNameOrPathNotUnique());
        }

        notificationService.NotifyLibraryUpdatedAsync(mapper.Map<LibraryDto>(library));
        return Ok();
    }

    // POST: api/Library
    [HttpPost("create")]
    [RequirePermission(Permission.WriteLibraries)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<long>> PostLibrary(LibraryCreateDto libraryCreate)
    {
        if (string.IsNullOrWhiteSpace(libraryCreate.Name))
            return BadRequest(Localizer.LibraryNameRequired());
        if (await context.Libraries.AnyAsync(l => l.Name == libraryCreate.Name))
            return Conflict(Localizer.LibraryNameExists(libraryCreate.Name));

        Library library = mapper.Map<Library>(libraryCreate);
        library.Creation = DateTime.UtcNow;

        context.Libraries.Add(library);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("constraint") ?? false)
        {
            return Conflict(Localizer.LibraryNameExists(libraryCreate.Name));
        }

        notificationService.NotifyLibraryCreatedAsync(mapper.Map<LibraryDto>(library));
        return library.Id;
    }

    // DELETE: api/Library/5
    [HttpDelete("{id:long}")]
    [RequirePermission(Permission.DeleteLibraries)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLibrary(long id)
    {
        Library? library = await context.Libraries.FindAsync(id);
        if (library == null) return NotFound(Localizer.LibraryNotFound(id));

        List<Serie> seriesToUpdate = await context.Series
            .Where(s => s.LibraryId == id)
            .ToListAsync();

        foreach (Serie serie in seriesToUpdate) serie.LibraryId = null;

        context.Libraries.Remove(library);
        await context.SaveChangesAsync();
        notificationService.NotifyLibraryDeletedAsync(library.Id);

        return Ok();
    }
}