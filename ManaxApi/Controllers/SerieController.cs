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
        public async Task<ActionResult<IEnumerable<Serie>>> GetSeries()
        {
            return await context.Series.ToListAsync();
        }

        // GET: api/Serie/5
        [HttpGet("{id:long}")]
        [AuthorizeRole(UserRole.User)]
        public async Task<ActionResult<Serie>> GetSerie(long id)
        {
            Serie? serie = await context.Series.FindAsync(id);

            if (serie == null)
            {
                return NotFound();
            }

            return serie;
        }

        // PUT: api/Serie/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id:long}")]
        [AuthorizeRole(UserRole.Admin)]
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
        public async Task<ActionResult<Serie>> PostSerie(Serie serie)
        {
            context.Series.Add(serie);
            await context.SaveChangesAsync();

            return CreatedAtAction("GetSerie", new { id = serie.Id }, serie);
        }

        // DELETE: api/Serie/5
        [HttpDelete("{id:long}")]
        [AuthorizeRole(UserRole.Admin)]
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
