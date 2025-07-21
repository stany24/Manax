using ManaxLibrary.DTOs.User;
using ManaxServer.Auth;
using ManaxServer.BackgroundTask;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.Serie;
using ManaxServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/scan")]
[ApiController]
public class CheckController(ManaxContext context) : ControllerBase
{
    [HttpGet("serie/{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CheckSerie(long id)
    {
        Serie? serie = await context.Series.FirstOrDefaultAsync(s => s.Id == id);

        if (serie == null) return NotFound(Localizer.Format("SerieNotFound", id));

        _ = TaskManagerService.AddTaskAsync(new SerieCheckTask(serie.Id));

        return Ok();
    }

    [HttpGet("chapter/{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CheckChapter(long id)
    {
        Chapter? chapter = await context.Chapters
            .FirstOrDefaultAsync(l => l.Id == id);

        if (chapter == null) return NotFound(Localizer.Format("ChapterNotFound", id));

        _ = TaskManagerService.AddTaskAsync(new ChapterCheckTask(chapter.Id));

        return Ok();
    }
}