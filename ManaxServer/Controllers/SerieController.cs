using System.Text.RegularExpressions;
using AutoMapper;
using ManaxLibrary.DTOs.Search;
using ManaxLibrary.DTOs.Serie;
using ManaxLibrary.DTOs.User;
using ManaxServer.Auth;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.Library;
using ManaxServer.Models.Serie;
using ManaxServer.Services;
using ManaxServer.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/serie")]
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
            .FirstOrDefaultAsync(l => l.Id == id);

        if (serie == null) return NotFound(Localizer.Format("SerieNotFound", id));

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
    public async Task<IActionResult> GetPoster(long id)
    {
        Serie? serie = await context.Series.FindAsync(id);
        if (serie == null) return NotFound();
        string posterName = SettingsManager.Data.PosterName + "." + SettingsManager.Data.ImageFormat.ToString().ToLower();
        string posterPath = Path.Combine(serie.Path, posterName);
        if (!System.IO.File.Exists(posterPath)) return NotFound(Localizer.Format("PosterNotFound", id));
        byte[] readAllBytes = await System.IO.File.ReadAllBytesAsync(posterPath);
        return File(readAllBytes, "image/webp", posterName);
    }

    // PUT: api/Serie/5
    [HttpPut("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PutSerie(long id, SerieUpdateDTO serieUpdate)
    {
        Serie? serie = await context.Series.FindAsync(id);

        if (serie == null) return NotFound(Localizer.Format("SerieNotFound", id));

        mapper.Map(serieUpdate, serie);
        serie.LastModification = DateTime.UtcNow;

        try
        {
            await context.SaveChangesAsync();
            NotificationService.NotifySerieUpdatedAsync(mapper.Map<SerieDTO>(serie));
        }
        catch (DbUpdateConcurrencyException)
        {
            BadRequest("Failed to update serie with ID " + id);
        }

        return Ok();
    }

    // POST: api/Serie
    [HttpPost]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(long))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<long>> PostSerie(SerieCreateDTO serieCreate)
    {
        Library? library = context.Libraries.FirstOrDefault(l => l.Id == serieCreate.LibraryId);
        if (library == null)
            return BadRequest(Localizer.Format("LibraryNotFound", serieCreate.LibraryId));

        try
        {
            string folderPath = library.Path + serieCreate.Title;
            if (System.IO.File.Exists(folderPath)) return BadRequest(Localizer.Format("SerieAlreadyExists"));
            Directory.CreateDirectory(folderPath);
            Serie serie = new()
            {
                Title = serieCreate.Title,
                FolderName = serieCreate.Title,
                LibraryId = serieCreate.LibraryId,
                Path = folderPath,
                Description = "",
                Status = Status.Ongoing,
                Creation = DateTime.UtcNow,
                LastModification = DateTime.UtcNow
            };
            context.Series.Add(serie);
            await context.SaveChangesAsync();
            NotificationService.NotifySerieCreatedAsync(mapper.Map<SerieDTO>(serie));
            return serie.Id;
        }
        catch (Exception)
        {
            return BadRequest(Localizer.Format("InvalidZipFile"));
        }
    }

    // DELETE: api/Serie/5
    [HttpDelete("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSerie(long id)
    {
        Serie? serie = await context.Series.FindAsync(id);
        if (serie == null) return NotFound(Localizer.Format("SerieNotFound", id));

        context.Series.Remove(serie);
        await context.SaveChangesAsync();
        NotificationService.NotifySerieDeletedAsync(serie.Id);

        return NoContent();
    }
    
    [HttpPost("search")]
    [AuthorizeRole(UserRole.User)]
    public List<long> Search(Search search)
    {
        Regex regex = new(search.RegexSearch, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
        Dictionary<long, int> seriesWithChapterCount = context.Chapters
            .GroupBy(c => c.SerieId)
            .Select(g => new { SerieId = g.Key, ChapterCount = g.Count() })
            .ToDictionary(x => x.SerieId, x => x.ChapterCount);
        
        List<Serie> series = context.Series
            .Where(s => search.IncludedLibraries.Contains(s.LibraryId))
            .Where(s => !search.ExcludedLibraries.Contains(s.LibraryId))
            .Where(s => search.IncludedStatuses.Contains(s.Status))
            .Where(s => !search.ExcludedStatuses.Contains(s.Status))
            .ToList();
        
        return series
            .Where(s => regex.Match(s.Title).Success || regex.Match(s.Description).Success)
            .Where(s => {
                int chapterCount = seriesWithChapterCount.TryGetValue(s.Id, out int value) ? value : 0;
                return chapterCount >= search.MinChapters && chapterCount <= search.MaxChapters;
            })
            .Select(s => s.Id)
            .ToList();
    }
}