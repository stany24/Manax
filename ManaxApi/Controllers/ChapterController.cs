using ManaxApi.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ManaxApi.Models.Chapter;
using ManaxApi.Models.User;

namespace ManaxApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChapterController(ChapterContext context) : ControllerBase
    {
        // GET: api/Chapter
        [HttpGet("/api/chapters")]
        [AuthorizeRole(UserRole.User)]
        public async Task<ActionResult<IEnumerable<long>>> GetChapters()
        {
            return await context.Chapters.Select(chapter => chapter.Id).ToListAsync();
        }

        // GET: api/Chapter/5
        [HttpGet("{id:long}")]
        [AuthorizeRole(UserRole.User)]
        public async Task<ActionResult<Chapter>> GetChapter(long id)
        {
            Chapter? serie = await context.Chapters.FindAsync(id);

            if (serie == null)
            {
                return NotFound();
            }

            return serie;
        }

        // PUT: api/Chapter/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id:long}")]
        [AuthorizeRole(UserRole.Admin)]
        public async Task<IActionResult> PutChapter(long id, Chapter serie)
        {
            if (id != serie.Id)
            {
                return BadRequest();
            }

            context.Entry(serie).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChapterExists(id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        // POST: api/Chapter
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [AuthorizeRole(UserRole.Admin)]
        public async Task<ActionResult<Chapter>> PostChapter(Chapter serie)
        {
            context.Chapters.Add(serie);
            await context.SaveChangesAsync();

            return CreatedAtAction("GetChapter", new { id = serie.Id }, serie);
        }

        // DELETE: api/Chapter/5
        [HttpDelete("{id:long}")]
        [AuthorizeRole(UserRole.Admin)]
        public async Task<IActionResult> DeleteChapter(long id)
        {
            Chapter? serie = await context.Chapters.FindAsync(id);
            if (serie == null)
            {
                return NotFound();
            }

            context.Chapters.Remove(serie);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool ChapterExists(long id)
        {
            return context.Chapters.Any(e => e.Id == id);
        }
    }
}
