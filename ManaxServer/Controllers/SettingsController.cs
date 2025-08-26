using ManaxLibrary.DTO.Setting;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Services.Fix;
using ManaxServer.Services.Renaming;
using ManaxServer.Services.Task;
using ManaxServer.Settings;
using ManaxServer.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManaxServer.Controllers;

[Route("api/settings")]
[ApiController]
public class SettingsController(
    IServiceProvider serviceProvider,
    ITaskService taskService,
    IFixService fixService) : ControllerBase
{
    private readonly object _lock = new();

    [HttpGet]
    [Authorize(Roles = "Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public SettingsData GetSettings()
    {
        return SettingsManager.Data;
    }

    [HttpPut]
    [Authorize(Roles = "Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ChangeSettings(SettingsData data)
    {
        
        lock (_lock)
        {
            SettingsData oldData = SettingsManager.Data;
            if (!data.IsValid) return BadRequest(Localizer.GetString("SettingsUpdateNotForced"));
            SettingsManager.OverwriteSettings(data);
            IServiceScope scope = serviceProvider.CreateScope();
            Task.Run(() => CheckModifications(data, oldData,scope));
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
        {
            foreach (long serieId in manaxContext.Chapters.Select(serie => serie.Id))
                _ = taskService.AddTaskAsync(new FixChapterTask(fixService, serieId));
        }
    }

    private void HandlePosterModifications(SettingsData newData, SettingsData oldData, ManaxContext context)
    {
        if (newData.PosterName != oldData.PosterName || newData.PosterFormat != oldData.PosterFormat)
        {
            RenamingService.RenamePosters(context, oldData.PosterName, newData.PosterName,
                oldData.PosterFormat, newData.PosterFormat);
        }

        if (newData.MaxPosterWidth != oldData.MaxPosterWidth || newData.MinPosterWidth != oldData.MinPosterWidth ||
            newData.PosterQuality != oldData.PosterQuality)
            foreach (long serieId in context.Series.Select(serie => serie.Id))
                _ = taskService.AddTaskAsync(new FixPosterTask(fixService, serieId));
    }
}