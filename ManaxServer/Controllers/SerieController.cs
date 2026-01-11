using System.Text.RegularExpressions;
using ManaxLibrary;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.DTO.Search;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Models;
using ManaxServer.Models.SavePoint;
using ManaxServer.Models.Serie;
using ManaxServer.Services.BackgroundTask;
using ManaxServer.Services.Fix;
using ManaxServer.Services.Notification;
using ManaxServer.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/serie")]
[ApiController]
public class SerieController(
    ManaxContext context,
    INotificationService notificationService,
    IFixService fixService,
    IBackgroundTaskService backgroundTaskService)
    : ControllerBase
{
    [HttpGet("/api/series")]
    [RequirePermission(Permission.ReadSeries)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<long>>> GetSeries()
    {
        return await context.Series.Select(serie => serie.Id).ToListAsync();
    }

    [HttpGet("{id:long}")]
    [RequirePermission(Permission.ReadSeries)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SerieDto>> GetSerie(long id)
    {
        Serie? serie = await context.Series
            .Include(s => s.Tags)
            .Include(s => s.Persons)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (serie == null) return NotFound(ErrorCode.SerieDoesNotExist);

        return serie.ToDto();
    }

    [HttpGet("{id:long}/chapters")]
    [RequirePermission(Permission.ReadChapters)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<List<long>> GetSerieChapters(long id)
    {
        Serie? serie = context.Series.FirstOrDefault(s => s.Id == id);
        if (serie == null) return NotFound(ErrorCode.SerieDoesNotExist);
        List<long> chaptersIds = context.Chapters
            .Where(c => c.SerieId == id)
            .OrderBy(c => c.Number)
            .Select(c => c.Id).ToList();

        return chaptersIds;
    }

    [HttpGet("{id:long}/reads")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<List<ReadDto>> GetSerieReads(long id)
    {
        Serie? serie = context.Series.FirstOrDefault(s => s.Id == id);
        if (serie == null) return NotFound(ErrorCode.SerieDoesNotExist);
        List<ReadDto> reads = context.Reads
            .Where(r => r.Chapter.SerieId == id)
            .Where(r => r.UserId == UserController.GetCurrentUserId(HttpContext))
            .Select(r => r.ToDto())
            .ToList();

        return reads;
    }

    [HttpGet("{id:long}/poster")]
    [RequirePermission(Permission.ReadSeries)]
    [Produces("image/webp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetPoster(long id)
    {
        Serie? serie = context.Series
            .Include(s => s.SavePoint)
            .FirstOrDefault(s => s.Id == id);
        if (serie == null) return NotFound(ErrorCode.SerieDoesNotExist);
        string poster = serie.PosterPath;
        if (!System.IO.File.Exists(poster)) return NotFound(ErrorCode.SerieHasNoPoster);
        byte[] readAllBytes = await System.IO.File.ReadAllBytesAsync(poster);
        return File(readAllBytes, "image/webp", Path.GetFileName(poster));
    }

    [HttpGet("{id:long}/banner")]
    [RequirePermission(Permission.ReadSeries)]
    [Produces("image/webp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetBanner(long id)
    {
        Serie? serie = context.Series
            .Include(s => s.SavePoint)
            .FirstOrDefault(s => s.Id == id);
        if (serie == null) return NotFound(ErrorCode.SerieDoesNotExist);
        string banner = serie.BannerPath;
        if (!System.IO.File.Exists(banner)) return NotFound(ErrorCode.SerieHasNoBanner);
        byte[] readAllBytes = await System.IO.File.ReadAllBytesAsync(banner);
        return File(readAllBytes, "image/webp", Path.GetFileName(banner));
    }

    [HttpPut("{id:long}")]
    [RequirePermission(Permission.WriteSeries)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> PutSerie(long id, SerieUpdateDto serieUpdate)
    {
        Serie? serie = await context.Series.FindAsync(id);
        if (serie == null) return NotFound(ErrorCode.SerieDoesNotExist);
        if (serieUpdate.Title.Trim() == string.Empty) { return BadRequest(ErrorCode.InvalidSerieData);}
        serie.Update(serieUpdate, context);

        try { await context.SaveChangesAsync(); }
        catch { return BadRequest(ErrorCode.InvalidSerieData); }
        
        backgroundTaskService.AddTask(new FixSerieBackGroundTask(fixService, serie.Id));
        notificationService.NotifySerieUpdatedAsync(serie.ToDto());
        return Ok();
    }

    [HttpPost]
    [RequirePermission(Permission.WriteSeries)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<long>> PostSerie(SerieCreateDto serieCreate)
    {
        SavePoint? savePoint = SelectSavePoint();
        if (savePoint == null) return BadRequest(ErrorCode.NoSavePointAvailable);
        if (!serieCreate.IsValid()) { return BadRequest(ErrorCode.InvalidSerieData);}
        Serie serie = new(serieCreate, savePoint);
        string folderPath = serie.SavePath;
        if (System.IO.File.Exists(folderPath)) return BadRequest(ErrorCode.SerieAlreadyExists);
        
        context.Series.Add(serie);
        await context.SaveChangesAsync();
        Directory.CreateDirectory(folderPath);
        notificationService.NotifySerieCreatedAsync(serie.ToDto());
        backgroundTaskService.AddTask(new FixSerieBackGroundTask(fixService, serie.Id));
        return Ok(serie.Id);
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

    [HttpDelete("{id:long}")]
    [RequirePermission(Permission.DeleteSeries)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteSerie(long id)
    {
        Serie? serie = await context.Series.FindAsync(id);
        if (serie == null) return NotFound(ErrorCode.SerieDoesNotExist);

        context.Series.Remove(serie);
        await context.SaveChangesAsync();
        notificationService.NotifySerieDeletedAsync(serie.Id);

        return Ok();
    }

    [HttpPost("search")]
    [RequirePermission(Permission.ReadSeries)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<List<long>> Search(Search search)
    {
        Regex regex = new(search.RegexSearch, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        Dictionary<long, int> seriesWithChapterCount = context.Chapters
            .GroupBy(c => c.SerieId)
            .Select(g => new { SerieId = g.Key, ChapterCount = g.Count() })
            .ToDictionary(x => x.SerieId, x => x.ChapterCount);

        List<Serie> series = context.Series.Include(serie => serie.Library).ToList();

        if (search.IncludedLibraries.Count > 0)
            series = series.Where(s => search.IncludedLibraries.Contains(s.Library?.Id ?? -1)).ToList();
        series = series.Where(s => !search.ExcludedLibraries.Contains(s.Library?.Id ?? -1)).ToList();

        if (search.IncludedStatuses.Count > 0)
            series = series.Where(s => search.IncludedStatuses.Contains(s.Status)).ToList();
        series = series.Where(s => !search.ExcludedStatuses.Contains(s.Status)).ToList();

        List<long> result = series
            .Where(s => regex.IsMatch(s.Title) || regex.IsMatch(s.Description))
            .Where(s =>
            {
                int chapterCount = seriesWithChapterCount.TryGetValue(s.Id, out int value) ? value : 0;
                return chapterCount >= search.MinChapters && chapterCount <= search.MaxChapters;
            })
            .Select(s => s.Id)
            .ToList();
        return Ok(result);
    }
}