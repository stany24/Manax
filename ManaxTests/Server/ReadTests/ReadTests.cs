using ManaxLibrary;
using ManaxLibrary.DTO.Read;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.Read;
using ManaxServer.Models.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxTests.Server.ReadTests;

[TestClass]
public class TestReadController : ReadTestsSetup
{
    [TestMethod]
    public async Task ReadWithValidChapterCreatesNewRead()
    {
        Chapter chapter = Context.Chapters.First();
        ReadCreateDto readCreateDto = new()
        {
            ChapterId = chapter.Id,
            Page = 10
        };

        ActionResult result = await Controller.Read(readCreateDto);

        Assert.IsInstanceOfType<OkResult>(result);

        Read? createdRead = await Context.Reads
            .FirstOrDefaultAsync(r => r.ChapterId == chapter.Id && r.UserId == 1);
        Assert.IsNotNull(createdRead);
        Assert.AreEqual(readCreateDto.Page, createdRead.Page);
        Assert.AreEqual(chapter.Id, createdRead.ChapterId);
        Assert.AreEqual(1, createdRead.UserId);
    }

    [TestMethod]
    public async Task ReadWithExistingReadUpdatesExistingRead()
    {
        Chapter chapter = Context.Chapters.First();
        User user = Context.Users.First();

        DateTime start = DateTime.UtcNow.AddDays(-1);
        Read existingRead = new()
        {
            ChapterId = chapter.Id,
            UserId = user.Id,
            Page = 5,
            Date = start,
            Chapter = chapter,
            User = user
        };
        Context.Reads.Add(existingRead);
        await Context.SaveChangesAsync();

        ReadCreateDto readCreateDto = new()
        {
            ChapterId = chapter.Id,
            Page = 15
        };

        ActionResult result = await Controller.Read(readCreateDto);

        Assert.IsInstanceOfType<OkResult>(result);

        Read? updatedRead = await Context.Reads
            .FirstOrDefaultAsync(r => r.ChapterId == chapter.Id && r.UserId == user.Id);
        Assert.IsNotNull(updatedRead);
        Assert.AreEqual<uint>(15, updatedRead.Page);
        Assert.IsTrue(updatedRead.Date > start);
    }

    [TestMethod]
    public async Task ReadWithInvalidChapterReturnsNotFound()
    {
        ReadCreateDto readCreateDto = new()
        {
            ChapterId = 999999,
            Page = 10
        };

        ActionResult result = await Controller.Read(readCreateDto);

        CheckTypeAndErrorCode<NotFoundObjectResult>(result, ErrorCode.ChapterDoesNotExist);
    }

    [TestMethod]
    public async Task ReadWithoutAuthenticationReturnsUnauthorized()
    {
        Controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        ReadCreateDto readCreateDto = new()
        {
            ChapterId = 1,
            Page = 10
        };

        ActionResult result = await Controller.Read(readCreateDto);

        CheckTypeAndErrorCode<UnauthorizedObjectResult>(result, ErrorCode.TokenRequired);
    }

    [TestMethod]
    public async Task ReadVerifyDateIsSetToUtcNow()
    {
        Chapter chapter = Context.Chapters.First();
        ReadCreateDto readCreateDto = new()
        {
            ChapterId = chapter.Id,
            Page = 10
        };

        DateTime beforeRead = DateTime.UtcNow;
        ActionResult result = await Controller.Read(readCreateDto);
        DateTime afterRead = DateTime.UtcNow;

        Assert.IsInstanceOfType<OkResult>(result);

        Read? createdRead = await Context.Reads
            .FirstOrDefaultAsync(r => r.ChapterId == chapter.Id && r.UserId == 1);
        Assert.IsNotNull(createdRead);
        Assert.IsTrue(createdRead.Date >= beforeRead);
        Assert.IsTrue(createdRead.Date <= afterRead);
    }

    [TestMethod]
    public async Task ReadVerifyNotificationIsSent()
    {
        Chapter chapter = Context.Chapters.First();
        ReadCreateDto readCreateDto = new()
        {
            ChapterId = chapter.Id,
            Page = 10
        };

        ActionResult result = await Controller.Read(readCreateDto);

        Assert.IsInstanceOfType<OkResult>(result);
        Assert.IsNotNull(MockNotificationService.ReadCreated);
    }

    [TestMethod]
    public async Task ReadWithZeroPageCreatesRead()
    {
        Chapter chapter = Context.Chapters.First();
        ReadCreateDto readCreateDto = new()
        {
            ChapterId = chapter.Id,
            Page = 0
        };

        ActionResult result = await Controller.Read(readCreateDto);

        Assert.IsInstanceOfType<OkResult>(result);

        Read? createdRead = await Context.Reads
            .FirstOrDefaultAsync(r => r.ChapterId == chapter.Id && r.UserId == 1);
        Assert.IsNotNull(createdRead);
        Assert.AreEqual<uint>(0, createdRead.Page);
    }

    [TestMethod]
    public async Task ReadUpdateExistingReadVerifyOnlyOneReadExists()
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

        ReadCreateDto readCreateDto = new()
        {
            ChapterId = chapter.Id,
            Page = 15
        };

        ActionResult result = await Controller.Read(readCreateDto);

        Assert.IsInstanceOfType<OkResult>(result);

        int readCount = Context.Reads.Count(r => r.ChapterId == chapter.Id && r.UserId == 1);
        Assert.AreEqual(1, readCount);
    }

    [TestMethod]
    public async Task ReadWithDifferentUserDoesNotAffectOtherUserReads()
    {
        Chapter chapter = Context.Chapters.First();
        User otherUser = Context.Users.Skip(1).First();

        Read otherUserRead = new()
        {
            ChapterId = chapter.Id,
            UserId = otherUser.Id,
            Page = 5,
            Date = DateTime.UtcNow,
            Chapter = chapter,
            User = otherUser
        };
        Context.Reads.Add(otherUserRead);
        await Context.SaveChangesAsync();

        ReadCreateDto readCreateDto = new()
        {
            ChapterId = chapter.Id,
            Page = 10
        };

        ActionResult result = await Controller.Read(readCreateDto);

        Assert.IsInstanceOfType<OkResult>(result);

        Read? otherUserReadAfter = await Context.Reads
            .FirstOrDefaultAsync(r => r.ChapterId == chapter.Id && r.UserId == otherUser.Id);
        Assert.IsNotNull(otherUserReadAfter);
        Assert.AreEqual<uint>(5, otherUserReadAfter.Page);

        Read? currentUserRead = await Context.Reads
            .FirstOrDefaultAsync(r => r.ChapterId == chapter.Id && r.UserId == 1);
        Assert.IsNotNull(currentUserRead);
        Assert.AreEqual<uint>(10, currentUserRead.Page);
    }

    [TestMethod]
    public async Task ReadVerifyNotificationCalledWithCorrectData()
    {
        Chapter chapter = Context.Chapters.First();
        ReadCreateDto readCreateDto = new()
        {
            ChapterId = chapter.Id,
            Page = 10
        };

        ActionResult result = await Controller.Read(readCreateDto);

        Assert.IsInstanceOfType<OkResult>(result);

        ReadDto? readCreated = MockNotificationService.ReadCreated;
        Assert.IsNotNull(readCreated);
        Assert.AreEqual<uint>(10, readCreated.Page);
        Assert.AreEqual(chapter.Id, readCreated.ChapterId);
        Assert.AreEqual(1, readCreated.UserId);
    }

    [TestMethod]
    public async Task ReadWithLargePageCreatesRead()
    {
        Chapter chapter = Context.Chapters.First();
        ReadCreateDto readCreateDto = new()
        {
            ChapterId = chapter.Id,
            Page = 999999
        };

        ActionResult result = await Controller.Read(readCreateDto);

        Assert.IsInstanceOfType<OkResult>(result);

        Read? createdRead = await Context.Reads
            .FirstOrDefaultAsync(r => r.ChapterId == chapter.Id && r.UserId == 1);
        Assert.IsNotNull(createdRead);
        Assert.AreEqual(Convert.ToUInt32(chapter.PageNumber), createdRead.Page);
    }

    [TestMethod]
    public async Task ReadMultipleReadsForDifferentChaptersCreatesMultipleReads()
    {
        List<Chapter> chapters = Context.Chapters.Take(2).ToList();

        foreach (Chapter chapter in chapters)
        {
            ReadCreateDto readCreateDto = new()
            {
                ChapterId = chapter.Id,
                Page = 10
            };

            ActionResult result = await Controller.Read(readCreateDto);
            Assert.IsInstanceOfType<OkResult>(result);
        }

        int totalReads = Context.Reads.Count(r => r.UserId == 1);
        Assert.AreEqual(2, totalReads);

        foreach (Chapter chapter in chapters)
        {
            Read? read = await Context.Reads
                .FirstOrDefaultAsync(r => r.ChapterId == chapter.Id && r.UserId == 1);
            Assert.IsNotNull(read);
            Assert.AreEqual<uint>(10, read.Page);
        }
    }
}