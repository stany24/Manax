using ManaxLibrary;
using ManaxLibrary.DTO.Setting;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Services.BackgroundTask;
using ManaxServer.Services.Fix;
using ManaxServer.Settings;
using ManaxServer.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ManaxServer.Controllers;

[Route("api/settings")]
[ApiController]
public class SettingsController(
    IServiceProvider serviceProvider,
    IBackgroundTaskService backgroundTaskService,
    IFixService fixService) : ControllerBase
{
    private readonly Lock _lock = new();

    [HttpGet]
    [RequirePermission(Permission.ReadServerSettings)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<SettingsDataDto> GetSettings()
    {
        return Ok(SettingsManager.DataDto);
    }

    [HttpPut]
    [RequirePermission(Permission.WriteServerSettings)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult ChangeSettings(SettingsDataDto dataDto)
    {
        lock (_lock)
        {
            SettingsDataDto oldDataDto = SettingsManager.DataDto;
            if (dataDto.Validate() != null) return BadRequest(ErrorCode.InvalidSettings);
            SettingsManager.OverwriteSettings(dataDto);
            IServiceScope scope = serviceProvider.CreateScope();
            Task.Run(() => CheckModifications(dataDto, oldDataDto, scope));
            return Ok();
        }
    }

    private void CheckModifications(SettingsDataDto newDataDto, SettingsDataDto oldDataDto, IServiceScope scope)
    {
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        lock (_lock)
        {
            HandlePosterModifications(newDataDto, oldDataDto, manaxContext);
            HandleBannerModifications(newDataDto, oldDataDto, manaxContext);
            HandleChapterModifications(newDataDto, oldDataDto, manaxContext);
            HandleSerieModifications(newDataDto, oldDataDto, manaxContext);
        }

        scope.Dispose();
    }

    private void HandleSerieModifications(SettingsDataDto newDataDto, SettingsDataDto oldDataDto, ManaxContext manaxContext)
    {
    }

    private void HandleBannerModifications(SettingsDataDto newDataDto, SettingsDataDto oldDataDto, ManaxContext manaxContext)
    {
    }

    private void HandleChapterModifications(SettingsDataDto newDataDto, SettingsDataDto oldDataDto, ManaxContext manaxContext)
    {
        if (newDataDto.ImageFormat != oldDataDto.ImageFormat ||
            newDataDto.ImageQuality != oldDataDto.ImageQuality ||
            newDataDto.MaxChapterWidth != oldDataDto.MaxChapterWidth ||
            newDataDto.MinChapterWidth != oldDataDto.MinChapterWidth)
            foreach (long chapterId in manaxContext.Chapters.Select(chapter => chapter.Id))
            {
                Chapter? chapter = manaxContext.Chapters.Find(chapterId);
                if (chapter == null) continue;
            }
    }

    private void HandlePosterModifications(SettingsDataDto newDataDto, SettingsDataDto oldDataDto, ManaxContext context)
    {
        if (newDataDto.MaxPosterWidth != oldDataDto.MaxPosterWidth || newDataDto.MinPosterWidth != oldDataDto.MinPosterWidth ||
            newDataDto.PosterQuality != oldDataDto.PosterQuality)
            foreach (long serieId in context.Series.Select(serie => serie.Id))
                backgroundTaskService.AddTask(new FixPosterBackGroundTask(fixService, serieId));
    }
}