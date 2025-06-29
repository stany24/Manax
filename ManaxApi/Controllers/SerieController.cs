using AutoMapper;
using ManaxApi.Auth;
using ManaxApi.Models;
using ManaxApi.Models.Serie;
using ManaxLibrary.DTOs;
using ManaxLibrary.DTOs.Serie;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SerieController(ManaxContext context, IMapper mapper) : ControllerBase
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SerieDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SerieDTO>> GetSerie(long id)
    {
        Serie? serie = await context.Series
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);
            
        if (serie == null) return NotFound();
        
        return mapper.Map<SerieDTO>(serie);
    }

    // GET: api/series/{id}/chapters
    [HttpGet("/api/series/{id:long}/chapters")]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<long>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IEnumerable<long> GetSerieChapters(long id)
    {
        IEnumerable<long> chaptersIds = context.Chapters
            .Where(c => c.SerieId == id)
            .OrderBy(c => c.FileName)
            .Select(c => c.Id);

        return chaptersIds;
    }
    
    // GET: api/serie/{id}/poster
    [HttpGet("{id:long}/poster")]
    [AuthorizeRole(UserRole.User)]
    [Produces("image/webp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChapterPage(long id)
    {
        Serie? serie = await context.Series.FindAsync(id);
        if (serie == null) return NotFound();
        string posterPath = Path.Combine(serie.Path,"poster.webp");
        if (!System.IO.File.Exists(posterPath)) return NotFound();
        byte[] readAllBytes = await System.IO.File.ReadAllBytesAsync(posterPath);
        return File(readAllBytes, "image/webp", "poster.webp");
    }

    // PUT: api/Serie/5
    [HttpPut("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PutSerie(long id, SerieUpdateDTO serieUpdate)
    {
        Serie? serie = await context.Series.FindAsync(id);
        
        if (serie == null) return NotFound();
        
        mapper.Map(serieUpdate, serie);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!SerieExists(id)) return NotFound();
            throw;
        }

        return NoContent();
    }

    // POST: api/Serie
    [HttpPost]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(long))]
    public async Task<ActionResult<long>> PostSerie(SerieCreateDTO serieCreate)
    {
        Serie? serie = mapper.Map<Serie>(serieCreate);
        
        context.Series.Add(serie);
        await context.SaveChangesAsync();

        return serie.Id;
    }

    // DELETE: api/Serie/5
    [HttpDelete("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSerie(long id)
    {
        Serie? serie = await context.Series.FindAsync(id);
        if (serie == null) return NotFound();

        context.Series.Remove(serie);
        await context.SaveChangesAsync();

        return NoContent();
    }

    private bool SerieExists(long id)
    {
        return context.Series.Any(e => e.Id == id);
    }
}