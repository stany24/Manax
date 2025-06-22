using ManaxApi.Auth;
using ManaxApi.Models.Chapter;
using ManaxApi.Models.Library;
using ManaxApi.Models.Serie;
using ManaxApi.Models.User;
using ManaxApi.Services;
using ManaxApi.Task;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ScanController(ManaxContext context) : ControllerBase
{

    [HttpGet("library/{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ScanLibrary(long id)
    {
        Library? library = await context.Libraries
            .Include(l => l.Series)
            .ThenInclude(s => s.Chapters)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (library == null) return NotFound();

        _ =TaskManagerService.AddTaskAsync(new LibraryScanTask(library));

        return Ok();
    }

    [HttpGet("serie/{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ScanSerie(long id)
    {
        Serie? serie = await context.Series
            .Include(s => s.Chapters)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (serie == null) return NotFound();

        _ = TaskManagerService.AddTaskAsync(new SerieScanTask(serie.Id));

        return Ok();
    }

    [HttpGet("chapter/{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ScanChapter(long id)
    {
        Chapter? chapter = await context.Chapters
            .FirstOrDefaultAsync(l => l.Id == id);

        if (chapter == null) return NotFound();

        _ = TaskManagerService.AddTaskAsync(new ChapterScanTask(chapter.Id));

        return Ok();
    }

    [HttpGet("tasks")]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Dictionary<string, int>> GetTasks()
    {
        return TaskManagerService.GetTasks();
    }
}