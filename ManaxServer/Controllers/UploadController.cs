using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;
using ImageMagick;
using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Setting;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.Serie;
using ManaxServer.Services.Fix;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using ManaxServer.Services.Task;
using ManaxServer.Settings;
using ManaxServer.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/upload")]
[ApiController]
public partial class UploadController(
    ManaxContext context,
    IMapper mapper,
    INotificationService notificationService,
    ITaskService taskService,
    IFixService fixService) : ControllerBase
{
    [GeneratedRegex("\\d{1,4}")]
    private static partial Regex RegexNumber();

    // POST: api/upload/chapter
    [HttpPost("chapter")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadChapter(IFormFile file, [FromForm] long serieId)
    {
        return await CreateOrReplaceChapter(file, serieId, false);
    }

    // POST: api/upload/chapter/replace
    [HttpPost("chapter/replace")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReplaceChapter(IFormFile file, [FromForm] long serieId)
    {
        return await CreateOrReplaceChapter(file, serieId, true);
    }

    // POST: api/upload/poster
    [HttpPost("poster")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPoster(IFormFile file, [FromForm] long serieId)
    {
        return await CreateOrReplacePoster(file, serieId, false);
    }

    // POST: api/upload/poster/replace
    [HttpPost("poster/replace")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReplacePoster(IFormFile file, [FromForm] long serieId)
    {
        return await CreateOrReplacePoster(file, serieId, true);
    }

    private async Task<IActionResult> CreateOrReplaceChapter(IFormFile file, [FromForm] long serieId, bool removeOld)
    {
        Serie? serie = context.Series
            .Include(s => s.SavePoint)
            .FirstOrDefault(s => s.Id == serieId);
        if (serie == null)
            return BadRequest(Localizer.GetString("SerieNotFound"));

        if (!TryGetPagesCountFromCbz(file, out int pagesCount))
            return BadRequest(Localizer.GetString("InvalidZipFile"));

        string filePath = Path.Combine(serie.SavePath, file.FileName);
        bool replacing = false;
        if (Directory.Exists(filePath) || System.IO.File.Exists(filePath))
        {
            if (removeOld)
            {
                replacing = true;
                System.IO.File.Delete(filePath);
            }
            else
            {
                return BadRequest(Localizer.GetString("ChapterAlreadyExists"));
            }
        }

        await SaveFileAsync(file, filePath);

        int number = ExtractChapterNumber(file.FileName);
        DateTime creation = GetChapterCreationDate(filePath);

        Chapter chapter = new()
        {
            SerieId = serieId,
            Path = filePath,
            FileName = file.FileName,
            Number = number,
            Pages = pagesCount,
            Creation = creation,
            LastModification = DateTime.UtcNow
        };

        context.Chapters.Add(chapter);
        serie.LastModification = DateTime.UtcNow;
        await context.SaveChangesAsync();
        if (!replacing) notificationService.NotifyChapterAddedAsync(mapper.Map<ChapterDto>(chapter));

        _ = taskService.AddTaskAsync(new FixChapterTask(fixService, chapter.Id));
        _ = taskService.AddTaskAsync(new FixSerieTask(fixService, chapter.SerieId));

        return Ok();
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
        return match.Success ? Convert.ToInt32(match.Value,CultureInfo.InvariantCulture) : 0;
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
            return BadRequest(Localizer.GetString("SerieNotFound"));

        ImageFormat format = SettingsManager.Data.PosterFormat;
        string path = Path.Combine(serie.SavePath,
            SettingsManager.Data.PosterName + "." + format.ToString().ToLower(CultureInfo.InvariantCulture));
        if (System.IO.File.Exists(path) && !replace) return BadRequest(Localizer.GetString("PosterAlreadyExists"));
        try
        {
            MagickImage image = new(file.OpenReadStream());
            image.Quality = SettingsManager.Data.PosterQuality;
            await image.WriteAsync(path, GetMagickFormat(format));
            _ = taskService.AddTaskAsync(new FixPosterTask(fixService, serie.Id));
            notificationService.NotifyPosterModifiedAsync(serie.Id);
        }
        catch (Exception e)
        {
            return BadRequest(Localizer.Format("InvalidImageFile", e.Message));
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
}