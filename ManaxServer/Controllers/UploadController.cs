using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;
using ImageMagick;
using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Setting;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
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

[Route("api/upload")]
[ApiController]
public partial class UploadController(
    ManaxContext context,
    IMapper mapper,
    INotificationService notificationService,
    IBackgroundTaskService backgroundTaskService,
    IFixService fixService) : ControllerBase
{
    [GeneratedRegex("\\d{1,4}")]
    private static partial Regex RegexNumber();

    [GeneratedRegex(@"[^a-zA-Z0-9_\-\.]")]
    private static partial Regex InvalidPathChars();

    // POST: api/upload/chapter
    [HttpPost("chapter")]
    [RequirePermission(Permission.UploadChapter)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadChapter(IFormFile file, [FromForm] long serieId)
    {
        Serie? serie = context.Series
            .Include(s => s.SavePoint)
            .FirstOrDefault(s => s.Id == serieId);
        if (serie == null)
            return BadRequest(Localizer.SerieNotFound(serieId));

        if (!TryGetPagesCountFromCbz(file, out int pagesCount))
            return BadRequest(Localizer.InvalidZipFile());

        string filePath = Path.Combine(serie.SavePath, file.FileName);
        if (Directory.Exists(filePath) || System.IO.File.Exists(filePath))
            return BadRequest(Localizer.ChapterAlreadyExists());

        await SaveFileAsync(file, filePath);

        int number = ExtractChapterNumber(file.FileName);

        DateTime creation = GetChapterCreationDate(filePath);

        Chapter chapter = new()
        {
            SerieId = serieId,
            Path = filePath,
            FileName = file.FileName,
            Number = number,
            PageNumber = pagesCount,
            Creation = creation,
            LastModification = DateTime.UtcNow
        };

        context.Chapters.Add(chapter);
        serie.LastModification = DateTime.UtcNow;
        await context.SaveChangesAsync();

        notificationService.NotifyChapterAddedAsync(mapper.Map<ChapterDto>(chapter));

        _ = backgroundTaskService.AddTaskAsync(new FixChapterBackGroundTask(fixService, chapter.Id));
        _ = backgroundTaskService.AddTaskAsync(new FixSerieBackGroundTask(fixService, chapter.SerieId));

        return Ok();
    }

    // POST: api/upload/chapter/replace
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
            return BadRequest(Localizer.SerieNotFound(serieId));

        string sanitizedFileName = SanitizeFileName(file.FileName);
        if (string.IsNullOrEmpty(sanitizedFileName))
            return BadRequest("Invalid filename");

        string filePath = Path.Combine(serie.SavePath, sanitizedFileName);
        if (!IsPathSafe(filePath, serie.SavePath))
            return BadRequest("Invalid file path");

        if (!Directory.Exists(filePath) && !System.IO.File.Exists(filePath))
            return BadRequest(Localizer.ChapterDoesNotExist());

        int number = ExtractChapterNumber(sanitizedFileName);
        Chapter? chapter = context.Chapters.FirstOrDefault(c => c.Number == number);
        if (chapter == null)
            return BadRequest(Localizer.ChapterDoesNotExist());

        if (!TryGetPagesCountFromCbz(file, out int pagesCount))
            return BadRequest(Localizer.InvalidZipFile());

        chapter.LastModification = DateTime.UtcNow;
        chapter.PageNumber = pagesCount;

        serie.LastModification = DateTime.UtcNow;

        await SaveFileAsync(file, filePath);
        await context.SaveChangesAsync();

        notificationService.NotifyChapterModifiedAsync(mapper.Map<ChapterDto>(chapter));

        _ = backgroundTaskService.AddTaskAsync(new FixChapterBackGroundTask(fixService, chapter.Id));
        _ = backgroundTaskService.AddTaskAsync(new FixSerieBackGroundTask(fixService, chapter.SerieId));

        return Ok();
    }

    // POST: api/upload/poster
    [HttpPost("poster")]
    [RequirePermission(Permission.UploadChapter)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPoster(IFormFile file, [FromForm] long serieId)
    {
        return await CreateOrReplacePoster(file, serieId, false);
    }

    // POST: api/upload/poster/replace
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

    private static int ExtractChapterNumber(string fileName)
    {
        Regex regex = RegexNumber();
        Match match = regex.Match(fileName);
        return match.Success ? Convert.ToInt32(match.Value, CultureInfo.InvariantCulture) : 0;
    }

    private DateTime GetChapterCreationDate(string filePath)
    {
        Chapter? replacedChapter = context.Chapters.FirstOrDefault(c => c.FileName == filePath);
        return replacedChapter?.Creation ?? DateTime.UtcNow;
    }

    private async Task<IActionResult> CreateOrReplacePoster(IFormFile file, [FromForm] long serieId, bool replace)
    {
        Serie? serie = context.Series
            .Include(s => s.SavePoint)
            .FirstOrDefault(s => s.Id == serieId);
        if (serie == null)
            return BadRequest(Localizer.SerieNotFound(serieId));

        ImageFormat format = SettingsManager.Data.PosterFormat;
        string path = Path.Combine(serie.SavePath,
            SettingsManager.Data.PosterName + "." + format.ToString().ToLower(CultureInfo.InvariantCulture));
        if (System.IO.File.Exists(path) && !replace) return BadRequest(Localizer.PosterAlreadyExists());
        try
        {
            MagickImage image = new(file.OpenReadStream());
            image.Quality = SettingsManager.Data.PosterQuality;
            await image.WriteAsync(path, GetMagickFormat(format));
            _ = backgroundTaskService.AddTaskAsync(new FixPosterBackGroundTask(fixService, serie.Id));
            notificationService.NotifyPosterModifiedAsync(serie.Id);
        }
        catch (Exception e)
        {
            return BadRequest(Localizer.InvalidImageFile(e.Message));
        }

        return Ok();
    }

    private static MagickFormat GetMagickFormat(ImageFormat format)
    {
        return format switch
        {
            ImageFormat.Webp => MagickFormat.WebP,
            ImageFormat.Png => MagickFormat.Png,
            ImageFormat.Jpeg => MagickFormat.Jpeg,
            _ => MagickFormat.WebP
        };
    }

    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return string.Empty;
        fileName = fileName.Replace("..", "").Replace("/", "").Replace("\\", "");
        return InvalidPathChars().Replace(fileName, "");
    }

    private static bool IsPathSafe(string filePath, string basePath)
    {
        try
        {
            string fullFilePath = Path.GetFullPath(filePath);
            string fullBasePath = Path.GetFullPath(basePath);
            return fullFilePath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}