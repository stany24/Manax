using System.IO.Compression;
using System.Text.RegularExpressions;
using AutoMapper;
using ImageMagick;
using ManaxLibrary.DTOs;
using ManaxLibrary.DTOs.Setting;
using ManaxLibrary.DTOs.User;
using ManaxServer.Auth;
using ManaxServer.BackgroundTask;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.Serie;
using ManaxServer.Services;
using ManaxServer.Settings;
using Microsoft.AspNetCore.Mvc;

namespace ManaxServer.Controllers;

[Route("api/upload")]
[ApiController]
public partial class UploadController(ManaxContext context, IMapper mapper) : ControllerBase
{
    [GeneratedRegex("\\d{1,4}")]
    private static partial Regex RegexNumber();

    // POST: api/upload/chapter
    [HttpPost("chapter")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadChapter(IFormFile file, [FromForm] long serieId)
    {
        return await UpdateOrReplaceChapter(file, serieId, false);
    }

    // POST: api/upload/chapter/replace
    [HttpPost("chapter/replace")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReplaceChapter(IFormFile file, [FromForm] long serieId)
    {
        return await UpdateOrReplaceChapter(file, serieId, true);
    }

    // POST: api/upload/poster
    [HttpPost("poster")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPoster(IFormFile file, [FromForm] long serieId)
    {
        return await UpdateOrReplacePoster(file, serieId, false);
    }

    // POST: api/upload/poster/replace
    [HttpPost("poster/replace")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReplacePoster(IFormFile file, [FromForm] long serieId)
    {
        return await UpdateOrReplacePoster(file, serieId, true);
    }

    private async Task<IActionResult> UpdateOrReplaceChapter(IFormFile file, [FromForm] long serieId, bool replace)
    {
        Serie? serie = context.Series.FirstOrDefault(s => s.Id == serieId);
        if (serie == null)
            return BadRequest(Localizer.GetString("SerieNotFound"));

        int pagesCount;
        try
        {
            using ZipArchive zipArchive = new(file.OpenReadStream());
            pagesCount = zipArchive.Entries.Count;
        }
        catch (Exception)
        {
            return BadRequest(Localizer.GetString("InvalidZipFile"));
        }

        string filePath = Path.Combine(serie.Path, file.FileName);
        if (Directory.Exists(filePath) || System.IO.File.Exists(filePath))
        {
            if (replace)
                System.IO.File.Delete(filePath);
            else
                return BadRequest(Localizer.GetString("ChapterAlreadyExists"));
        }

        byte[] buffer = new byte[file.Length];
        _ = await file.OpenReadStream().ReadAsync(buffer.AsMemory(0, (int)file.Length));
        await System.IO.File.WriteAllBytesAsync(filePath, buffer);

        int number = 0;
        Regex regex = RegexNumber();
        Match match = regex.Match(file.FileName);
        if (match.Success) number = Convert.ToInt32(match.Value);

        Chapter chapter = new()
        {
            SerieId = serieId,
            Path = filePath,
            FileName = file.FileName,
            Number = number,
            Pages = pagesCount
        };

        context.Chapters.Add(chapter);
        await context.SaveChangesAsync();
        if (!replace) { NotificationService.NotifyChapterAddedAsync(mapper.Map<ChapterDTO>(chapter)); }

        _ = TaskManagerService.AddTaskAsync(new ChapterCheckTask(chapter.Id));
        _ = TaskManagerService.AddTaskAsync(new SerieCheckTask(chapter.SerieId));
        
        return Ok();
    }

    private async Task<IActionResult> UpdateOrReplacePoster(IFormFile file, [FromForm] long serieId, bool replace)
    {
        Serie? serie = context.Series.FirstOrDefault(s => s.Id == serieId);
        if (serie == null)
            return BadRequest(Localizer.GetString("SerieNotFound"));

        ImageFormat format = SettingsManager.Data.PosterFormat;
        string path = Path.Combine(serie.Path,SettingsManager.Data.PosterName +"."+format.ToString().ToLower());
        if (System.IO.File.Exists(path) && !replace) return BadRequest(Localizer.GetString("PosterAlreadyExists"));
        try
        {
            MagickImage image = new(file.OpenReadStream());
            image.Quality = SettingsManager.Data.PosterQuality;
            await image.WriteAsync(path, GetMagickFormat(format));
            _ = TaskManagerService.AddTaskAsync(new PosterCheckTask(serie.Id));
            NotificationService.NotifyPosterModifiedAsync(serie.Id);
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