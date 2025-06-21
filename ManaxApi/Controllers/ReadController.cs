using ManaxApi.Auth;
using ManaxApi.Models.Chapter;
using ManaxApi.Models.Read;
using ManaxApi.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReadController(UserContext userContext, ReadContext readContext) : ControllerBase
{
    [HttpPut("read")]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Read(long chapterId, DateTime? dateTime)
    {
        long? userId = UserController.GetCurrentUserId(HttpContext);
        if (userId == null) return Unauthorized();

        User? user = await userContext.Users.FindAsync(userId);
        Chapter? chapter = await readContext.Reads
            .Include(r => r.Chapter)
            .Where(r => r.Chapter.Id == chapterId)
            .Select(r => r.Chapter)
            .FirstOrDefaultAsync();

        if (user == null || chapter == null) return NotFound();

        Read? existingRead = await readContext.Reads
            .FirstOrDefaultAsync(r => r.User.Id == userId && r.Chapter.Id == chapterId);

        if (existingRead != null)
        {
            existingRead.Date = dateTime ?? DateTime.UtcNow;
        }
        else
        {
            Read read = new()
            {
                User = user,
                Chapter = chapter,
                Date = dateTime ?? DateTime.UtcNow
            };
            await readContext.Reads.AddAsync(read);
        }

        await readContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("unread")]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unread(long chapterId)
    {
        long? userId = UserController.GetCurrentUserId(HttpContext);
        if (userId == null) return Unauthorized();

        Read? existingRead = await readContext.Reads
            .FirstOrDefaultAsync(r => r.User.Id == userId && r.Chapter.Id == chapterId);

        if (existingRead == null) return Ok();

        readContext.Reads.Remove(existingRead);
        await readContext.SaveChangesAsync();

        return Ok();
    }
}