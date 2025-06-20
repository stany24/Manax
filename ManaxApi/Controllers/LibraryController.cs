using ManaxApi.Auth;
using ManaxApi.Models.Library;
using ManaxApi.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LibraryController(LibraryContext context) : ControllerBase
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LibraryInfo))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LibraryInfo>> GetLibrary(long id)
    {
        LibraryInfo? infos = await context.Libraries
            .AsNoTracking()
            .Where(l => l.Id == id)
            .Select(library => library.GetInfo() )
            .FirstOrDefaultAsync();
        if (infos == null) return NotFound();
        return infos;
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
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PutLibrary(long id, Library library)
    {
        if (id != library.Id) return BadRequest();

        context.Entry(library).State = EntityState.Modified;

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
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost("create")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Library))]
    public async Task<ActionResult<Library>> PostLibrary(Library library)
    {
        context.Libraries.Add(library);
        await context.SaveChangesAsync();

        return CreatedAtAction("GetLibrary", new { id = library.Id }, library);
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