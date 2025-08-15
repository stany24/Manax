using System.IO.Compression;
using ManaxLibrary.DTO.Chapter;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/chapter")]
[ApiController]
public class ChapterController(ManaxContext context, IMapper mapper, INotificationService notificationService) : ControllerBase
{
    // GET: api/Chapter
    [HttpGet("/api/chapters")]
    [Authorize(Roles = "User,Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<long>))]
    public async Task<ActionResult<IEnumerable<long>>> GetChapters()
    {
        return await context.Chapters.Select(chapter => chapter.Id).ToListAsync();
    }

    // GET: api/Chapter/5
    [HttpGet("{id:long}")]
    [Authorize(Roles = "User,Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChapterDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChapterDto>> GetChapter(long id)
    {
        Chapter? chapter = await context.Chapters.FindAsync(id);
        
        if (chapter == null) return NotFound(Localizer.Format("ChapterNotFound",id));

        return mapper.Map<ChapterDto>(chapter);
    }

    // DELETE: api/Chapter/5
    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin,Owner")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteChapter(long id)
    {
        Chapter? chapter = await context.Chapters.FindAsync(id);
        if (chapter == null) return NotFound(Localizer.Format("ChapterNotFound",id));

        context.Chapters.Remove(chapter);
        await context.SaveChangesAsync();
        notificationService.NotifyChapterRemovedAsync(chapter.Id);

        return NoContent();
    }

    // GET: api/Chapter/{id}/page/{number}
    [HttpGet("{id:long}/page/{number:int}")]
    [Authorize(Roles = "User,Admin,Owner")]
    [Produces("image/webp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChapterPage(long id, int number)
    {
        Chapter? chapter = await context.Chapters.FindAsync(id);
        if (chapter == null) return NotFound(Localizer.Format("ChapterNotFound",id));

        string filePath = chapter.Path;
        if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath) || !filePath.EndsWith(".cbz"))
            return NotFound("Chapter file does not exist or is not a valid CBZ file.");
        using ZipArchive archive = ZipFile.OpenRead(filePath);
        if (number < 0 || number >= archive.Entries.Count)
            return NotFound("Page number " + number + " too big for chapter with " + archive.Entries.Count + " pages.");
        ZipArchiveEntry entry = archive.Entries[number];
        await using Stream stream = entry.Open();
        using MemoryStream memoryStream = new();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return File(memoryStream.ToArray(), "image/webp", entry.Name);
    }

    // GET: api/Chapter/{id}/pages
    [HttpGet("{id:long}/pages")]
    [Authorize(Roles = "User,Admin,Owner")]
    [Produces("application/x-cbz")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChapterPages(long id)
    {
        Chapter? chapter = await context.Chapters.FindAsync(id);
        if (chapter == null) return NotFound(Localizer.Format("ChapterNotFound",id));
        string filePath = chapter.Path;
        if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            return NotFound("Chapter file does not exist or is not a valid CBZ file.");
        byte[] bytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(bytes, "application/x-cbz");
    }
}