using ManaxLibrary.DTO.Read;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.Read;
using ManaxServer.Models.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ManaxTests.ReadTests;

[TestClass]
public class UnReadTests: ReadTestsSetup
{
    [TestMethod]
    public async Task Unread_WithExistingRead_RemovesRead()
    {
        Chapter chapter = _context.Chapters.First();
        User user = _context.Users.First();

        Read existingRead = new()
        {
            ChapterId = chapter.Id,
            UserId = user.Id,
            Page = 5,
            Date = DateTime.UtcNow,
            Chapter = chapter,
            User = user
        };
        _context.Reads.Add(existingRead);
        await _context.SaveChangesAsync();

        IActionResult result = await _controller.Unread((int)chapter.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Read? deletedRead = await _context.Reads
            .FirstOrDefaultAsync(r => r.ChapterId == chapter.Id && r.UserId == 1);
        Assert.IsNull(deletedRead);
    }

    [TestMethod]
    public async Task Unread_WithNonExistingRead_ReturnsOk()
    {
        Chapter chapter = _context.Chapters.First();

        IActionResult result = await _controller.Unread((int)chapter.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));
    }

    [TestMethod]
    public async Task Unread_WithInvalidChapterId_ReturnsOk()
    {
        IActionResult result = await _controller.Unread(999999);

        Assert.IsInstanceOfType(result, typeof(OkResult));
    }

    [TestMethod]
    public async Task Unread_WithoutAuthentication_ReturnsUnauthorized()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        IActionResult result = await _controller.Unread(1);

        Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
    }

    [TestMethod]
    public async Task Unread_VerifyReadCountDecreases()
    {
        Chapter chapter = _context.Chapters.First();
        User user = _context.Users.First();

        Read existingRead = new()
        {
            ChapterId = chapter.Id,
            UserId = user.Id,
            Page = 5,
            Date = DateTime.UtcNow,
            Chapter = chapter,
            User = user
        };
        _context.Reads.Add(existingRead);
        await _context.SaveChangesAsync();

        int initialCount = _context.Reads.Count();

        IActionResult result = await _controller.Unread((int)chapter.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        int finalCount = _context.Reads.Count();
        Assert.AreEqual(initialCount - 1, finalCount);
    }

    [TestMethod]
    public async Task Unread_WithDifferentUser_DoesNotAffectOtherUserReads()
    {
        Chapter chapter = _context.Chapters.First();
        User user = _context.Users.First();
        User otherUser = _context.Users.Skip(1).First();

        Read userRead = new()
        {
            ChapterId = chapter.Id,
            UserId = user.Id,
            Page = 5,
            Date = DateTime.UtcNow,
            Chapter = chapter,
            User = user
        };

        Read otherUserRead = new()
        {
            ChapterId = chapter.Id,
            UserId = otherUser.Id,
            Page = 10,
            Date = DateTime.UtcNow,
            Chapter = chapter,
            User = otherUser
        };

        _context.Reads.AddRange(userRead, otherUserRead);
        await _context.SaveChangesAsync();

        IActionResult result = await _controller.Unread((int)chapter.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Read? deletedUserRead = await _context.Reads
            .FirstOrDefaultAsync(r => r.ChapterId == chapter.Id && r.UserId == user.Id);
        Assert.IsNull(deletedUserRead);

        Read? remainingOtherUserRead = await _context.Reads
            .FirstOrDefaultAsync(r => r.ChapterId == chapter.Id && r.UserId == otherUser.Id);
        Assert.IsNotNull(remainingOtherUserRead);
        Assert.AreEqual(10, remainingOtherUserRead.Page);
    }
}