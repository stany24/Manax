using ManaxLibrary.DTOs;
using ManaxLibrary.DTOs.Chapter;
using ManaxLibrary.DTOs.Setting;
using ManaxLibrary.DTOs.User;
using ManaxServer.Auth;
using ManaxServer.BackgroundTask;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Services;
using ManaxServer.Settings;
using Microsoft.AspNetCore.Mvc;

namespace ManaxServer.Controllers;

[Route("api/settings")]
[ApiController]
public class SettingsController(IServiceProvider serviceProvider) : ControllerBase
{
    private readonly object _lock = new();

    [HttpGet]
    [AuthorizeRole(UserRole.Owner)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SettingsData))]
    public SettingsData GetSettings()
    {
        return SettingsManager.Data;
    }

    [HttpPut]
    [AuthorizeRole(UserRole.Owner)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChapterDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeSettings(SettingsData data)
    {
        lock (_lock)
        {
            SettingsData oldData = SettingsManager.Data;
            if (!data.IsValid)
            {
                return BadRequest(Localizer.GetString("SettingsUpdateNotForced"));
            }
            SettingsManager.OverwriteSettings(data);
            Task.Run(() => CheckModifications(data, oldData));
            return Ok();
        }   
    }
    
    private void CheckModifications(SettingsData newData, SettingsData oldData)
    {
        lock (_lock)
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            ManaxContext scopedContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();
            HandlePosterModifications(newData, oldData, scopedContext);
        }
    }

    private static void HandlePosterModifications(SettingsData newData, SettingsData oldData, ManaxContext context)
    {
        if(newData.PosterName != oldData.PosterName || newData.PosterFormat != oldData.PosterFormat )
        {
            _ = TaskManagerService.AddTaskAsync(new PosterRenamingTask(oldData.PosterName, newData.PosterName,
                oldData.PosterFormat, newData.PosterFormat));
        }
        
        if(newData.MaxPosterWidth != oldData.MaxPosterWidth || newData.MinPosterWidth != oldData.MinPosterWidth || newData.PosterQuality != oldData.PosterQuality)
        {
            foreach (long serieId in context.Series.Select(serie => serie.Id ))
            {
                _ = TaskManagerService.AddTaskAsync(new PosterCheckTask(serieId));
            }
        }
    }
}