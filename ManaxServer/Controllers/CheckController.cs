using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.Serie;
using ManaxServer.Services;
using ManaxServer.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/scan")]
[ApiController]
public class CheckController(ManaxContext context) : ControllerBase
{
    [HttpGet("serie/{id:long}")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CheckSerie(long id)
    {
        Serie? serie = await context.Series.FirstOrDefaultAsync(s => s.Id == id);

        if (serie == null) return NotFound(Localizer.Format("SerieNotFound", id));

        _ = ServicesManager.Task.AddTaskAsync(new FixSerieTask(serie.Id));

        return Ok();
    }

    [HttpGet("chapter/{id:long}")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CheckChapter(long id)
    {
        Chapter? chapter = await context.Chapters
            .FirstOrDefaultAsync(l => l.Id == id);

        if (chapter == null) return NotFound(Localizer.Format("ChapterNotFound", id));

        _ = ServicesManager.Task.AddTaskAsync(new FixChapterTask(chapter.Id));

        return Ok();
    }
}