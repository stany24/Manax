using AutoMapper;
using ManaxLibrary.DTOs.Library;
using ManaxLibrary.DTOs.User;
using ManaxServer.Auth;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.Library;
using ManaxServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/library")]
[ApiController]
public class LibraryController(ManaxContext context, IMapper mapper) : ControllerBase
{
    // GET: api/Library
    [HttpGet("/api/Libraries")]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<long>))]
    public async Task<ActionResult<IEnumerable<long>>> GetLibraries()
    {
        return await context.Libraries.Select(t => t.Id).ToListAsync();
    }

    // GET: api/library/{id}
    [HttpGet("{id:long}")]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LibraryDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LibraryDTO>> GetLibrary(long id)
    {
        Library? library = await context.Libraries
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);

        if (library == null) return NotFound(Localizer.Format("LibraryNotFound", id));

        return mapper.Map<LibraryDTO>(library);
    }

    // PUT: api/Library/5
    [HttpPut("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PutLibrary(long id, LibraryUpdateDTO libraryUpdate)
    {
        Library? library = await context.Libraries.FindAsync(id);

        if (library == null) return NotFound(Localizer.Format("LibraryNotFound", id));

        if (!Directory.Exists(libraryUpdate.Path))
            return BadRequest(Localizer.Format("LibraryPathNotExist", libraryUpdate.Path));

        // Check if name is unique (except for the current library)
        if (await context.Libraries.AnyAsync(l => l.Name == libraryUpdate.Name && l.Id != id))
            return Conflict(Localizer.Format("LibraryNameExists", libraryUpdate.Name));

        // Check if path is unique (except for the current library)
        if (await context.Libraries.AnyAsync(l => l.Path == libraryUpdate.Path && l.Id != id))
            return Conflict(Localizer.Format("LibraryPathExists", libraryUpdate.Path));

        mapper.Map(libraryUpdate, library);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!context.Libraries.Any(e => e.Id == id)) return NotFound(Localizer.Format("LibraryAlreadyCreated"));
            throw;
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("constraint") ?? false)
        {
            return Conflict(Localizer.Format("LibraryNameOrPathNotUnique"));
        }

        NotificationService.NotifyLibraryCreatedAsync(mapper.Map<LibraryDTO>(library));
        return NoContent();
    }

    // POST: api/Library
    [HttpPost("create")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(long))]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<long>> PostLibrary(LibraryCreateDTO libraryCreate)
    {
        if (!libraryCreate.Path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            libraryCreate.Path += Path.DirectorySeparatorChar;
        if (!Directory.Exists(libraryCreate.Path))
            return BadRequest(Localizer.Format("LibraryPathNotExist", libraryCreate.Path));

        // Check if name is unique
        if (await context.Libraries.AnyAsync(l => l.Name == libraryCreate.Name))
            return Conflict(Localizer.Format("LibraryNameExists", libraryCreate.Name));

        // Check if path is unique
        if (await context.Libraries.AnyAsync(l => l.Path == libraryCreate.Path))
            return Conflict(Localizer.Format("LibraryPathExists", libraryCreate.Path));

        Library? library = mapper.Map<Library>(libraryCreate);

        context.Libraries.Add(library);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("constraint") ?? false)
        {
            return Conflict(Localizer.Format("LibraryNameOrPathNotUnique"));
        }

        NotificationService.NotifyLibraryCreatedAsync(mapper.Map<LibraryDTO>(library));
        return library.Id;
    }

    // DELETE: api/Library/5
    [HttpDelete("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLibrary(long id)
    {
        Library? library = await context.Libraries.FindAsync(id);
        if (library == null) return NotFound(Localizer.Format("LibraryNotFound", id));

        context.Libraries.Remove(library);
        await context.SaveChangesAsync();
        NotificationService.NotifyLibraryDeletedAsync(library.Id);

        return Ok();
    }
}