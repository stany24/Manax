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
public class ScanController(ManaxContext ManaxContext)
    : ControllerBase
{
    [HttpGet("library/{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ScanLibrary(long id)
    {
        Library? library = await ManaxContext.Libraries
            .Include(l => l.Series)
            .ThenInclude(s => s.Chapters)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (library == null) return NotFound();

        TaskManagerService.AddTask(new LibraryScanTask(library, ManaxContext));

        return Ok();
    }

    [HttpGet("serie/{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ScanSerie(long id)
    {
        Serie? serie = await ManaxContext.Series
            .Include(s => s.Chapters)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (serie == null) return NotFound();

        TaskManagerService.AddTask(new SerieScanTask(serie, ManaxContext));

        return Ok();
    }

    [HttpGet("chapter/{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ScanChapter(long id)
    {
        Chapter? chapter = await ManaxContext.Chapters
            .FirstOrDefaultAsync(l => l.Id == id);

        if (chapter == null) return NotFound();

        TaskManagerService.AddTask(new ChapterScanTask(chapter, ManaxContext));

        return Ok();
    }

    [HttpGet("tasks")]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Dictionary<string, int>>> GetTasks()
    {
        return TaskManagerService.GetTasks();
    }
}