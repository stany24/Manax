using System.IO.Compression;
using AutoMapper;
using ManaxApi.Auth;
using ManaxApi.DTOs;
using ManaxApi.Models.Chapter;
using ManaxApi.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChapterController(ManaxContext context, IMapper mapper) : ControllerBase
{
    // GET: api/Chapter
    [HttpGet("/api/chapters")]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<long>))]
    public async Task<ActionResult<IEnumerable<long>>> GetChapters()
    {
        return await context.Chapters.Select(chapter => chapter.Id).ToListAsync();
    }

    // GET: api/Chapter/5
    [HttpGet("{id:long}")]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChapterDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChapterDTO>> GetChapter(long id)
    {
        Chapter? chapter = await context.Chapters.FindAsync(id);

        if (chapter == null) return NotFound();

        return mapper.Map<ChapterDTO>(chapter);
    }

    // DELETE: api/Chapter/5
    [HttpDelete("{id:long}")]
    [AuthorizeRole(UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteChapter(long id)
    {
        Chapter? chapter = await context.Chapters.FindAsync(id);
        if (chapter == null) return NotFound();

        context.Chapters.Remove(chapter);
        await context.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/Chapter/{id}/page/{number}
    [HttpGet("{id:long}/page/{number:int}")]
    [AuthorizeRole(UserRole.User)]
    [Produces("image/webp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChapterPage(long id, int number)
    {
        Chapter? chapter = await context.Chapters.FindAsync(id);
        if (chapter == null) return NotFound();

        string filePath = chapter.Path;
        if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath) || !filePath.EndsWith(".cbz"))
            return NotFound();
        using ZipArchive archive = ZipFile.OpenRead(filePath);
        if (number < 0 || number >= archive.Entries.Count)
            return NotFound();
        ZipArchiveEntry entry = archive.Entries[number];
        await using Stream stream = entry.Open();
        using MemoryStream memoryStream = new();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return File(memoryStream.ToArray(), "image/webp", entry.Name);
    }

    // GET: api/Chapter/{id}/pages
    [HttpGet("{id:long}/pages")]
    [AuthorizeRole(UserRole.User)]
    [Produces("application/x-cbz")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChapterPages(long id)
    {
        Chapter? chapter = await context.Chapters.FindAsync(id);
        if (chapter == null) return NotFound();
        string filePath = chapter.Path;
        if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            return NotFound();
        byte[] bytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(bytes, "application/x-cbz");
    }

    private bool ChapterExists(long id)
    {
        return context.Chapters.Any(e => e.Id == id);
    }
}