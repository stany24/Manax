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
public class ScanController(LibraryContext libraryContext, SerieContext serieContext, ChapterContext chapterContext)
    : ControllerBase
{
    [HttpGet("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ScanLibrary(long id)
    {
        Library? library = await libraryContext.Libraries
            .Include(l => l.Series)
            .ThenInclude(s => s.Chapters)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (library == null) return NotFound();

        TaskManagerService.AddTask(new LibraryScanTask(library, libraryContext, serieContext, chapterContext));

        return Ok();
    }

    [HttpGet("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ScanSerie(long id)
    {
        Serie? serie = await serieContext.Series
            .Include(s => s.Chapters)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (serie == null) return NotFound();

        TaskManagerService.AddTask(new SerieScanTask(serie, libraryContext, serieContext, chapterContext));

        return Ok();
    }

    [HttpGet("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ScanChapter(long id)
    {
        Chapter? chapter = await chapterContext.Chapters
            .FirstOrDefaultAsync(l => l.Id == id);

        if (chapter == null) return NotFound();

        TaskManagerService.AddTask(new ChapterScanTask(chapter, libraryContext, serieContext, chapterContext));

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