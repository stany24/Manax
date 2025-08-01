using AutoMapper;
using ManaxLibrary.DTOs.Read;
using ManaxLibrary.DTOs.User;
using ManaxServer.Auth;
using ManaxServer.Localization;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.Read;
using ManaxServer.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Controllers;

[Route("api/read")]
[ApiController]
public class ReadController(ManaxContext context, IMapper mapper) : ControllerBase
{
    [HttpPut("read")]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Read(ReadCreateDTO readCreate)
    {
        long? userId = UserController.GetCurrentUserId(HttpContext);
        if (userId == null) return Unauthorized(Localizer.Format("UserMustBeLoggedInRead"));

        User? user = await context.Users.FindAsync(userId);
        Chapter? chapter = await context.Chapters.FindAsync(readCreate.ChapterId);

        if (user == null || chapter == null) return NotFound(Localizer.Format("UserOrChapterNotFound"));

        Read? existingRead = await context.Reads
            .FirstOrDefaultAsync(r => r.User.Id == userId && r.Chapter.Id == readCreate.ChapterId);

        if (existingRead != null)
        {
            existingRead.Date = DateTime.UtcNow;
        }
        else
        {
            Read? read = mapper.Map<Read>(readCreate);
            read.User = user;
            read.Chapter = chapter;
            read.Date = DateTime.UtcNow;

            await context.Reads.AddAsync(read);
        }

        await context.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("unread")]
    [AuthorizeRole(UserRole.User)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Unread(int chapterId)
    {
        long? currentUserId = UserController.GetCurrentUserId(HttpContext);
        if (currentUserId == null) return Unauthorized(Localizer.Format("UserMustBeLoggedInRead"));
        
        Read? existingRead = await context.Reads
            .FirstOrDefaultAsync(r => r.User.Id == currentUserId && r.Chapter.Id == chapterId);

        if (existingRead == null) return Ok();

        context.Reads.Remove(existingRead);
        await context.SaveChangesAsync();

        return Ok();
    }
}