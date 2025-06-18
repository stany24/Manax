using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ManaxApi.Models.Chapter;

namespace ManaxApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChapterController(ChapterContext context) : ControllerBase
    {
        // GET: api/Chapter
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Chapter>>> GetChapters()
        {
            return await context.Chapters.ToListAsync();
        }

        // GET: api/Chapter/5
        [HttpGet("{id:long}")]
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
        public async Task<ActionResult<Chapter>> PostChapter(Chapter serie)
        {
            context.Chapters.Add(serie);
            await context.SaveChangesAsync();

            return CreatedAtAction("GetChapter", new { id = serie.Id }, serie);
        }

        // DELETE: api/Chapter/5
        [HttpDelete("{id:long}")]
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
