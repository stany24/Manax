using AutoMapper;
using ManaxApi.Auth;
using ManaxApi.DTOs;
using ManaxApi.Models.Chapter;
using ManaxApi.Models.Library;
using ManaxApi.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Controllers;

[Route("api/[controller]")]
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
            
        if (library == null) return NotFound();
        
        return mapper.Map<LibraryDTO>(library);
    }

    // GET: api/library/{id}/series
    [HttpGet("{id:long}/series")]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<long>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<long>>> GetLibrarySeries(long id)
    {
        Library? library = await context.Libraries
            .AsNoTracking()
            .Where(l => l.Id == id)
            .Include(library => library.Series)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (library == null) return NotFound();

        return library.Series.Select(s => s.Id).ToList();
    }

    // PUT: api/Library/5
    [HttpPut("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PutLibrary(long id, LibraryUpdateDTO libraryDTO)
    {
        Library? library = await context.Libraries.FindAsync(id);
        
        if (library == null) return NotFound();
        
        mapper.Map(libraryDTO, library);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!context.Libraries.Any(e => e.Id == id)) return NotFound();
            throw;
        }

        return NoContent();
    }

    // POST: api/Library
    [HttpPost("create")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(long))]
    public async Task<ActionResult<long>> PostLibrary(LibraryCreateDTO libraryCreateDTO)
    {
        Library? library = mapper.Map<Library>(libraryCreateDTO);
        
        context.Libraries.Add(library);
        await context.SaveChangesAsync();

        return library.Id;
    }

    // DELETE: api/Library/5
    [HttpDelete("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteLibrary(long id)
    {
        Library? library = await context.Libraries.FindAsync(id);
        if (library == null) return NotFound();

        context.Libraries.Remove(library);
        await context.SaveChangesAsync();

        return NoContent();
    }
}