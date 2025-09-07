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
        Chapter chapter = Context.Chapters.First();
        User user = Context.Users.First();

        Read existingRead = new()
        {
            ChapterId = chapter.Id,
            UserId = user.Id,
            Page = 5,
            Date = DateTime.UtcNow,
            Chapter = chapter,
            User = user
        };
        Context.Reads.Add(existingRead);
        await Context.SaveChangesAsync();

        IActionResult result = await Controller.Unread((int)chapter.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Read? deletedRead = await Context.Reads
            .FirstOrDefaultAsync(r => r.ChapterId == chapter.Id && r.UserId == 1);
        Assert.IsNull(deletedRead);
    }

    [TestMethod]
    public async Task Unread_WithNonExistingRead_ReturnsOk()
    {
        Chapter chapter = Context.Chapters.First();

        IActionResult result = await Controller.Unread((int)chapter.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));
    }

    [TestMethod]
    public async Task Unread_WithInvalidChapterId_ReturnsOk()
    {
        IActionResult result = await Controller.Unread(999999);

        Assert.IsInstanceOfType(result, typeof(OkResult));
    }

    [TestMethod]
    public async Task Unread_WithoutAuthentication_ReturnsUnauthorized()
    {
        Controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        IActionResult result = await Controller.Unread(1);

        Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
    }

    [TestMethod]
    public async Task Unread_VerifyReadCountDecreases()
    {
        Chapter chapter = Context.Chapters.First();
        User user = Context.Users.First();

        Read existingRead = new()
        {
            ChapterId = chapter.Id,
            UserId = user.Id,
            Page = 5,
            Date = DateTime.UtcNow,
            Chapter = chapter,
            User = user
        };
        Context.Reads.Add(existingRead);
        await Context.SaveChangesAsync();

        int initialCount = Context.Reads.Count();

        IActionResult result = await Controller.Unread((int)chapter.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        int finalCount = Context.Reads.Count();
        Assert.AreEqual(initialCount - 1, finalCount);
    }

    [TestMethod]
    public async Task Unread_WithDifferentUser_DoesNotAffectOtherUserReads()
    {
        Chapter chapter = Context.Chapters.First();
        User user = Context.Users.First();
        User otherUser = Context.Users.Skip(1).First();

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

        Context.Reads.AddRange(userRead, otherUserRead);
        await Context.SaveChangesAsync();

        IActionResult result = await Controller.Unread((int)chapter.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Read? deletedUserRead = await Context.Reads
            .FirstOrDefaultAsync(r => r.ChapterId == chapter.Id && r.UserId == user.Id);
        Assert.IsNull(deletedUserRead);

        Read? remainingOtherUserRead = await Context.Reads
            .FirstOrDefaultAsync(r => r.ChapterId == chapter.Id && r.UserId == otherUser.Id);
        Assert.IsNotNull(remainingOtherUserRead);
        Assert.AreEqual(10, remainingOtherUserRead.Page);
    }
}