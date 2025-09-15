using ManaxLibrary.DTO.Setting;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Services.BackgroundTask;
using ManaxServer.Services.Fix;
using ManaxServer.Services.Renaming;
using ManaxServer.Settings;
using ManaxServer.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ManaxServer.Controllers;

[Route("api/settings")]
[ApiController]
public class SettingsController(
    IServiceProvider serviceProvider,
    IBackgroundTaskService backgroundTaskService,
    IFixService fixService,
    IRenamingService renamingService) : ControllerBase
{
    private readonly object _lock = new();

    [HttpGet]
    [RequirePermission(Permission.ReadServerSettings)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public SettingsData GetSettings()
    {
        return SettingsManager.Data;
    }

    [HttpPut]
    [RequirePermission(Permission.WriteServerSettings)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ChangeSettings(SettingsData data)
    {
        lock (_lock)
        {
            SettingsData oldData = SettingsManager.Data;
            if (!data.IsValid) return BadRequest(Localizer.SettingsUpdateNotForced());
            SettingsManager.OverwriteSettings(data);
            IServiceScope scope = serviceProvider.CreateScope();
            Task.Run(() => CheckModifications(data, oldData, scope));
            return Ok();
        }
    }

    private void CheckModifications(SettingsData newData, SettingsData oldData, IServiceScope scope)
    {
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        lock (_lock)
        {
            HandlePosterModifications(newData, oldData, manaxContext);
            HandleChapterModifications(newData, oldData, manaxContext);
        }

        scope.Dispose();
    }

    private void HandleChapterModifications(SettingsData newData, SettingsData oldData, ManaxContext manaxContext)
    {
        if (newData.ImageFormat != oldData.ImageFormat ||
            newData.ImageQuality != oldData.ImageQuality ||
            newData.MaxChapterWidth != oldData.MaxChapterWidth ||
            newData.MinChapterWidth != oldData.MinChapterWidth)
            foreach (long chapterId in manaxContext.Chapters.Select(chapter => chapter.Id))
                _ = backgroundTaskService.AddTaskAsync(new FixChapterBackGroundTask(fixService, chapterId));
    }

    private void HandlePosterModifications(SettingsData newData, SettingsData oldData, ManaxContext context)
    {
        if (newData.PosterName != oldData.PosterName || newData.PosterFormat != oldData.PosterFormat)
            renamingService.RenamePosters(oldData.PosterName, newData.PosterName, oldData.PosterFormat,
                newData.PosterFormat);

        if (newData.MaxPosterWidth != oldData.MaxPosterWidth || newData.MinPosterWidth != oldData.MinPosterWidth ||
            newData.PosterQuality != oldData.PosterQuality)
            foreach (long serieId in context.Series.Select(serie => serie.Id))
                _ = backgroundTaskService.AddTaskAsync(new FixPosterBackGroundTask(fixService, serieId));
    }
}