using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Setting;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Services;
using ManaxServer.Settings;
using ManaxServer.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManaxServer.Controllers;

[Route("api/settings")]
[ApiController]
public class SettingsController(IServiceProvider serviceProvider) : ControllerBase
{
    private readonly object _lock = new();

    [HttpGet]
    [Authorize(Roles = "Owner")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SettingsData))]
    public SettingsData GetSettings()
    {
        return SettingsManager.Data;
    }

    [HttpPut]
    [Authorize(Roles = "Owner")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChapterDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ChangeSettings(SettingsData data)
    {
        lock (_lock)
        {
            SettingsData oldData = SettingsManager.Data;
            if (!data.IsValid)
            {
                return BadRequest(Localizer.GetString("SettingsUpdateNotForced"));
            }
            SettingsManager.OverwriteSettings(data);
            using IServiceScope scope = serviceProvider.CreateScope();
            ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();
            Task.Run(() => CheckModifications(data, oldData,manaxContext));
            return Ok();
        }   
    }
    
    private void CheckModifications(SettingsData newData, SettingsData oldData,ManaxContext manaxContext)
    {
        lock (_lock)
        {
            HandlePosterModifications(newData, oldData, manaxContext);
        }
    }

    private static void HandlePosterModifications(SettingsData newData, SettingsData oldData, ManaxContext context)
    {
        if(newData.PosterName != oldData.PosterName || newData.PosterFormat != oldData.PosterFormat )
        {
            _ = ServicesManager.Task.AddTaskAsync(new PosterRenamingTask(oldData.PosterName, newData.PosterName,
                oldData.PosterFormat, newData.PosterFormat));
        }
        
        if(newData.MaxPosterWidth != oldData.MaxPosterWidth || newData.MinPosterWidth != oldData.MinPosterWidth || newData.PosterQuality != oldData.PosterQuality)
        {
            foreach (long serieId in context.Series.Select(serie => serie.Id ))
            {
                _ = ServicesManager.Task.AddTaskAsync(new FixPosterTask(serieId));
            }
        }
    }
}