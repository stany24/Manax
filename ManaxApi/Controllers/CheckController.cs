using ManaxApi.Auth;
using ManaxApi.BackgroundTask;
using ManaxApi.Models;
using ManaxApi.Models.Chapter;
using ManaxApi.Models.Serie;
using ManaxApi.Services;
using ManaxLibrary.DTOs.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Controllers;

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

        if (serie == null) return NotFound();

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

        if (chapter == null) return NotFound();

        _ = TaskManagerService.AddTaskAsync(new ChapterCheckTask(chapter.Id));

        return Ok();
    }

    [HttpGet("tasks")]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Dictionary<string, int>> GetTasks()
    {
        return TaskManagerService.GetTasks().Result;
    }
}