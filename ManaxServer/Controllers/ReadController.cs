using ManaxLibrary.DTO.Read;
using ManaxLibrary.DTO.User;
using ManaxServer.Attributes;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.Read;
using ManaxServer.Models.User;
using ManaxServer.Services.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/read")]
[ApiController]
public class ReadController(ManaxContext context, INotificationService notification) : ControllerBase
{
    [HttpPut("read")]
    [RequirePermission(Permission.MarkChapterAsRead)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Read(ReadCreateDto readCreate)
    {
        long? userId = UserController.GetCurrentUserId(HttpContext);
        if (userId == null) return Unauthorized();

        User? user = await context.Users.FindAsync(userId);
        Chapter? chapter = await context.Chapters.FindAsync(readCreate.ChapterId);

        if (user == null || chapter == null) return NotFound();

        Read? existingRead = await context.Reads
            .FirstOrDefaultAsync(r => r.User.Id == userId && r.Chapter.Id == readCreate.ChapterId);

        if (existingRead != null)
        {
            existingRead.Date = DateTime.UtcNow;
            existingRead.Page = readCreate.Page;
            await context.SaveChangesAsync();
            notification.NotifyReadCreated(existingRead.ToDto());
        }
        else
        {
            Read read = Models.Read.Read.Create(readCreate, user.Id);
            await context.Reads.AddAsync(read);
            await context.SaveChangesAsync();
            notification.NotifyReadCreated(read.ToDto());
        }


        return Ok();
    }

    [HttpPut("unread")]
    [RequirePermission(Permission.MarkChapterAsRead)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Unread(long chapterId)
    {
        long? currentUserId = UserController.GetCurrentUserId(HttpContext);
        if (currentUserId == null) return Unauthorized();

        Read? existingRead = await context.Reads
            .FirstOrDefaultAsync(r => r.User.Id == currentUserId && r.Chapter.Id == chapterId);

        if (existingRead == null) return Ok();

        context.Reads.Remove(existingRead);
        await context.SaveChangesAsync();

        return Ok();
    }
}