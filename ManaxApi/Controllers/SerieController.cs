using ManaxApi.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ManaxApi.Models.Serie;
using ManaxApi.Models.User;

namespace ManaxApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SerieController(SerieContext context) : ControllerBase
    {
        // GET: api/Serie
        [HttpGet("/api/series")]
        [AuthorizeRole(UserRole.User)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<long>))]
        public async Task<ActionResult<IEnumerable<long>>> GetSeries()
        {
            return await context.Series.Select(serie => serie.Id).ToListAsync();
        }

        // GET: api/serie/{id}
        [HttpGet("{id:long}")]
        [AuthorizeRole(UserRole.User)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SerieInfo))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SerieInfo>> GetSerie(long id)
        {
            SerieInfo? infos = await context.Series
                .AsNoTracking()
                .Where(l => l.Id == id)
                .Select(s => s.GetInfo())
                .FirstOrDefaultAsync();
            if (infos == null) return NotFound();
            return infos;
        }

        // GET: api/series/{id}/chapters
        [HttpGet("/api/series/{id:long}/chapters")]
        [AuthorizeRole(UserRole.User)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<long>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<long>>> GetSerieChapters(long id)
        {
            Serie? serie = await context.Series
                .Include(s => s.Chapters)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (serie == null)
            {
                return NotFound();
            }
            return serie.Chapters.Select(c => c.Id).ToList();
        }

        // PUT: api/Serie/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id:long}")]
        [AuthorizeRole(UserRole.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PutSerie(long id, Serie serie)
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
                if (!SerieExists(id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        // POST: api/Serie
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [AuthorizeRole(UserRole.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Serie))]
        public async Task<ActionResult<Serie>> PostSerie(Serie serie)
        {
            context.Series.Add(serie);
            await context.SaveChangesAsync();

            return CreatedAtAction("GetSerie", new { id = serie.Id }, serie);
        }

        // DELETE: api/Serie/5
        [HttpDelete("{id:long}")]
        [AuthorizeRole(UserRole.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteSerie(long id)
        {
            Serie? serie = await context.Series.FindAsync(id);
            if (serie == null)
            {
                return NotFound();
            }

            context.Series.Remove(serie);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool SerieExists(long id)
        {
            return context.Series.Any(e => e.Id == id);
        }
    }
}

