using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;
using ImageMagick;
using ManaxLibrary.DTO.Setting;
using ManaxLibrary.DTO.User;
using ManaxLibrary.Logging;
using ManaxServer.Attributes;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.Serie;
using ManaxServer.Services.BackgroundTask;
using ManaxServer.Services.Fix;
using ManaxServer.Services.Notification;
using ManaxServer.Settings;
using ManaxServer.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/upload")]
[ApiController]
public class UploadController(
    ManaxContext context,
    INotificationService notificationService,
    IBackgroundTaskService backgroundTaskService,
    IFixService fixService) : ControllerBase
{
    private readonly string[] _chapterNumberPatterns =
    [
        "CH\\d{1,4}",
        "(?i)chapter[-_ ]\\d{1,4}",
        "(?i)episode[-_ ][-_ ]\\d{1,4}",
        "(?i)episode[-_ ]\\d{1,4}",
        "(?i)chap[-_ ]\\d{1,4}",
        "(?i)ch.[-_ ]*\\d{1,4}",
        "(?i)ep.[-_ ]*\\d{1,4}",
        "(?i)Flight[-_ ]\\d{1,4}",
        "\\d{1,4}"
    ];

    [HttpPost("chapter")]
    [RequirePermission(Permission.UploadChapter)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadChapter(IFormFile file, [FromForm] long serieId)
    {
        Logger.LogInfo("Uploading chapter file: " + file.FileName + " to serie ID: " + serieId);
        long? currentUserId = UserController.GetCurrentUserId(HttpContext);
        if (currentUserId == null)
            return Unauthorized();
        
        Serie? serie = context.Series
            .Include(s => s.SavePoint)
            .FirstOrDefault(s => s.Id == serieId);
        if (serie == null || !TryGetPagesCountFromCbz(file, out int pagesCount)) 
            return BadRequest();
        
        int number = ExtractChapterNumber(file.FileName);
        Logger.LogInfo("Extracted chapter number: " + number);
        if (context.Chapters.Any(s => s.SerieId == serieId && s.Number == number))
            return BadRequest();
        
        string filePath = Path.Combine(serie.SavePath, number.ToString(),SettingsManager.Data.ArchiveFormat.ToString().ToLower());
        if (Directory.Exists(filePath) || System.IO.File.Exists(filePath))
            return BadRequest();
        
        string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        await SaveFileAsync(file, tempPath);
        
        NewChapter chapter = new()
        {
            SerieId = serieId,
            Number = number,
            UploaderId = (long)currentUserId,
            TempPath = tempPath,
        };

        _ = backgroundTaskService.AddTaskAsync(new FixNewChapterBackGroundTask(fixService, chapter));
        _ = backgroundTaskService.AddTaskAsync(new FixSerieBackGroundTask(fixService, chapter.SerieId));

        return Ok();
    }

    [HttpPost("chapter/replace")]
    [RequirePermission(Permission.UploadChapter)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReplaceChapter(IFormFile file, [FromForm] long serieId)
    {
        Serie? serie = context.Series
            .Include(s => s.SavePoint)
            .FirstOrDefault(s => s.Id == serieId);
        if (serie == null)
            return BadRequest();

        int number = ExtractChapterNumber(file.FileName);
        Chapter? chapter = context.Chapters.FirstOrDefault(c => c.Number == number);
        if (chapter == null || !TryGetPagesCountFromCbz(file, out int pagesCount))
            return BadRequest();

        chapter.LastModification = DateTime.UtcNow;
        chapter.PageNumber = pagesCount;

        serie.LastModification = DateTime.UtcNow;

        await SaveFileAsync(file, chapter.Path());
        await context.SaveChangesAsync();

        notificationService.NotifyChapterUpdatedAsync(chapter.ToDto());

        //_ = backgroundTaskService.AddTaskAsync(new FixNewChapterBackGroundTask(fixService, chapter));
        _ = backgroundTaskService.AddTaskAsync(new FixSerieBackGroundTask(fixService, chapter.SerieId));

        return Ok();
    }

    [HttpPost("poster")]
    [RequirePermission(Permission.UploadChapter)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPoster(IFormFile file, [FromForm] long serieId)
    {
        return await CreateOrReplacePoster(file, serieId, false);
    }

    [HttpPost("poster/replace")]
    [RequirePermission(Permission.UploadChapter)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReplacePoster(IFormFile file, [FromForm] long serieId)
    {
        return await CreateOrReplacePoster(file, serieId, true);
    }

    private static bool TryGetPagesCountFromCbz(IFormFile file, out int pagesCount)
    {
        try
        {
            using ZipArchive zipArchive = new(file.OpenReadStream());
            pagesCount = zipArchive.Entries.Count;
            return true;
        }
        catch
        {
            pagesCount = 0;
            return false;
        }
    }

    private static async Task SaveFileAsync(IFormFile file, string filePath)
    {
        byte[] buffer = new byte[file.Length];
        _ = await file.OpenReadStream().ReadAsync(buffer.AsMemory(0, (int)file.Length));
        await System.IO.File.WriteAllBytesAsync(filePath, buffer);
    }

    private int ExtractChapterNumber(string fileName)
    {
        foreach (string pattern in _chapterNumberPatterns)
        {
            Regex regex = new(pattern);
            Match match = regex.Match(fileName);
            if (!match.Success) continue;
            string numberStr = Regex.Replace(match.Value, @"[^\d]", "");
            if (int.TryParse(numberStr, out int number))
            {
                return number;
            }
        }
        return 0;
    }

    private async Task<IActionResult> CreateOrReplacePoster(IFormFile file, [FromForm] long serieId, bool replace)
    {
        Serie? serie = context.Series
            .Include(s => s.SavePoint)
            .FirstOrDefault(s => s.Id == serieId);
        if (serie == null)
            return BadRequest();

        ImageFormat format = SettingsManager.Data.PosterFormat;
        string path = Path.Combine(serie.SavePath,
            SettingsManager.Data.PosterName + "." + format.ToString().ToLower(CultureInfo.InvariantCulture));
        if (System.IO.File.Exists(path) && !replace) return BadRequest();
        try
        {
            MagickImage image = new(file.OpenReadStream());
            image.Quality = SettingsManager.Data.PosterQuality;
            await image.WriteAsync(path, format.GetMagickFormat());
            _ = backgroundTaskService.AddTaskAsync(new FixPosterBackGroundTask(fixService, serie.Id));
            notificationService.NotifyPosterUpdatedAsync(serie.Id);
        }
        catch (Exception)
        {
            return BadRequest();
        }

        return Ok();
    }
}