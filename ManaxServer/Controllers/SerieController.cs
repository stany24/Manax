using System.Globalization;
using System.Text.RegularExpressions;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.DTO.Search;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.SavePoint;
using ManaxServer.Models.Serie;
using ManaxServer.Services.BackgroundTask;
using ManaxServer.Services.Fix;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using ManaxServer.Settings;
using ManaxServer.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/serie")]
[ApiController]
public class SerieController(
    ManaxContext context,
    IMapper mapper,
    INotificationService notificationService,
    IFixService fixService,
    IBackgroundTaskService backgroundTaskService)
    : ControllerBase
{
    // GET: api/Serie
    [HttpGet("/api/series")]
    [RequirePermission(Permission.ReadSeries)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<long>>> GetSeries()
    {
        return await context.Series.Select(serie => serie.Id).ToListAsync();
    }

    // GET: api/serie/{id}
    [HttpGet("{id:long}")]
    [RequirePermission(Permission.ReadSeries)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SerieDto>> GetSerie(long id)
    {
        Serie? serie = await context.Series
            .FirstOrDefaultAsync(l => l.Id == id);

        if (serie == null) return NotFound(Localizer.SerieNotFound(id));

        return mapper.Map<SerieDto>(serie);
    }

    // GET: api/series/{id}/chapters
    [HttpGet("/api/series/{id:long}/chapters")]
    [RequirePermission(Permission.ReadChapters)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<List<long>> GetSerieChapters(long id)
    {
        Serie? serie = context.Series.FirstOrDefault(s => s.Id == id);
        if (serie == null) return NotFound(Localizer.SerieNotFound(id));
        List<long> chaptersIds = context.Chapters
            .Where(c => c.SerieId == id)
            .OrderBy(c => c.FileName)
            .Select(c => c.Id).ToList();

        return chaptersIds;
    }

    // GET: api/series/{id}/chapters
    [HttpGet("/api/series/{id:long}/reads")]
    [RequirePermission(Permission.ReadSavePoints)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<List<ReadDto>> GetSerieReads(long id)
    {
        Serie? serie = context.Series.FirstOrDefault(s => s.Id == id);
        if (serie == null) return NotFound(Localizer.SerieNotFound(id));
        List<ReadDto> reads = context.Reads
            .Where(r => r.Chapter.SerieId == id)
            .Where(r => r.UserId == UserController.GetCurrentUserId(HttpContext))
            .Select(r => mapper.Map<ReadDto>(r))
            .ToList();

        return reads;
    }

    // GET: api/serie/{id}/poster
    [HttpGet("{id:long}/poster")]
    [RequirePermission(Permission.ReadSeries)]
    [Produces("image/webp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPoster(long id)
    {
        Serie? serie = context.Series
            .Include(s => s.SavePoint)
            .FirstOrDefault(s => s.Id == id);
        if (serie == null) return NotFound(Localizer.SerieNotFound(id));
        string posterName = SettingsManager.Data.PosterName + "." +
                            SettingsManager.Data.PosterFormat.ToString().ToLower(CultureInfo.InvariantCulture);
        string posterPath = Path.Combine(serie.SavePath, posterName);
        if (!System.IO.File.Exists(posterPath)) return NotFound(Localizer.PosterNotFound(id));
        byte[] readAllBytes = await System.IO.File.ReadAllBytesAsync(posterPath);
        return File(readAllBytes, "image/webp", posterName);
    }

    // PUT: api/Serie/5
    [HttpPut("{id:long}")]
    [RequirePermission(Permission.WriteSeries)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PutSerie(long id, SerieUpdateDto serieUpdate)
    {
        Serie? serie = await context.Series.FindAsync(id);

        if (serie == null) return NotFound(Localizer.SerieNotFound(id));

        if (string.IsNullOrWhiteSpace(serieUpdate.Title))
            return BadRequest(Localizer.SerieTitleRequired());

        mapper.Map(serieUpdate, serie);
        serie.LastModification = DateTime.UtcNow;

        try
        {
            await context.SaveChangesAsync();
            _ = backgroundTaskService.AddTaskAsync(new FixSerieBackGroundTask(fixService, serie.Id));
            notificationService.NotifySerieUpdatedAsync(mapper.Map<SerieDto>(serie));
        }
        catch (DbUpdateConcurrencyException)
        {
            BadRequest("Failed to update serie with ID " + id);
        }

        return Ok();
    }

    // POST: api/Serie
    [HttpPost]
    [RequirePermission(Permission.WriteSeries)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<long>> PostSerie(SerieCreateDto serieCreate)
    {
        if (string.IsNullOrWhiteSpace(serieCreate.Title))
            return BadRequest(Localizer.SerieTitleRequired());

        SavePoint? savePoint = SelectSavePoint();
        if (savePoint == null) return BadRequest(Localizer.NoSavePoint());
        try
        {
            string folderPath = savePoint.Path + Path.DirectorySeparatorChar + serieCreate.Title;
            if (System.IO.File.Exists(folderPath)) return BadRequest(Localizer.SerieAlreadyExists());
            Directory.CreateDirectory(folderPath);
            Serie serie = new()
            {
                SavePointId = savePoint.Id,
                Title = serieCreate.Title,
                FolderName = serieCreate.Title,
                Description = "",
                Status = Status.Ongoing,
                Creation = DateTime.UtcNow,
                LastModification = DateTime.UtcNow
            };
            context.Series.Add(serie);
            await context.SaveChangesAsync();
            notificationService.NotifySerieCreatedAsync(mapper.Map<SerieDto>(serie));
            _ = backgroundTaskService.AddTaskAsync(new FixSerieBackGroundTask(fixService, serie.Id));
            return serie.Id;
        }
        catch (Exception)
        {
            return BadRequest(Localizer.SerieCreationFailed());
        }
    }

    private SavePoint? SelectSavePoint()
    {
        List<SavePoint> savePoints = context.SavePoints.ToList();
        if (savePoints.Count == 0) return null;
        long min = long.MaxValue;
        SavePoint? selectedSavePoint = null;
        foreach (SavePoint savePoint in savePoints)
        {
            if (!Directory.Exists(savePoint.Path)) continue;
            DirectoryInfo dirInfo = new(savePoint.Path);
            long size = GetDirectorySize(dirInfo);
            size = Math.Max(size, 1);
            DriveInfo driveInfo = new(savePoint.Path);
            long freeSpace = driveInfo.AvailableFreeSpace;
            if (freeSpace / size >= min) continue;
            min = freeSpace / size;
            selectedSavePoint = savePoint;
        }

        return selectedSavePoint;
    }

    private static long GetDirectorySize(DirectoryInfo d)
    {
        FileInfo[] fis = d.GetFiles();
        long size = fis.Sum(fi => fi.Length);
        DirectoryInfo[] dis = d.GetDirectories();
        size += dis.Sum(GetDirectorySize);
        return size;
    }

    // DELETE: api/Serie/5
    [HttpDelete("{id:long}")]
    [RequirePermission(Permission.DeleteSeries)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSerie(long id)
    {
        Serie? serie = await context.Series.FindAsync(id);
        if (serie == null) return NotFound(Localizer.SerieNotFound(id));

        context.Series.Remove(serie);
        await context.SaveChangesAsync();
        notificationService.NotifySerieDeletedAsync(serie.Id);

        return Ok();
    }

    [HttpPost("search")]
    [RequirePermission(Permission.ReadSeries)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public List<long> Search(Search search)
    {
        Regex regex = new(search.RegexSearch, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        Dictionary<long, int> seriesWithChapterCount = context.Chapters
            .GroupBy(c => c.SerieId)
            .Select(g => new { SerieId = g.Key, ChapterCount = g.Count() })
            .ToDictionary(x => x.SerieId, x => x.ChapterCount);

        List<Serie> series = context.Series.ToList();

        if (search.IncludedLibraries.Count > 0)
            series = series.Where(s => search.IncludedLibraries.Contains(s.LibraryId ?? -1)).ToList();
        series = series.Where(s => !search.ExcludedLibraries.Contains(s.LibraryId ?? -1)).ToList();

        if (search.IncludedStatuses.Count > 0)
            series = series.Where(s => search.IncludedStatuses.Contains(s.Status)).ToList();
        series = series.Where(s => !search.ExcludedStatuses.Contains(s.Status)).ToList();

        return series
            .Where(s => regex.Match(s.Title).Success || regex.Match(s.Description).Success)
            .Where(s =>
            {
                int chapterCount = seriesWithChapterCount.TryGetValue(s.Id, out int value) ? value : 0;
                return chapterCount >= search.MinChapters && chapterCount <= search.MaxChapters;
            })
            .Select(s => s.Id)
            .ToList();
    }
}