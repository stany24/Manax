using System.Globalization;
using ImageMagick;
using ManaxLibrary.DTO.Chapter;
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
    [HttpPost("chapter")]
    [RequirePermission(Permission.UploadChapter)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadChapter(NewChapterDto chapterDto)
    {
        Logger.LogInfo("Uploading chapter: " + chapterDto.Number + " to serie ID: " + chapterDto.SerieId);
        long? currentUserId = UserController.GetCurrentUserId(HttpContext);
        if (currentUserId == null)
            return Unauthorized();

        Serie? serie = context.Series
            .Include(s => s.SavePoint)
            .FirstOrDefault(s => s.Id == chapterDto.SerieId);
        if (serie == null ||
            context.Chapters.Any(s => s.SerieId == chapterDto.SerieId && s.Number == chapterDto.Number))
            return BadRequest();

        string filePath = Path.Combine(serie.SavePath, chapterDto.Number.ToString(CultureInfo.InvariantCulture),
            SettingsManager.DataDto.ArchiveFormat.ToString().ToLower(CultureInfo.InvariantCulture));
        if (Directory.Exists(filePath) || System.IO.File.Exists(filePath))
            return BadRequest();

        string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        await System.IO.File.WriteAllBytesAsync(tempPath, chapterDto.Data);

        NewChapter chapter = NewChapter.FromDto(chapterDto);
        chapter.UploaderId = currentUserId.Value;

        backgroundTaskService.AddTask(new FixNewChapterBackGroundTask(fixService, chapter));
        backgroundTaskService.AddTask(new FixSerieBackGroundTask(fixService, chapter.SerieId));

        return Ok();
    }

    [HttpPost("chapter/replace")]
    [RequirePermission(Permission.UploadChapter)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReplaceChapter(NewChapterDto chapterDto)
    {
        long? currentUserId = UserController.GetCurrentUserId(HttpContext);
        if (currentUserId == null)
            return Unauthorized();

        Chapter? chapter =
            context.Chapters.FirstOrDefault(c => c.Number == chapterDto.Number && c.SerieId == chapterDto.SerieId);
        if (chapter == null)
            return BadRequest();

        string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        await System.IO.File.WriteAllBytesAsync(tempPath, chapterDto.Data);

        NewChapter newChapter = NewChapter.FromDto(chapterDto);
        newChapter.UploaderId = (long)currentUserId;

        backgroundTaskService.AddTask(new ReplaceChapterBackGroundTask(fixService, chapter.Id, newChapter));
        backgroundTaskService.AddTask(new FixSerieBackGroundTask(fixService, chapter.SerieId));

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

    private async Task<IActionResult> CreateOrReplacePoster(IFormFile file, [FromForm] long serieId, bool replace)
    {
        Serie? serie = context.Series
            .Include(s => s.SavePoint)
            .FirstOrDefault(s => s.Id == serieId);
        if (serie == null)
            return BadRequest();

        ImageFormat format = SettingsManager.DataDto.PosterFormat;
        string path = Path.Combine(serie.SavePath,
            Serie.PosterName + "." + format.ToString().ToLower(CultureInfo.InvariantCulture));
        if (System.IO.File.Exists(path) && !replace) return BadRequest();
        try
        {
            MagickImage image = new(file.OpenReadStream());
            image.Quality = SettingsManager.DataDto.PosterQuality;
            await image.WriteAsync(path, format.GetMagickFormat());
            backgroundTaskService.AddTask(new FixPosterBackGroundTask(fixService, serie.Id));
            notificationService.NotifyPosterUpdatedAsync(serie.Id);
        }
        catch (Exception)
        {
            return BadRequest();
        }

        return Ok();
    }
}