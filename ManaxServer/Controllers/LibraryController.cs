using ManaxLibrary;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Models;
using ManaxServer.Models.Library;
using ManaxServer.Models.Serie;
using ManaxServer.Services.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/library")]
[ApiController]
public class LibraryController(ManaxContext context, INotificationService notificationService)
    : ControllerBase
{
    [HttpGet("/api/libraries")]
    [RequirePermission(Permission.ReadLibraries)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<long>>> GetLibraries()
    {
        List<long> libraries = await context.Libraries
            .Select(t => t.Id).ToListAsync();
        return Ok(libraries);
    }

    [HttpGet("{id:long}")]
    [RequirePermission(Permission.ReadLibraries)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LibraryDto>> GetLibrary(long id)
    {
        Library? library = await context.Libraries
            .FirstOrDefaultAsync(l => l.Id == id);

        if (library == null) return NotFound(ErrorCode.LibraryDoesNotExist);

        return Ok(library.ToDto());
    }

    [HttpPut("{id:long}")]
    [RequirePermission(Permission.WriteLibraries)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> PutLibrary(long id, LibraryUpdateDto libraryUpdate)
    {
        if(!libraryUpdate.IsValid()) return BadRequest(ErrorCode.InvalidLibraryData);
        Library? library = await context.Libraries.FindAsync(id);
        if (library == null) return NotFound(ErrorCode.LibraryDoesNotExist);
        library.Update(libraryUpdate);

        try { await context.SaveChangesAsync(); }
        catch { return Conflict(ErrorCode.InvalidLibraryData); }

        notificationService.NotifyLibraryUpdatedAsync(library.ToDto());
        return Ok();
    }

    [HttpPost("create")]
    [RequirePermission(Permission.WriteLibraries)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<long>> PostLibrary(LibraryCreateDto libraryCreate)
    {
        if (!libraryCreate.IsValid()) { return BadRequest(ErrorCode.InvalidLibraryData);}
        Library library = Library.Create(libraryCreate);
        library.Creation = DateTime.UtcNow;

        context.Libraries.Add(library);

        try { await context.SaveChangesAsync(); }
        catch { return Conflict(ErrorCode.InvalidLibraryData); }

        notificationService.NotifyLibraryCreatedAsync(library.ToDto());
        return Ok(library.Id);
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(Permission.DeleteLibraries)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteLibrary(long id)
    {
        Library? library = await context.Libraries.FindAsync(id);
        if (library == null) return NotFound(ErrorCode.LibraryDoesNotExist);

        await context.Series
            .Where(s => s.Library != null && s.Library.Id == id)
            .ForEachAsync(s => s.Library = null);

        context.Libraries.Remove(library);
        await context.SaveChangesAsync();
        notificationService.NotifyLibraryDeletedAsync(library.Id);

        return Ok();
    }
}