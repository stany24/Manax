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
    public async Task<ActionResult<IEnumerable<long>>> GetLibraries()
    {
        return await context.Libraries.Select(t => t.Id).ToListAsync();
    }

    // GET: api/library/{id}
    [HttpGet("{id:long}")]
    [AuthorizeRole(UserRole.User)]
    public async Task<ActionResult<LibraryInfo>> GetLibrary(long id)
    {
        Library? library = await context.Libraries
            .AsNoTracking()
            .Where(l => l.Id == id)
            .Include(library => library.Infos)
            .FirstOrDefaultAsync();
        if (library == null) return NotFound();
        return library.Infos;
    }

    // GET: api/library/{id}/series
    [HttpGet("{id:long}/series")]
    [AuthorizeRole(UserRole.User)]
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
            if (!LibraryExists(id)) return NotFound();

            throw;
        }

        return NoContent();
    }

    // POST: api/Library
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost("create")]
    [AuthorizeRole(UserRole.Admin)]
    public async Task<ActionResult<Library>> PostLibrary(Library library)
    {
        context.Libraries.Add(library);
        await context.SaveChangesAsync();

        return CreatedAtAction("GetLibrary", new { id = library.Id }, library);
    }

    // DELETE: api/Library/5
    [HttpDelete("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    public async Task<IActionResult> DeleteLibrary(long id)
    {
        Library? library = await context.Libraries.FindAsync(id);
        if (library == null) return NotFound();

        context.Libraries.Remove(library);
        await context.SaveChangesAsync();

        return NoContent();
    }

    private bool LibraryExists(long id)
    {
        return context.Libraries.Any(e => e.Id == id);
    }
}