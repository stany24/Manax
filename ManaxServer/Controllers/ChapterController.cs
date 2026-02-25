using System.IO.Compression;
using ManaxLibrary;
using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Services.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/chapter/{id:long}")]
[ApiController]
public class ChapterController(ManaxContext context, INotificationService notificationService) : ControllerBase
{
    [HttpGet("/api/chapters")]
    [RequirePermission(Permission.ReadChapters)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<long>>> GetChapters()
    {
        return await context.Chapters.Select(chapter => chapter.Id).ToListAsync();
    }

    [HttpGet("")]
    [RequirePermission(Permission.ReadChapters)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChapterDto>> GetChapter(long id)
    {
        Chapter? chapter = await context.Chapters.FindAsync(id);
        if (chapter == null) return NotFound(ErrorCode.ChapterDoesNotExist);
        return Ok(chapter.ToDto());
    }

    [HttpDelete("")]
    [RequirePermission(Permission.DeleteChapters)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteChapter(long id)
    {
        Chapter? chapter = await context.Chapters.FindAsync(id);
        if (chapter == null) return NotFound(ErrorCode.ChapterDoesNotExist);

        context.Chapters.Remove(chapter);
        await context.SaveChangesAsync();
        notificationService.NotifyChapterRemovedAsync(chapter.Id);

        return Ok();
    }

    [HttpGet("page/{number:int}")]
    [RequirePermission(Permission.ReadChapters)]
    [Produces("image/webp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetChapterPage(long id, int number)
    {
        Chapter? chapter = context.Chapters
            .Include(c => c.Serie)
            .ThenInclude(s => s.SavePoint)
            .FirstOrDefault(c => c.Id == id);
        if (chapter == null) return NotFound(ErrorCode.ChapterDoesNotExist);

        string filePath = chapter.Path();
        if (!System.IO.File.Exists(filePath))
            return NotFound(ErrorCode.ChapterFileDoesNotExist);
        await using ZipArchive archive = await ZipFile.OpenReadAsync(filePath);
        if (number < 0 || number >= archive.Entries.Count)
            return BadRequest(ErrorCode.PageDoesNotExist);
        List<ZipArchiveEntry> pages = archive.Entries.ToList();
        pages.Sort((a, b) => new NaturalSortComparer().Compare(a.Name, b.Name));
        ZipArchiveEntry entry = pages[number];
        await using Stream stream = await entry.OpenAsync();
        using MemoryStream memoryStream = new();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return File(memoryStream.ToArray(), "image/webp", entry.Name);
    }

    [HttpGet("pages")]
    [RequirePermission(Permission.ReadChapters)]
    [Produces("application/x-cbz")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetChapterPages(long id)
    {
        Chapter? chapter = context.Chapters
            .Include(c => c.Serie)
            .ThenInclude(s => s.SavePoint)
            .FirstOrDefault(c => c.Id == id);
        if (chapter == null) return NotFound(ErrorCode.ChapterDoesNotExist);

        string filePath = chapter.Path();
        if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            return NotFound(ErrorCode.ChapterFileDoesNotExist);
        byte[] bytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(bytes, "application/x-cbz");
    }
}