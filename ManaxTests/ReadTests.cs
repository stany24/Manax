using System.Security.Claims;
using ManaxLibrary.DTO.Read;
using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.Read;
using ManaxServer.Models.User;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using ManaxTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ManaxTests;

[TestClass]
public class TestReadController
{
    private ManaxContext _context = null!;
    private ReadController _controller = null!;
    private ManaxMapper _mapper = null!;
    private Mock<INotificationService> _mockNotificationService = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = SqliteTestDbContextFactory.CreateTestContext();

        _mapper = new ManaxMapper(new ManaxMapping());
        _mockNotificationService = new Mock<INotificationService>();

        _controller = new ReadController(_context, _mapper, _mockNotificationService.Object);

        ClaimsPrincipal user = new(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "TestUser1")
        ], "test"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user
            }
        };
    }

    [TestCleanup]
    public void Cleanup()
    {
        SqliteTestDbContextFactory.CleanupTestDatabase(_context);
    }

    [TestMethod]
    public async Task Read_WithValidChapter_CreatesNewRead()
    {
        Chapter chapter = _context.Chapters.First();
        ReadCreateDto readCreateDto = new()
        {
            ChapterId = chapter.Id,
            Page = 10
        };

        IActionResult result = await _controller.Read(readCreateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Read? createdRead = await _context.Reads
            .FirstOrDefaultAsync(r => r.ChapterId == chapter.Id && r.UserId == 1);
        Assert.IsNotNull(createdRead);
        Assert.AreEqual(readCreateDto.Page, createdRead.Page);
        Assert.AreEqual(chapter.Id, createdRead.ChapterId);
        Assert.AreEqual(1, createdRead.UserId);
    }

    [TestMethod]
    public async Task Read_WithExistingRead_UpdatesExistingRead()
    {
        Chapter chapter = _context.Chapters.First();
        User user = _context.Users.First();

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
        _context.Reads.Add(existingRead);
        await _context.SaveChangesAsync();

        ReadCreateDto readCreateDto = new()
        {
            ChapterId = chapter.Id,
            Page = 15
        };

        IActionResult result = await _controller.Read(readCreateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Read? updatedRead = await _context.Reads
            .FirstOrDefaultAsync(r => r.ChapterId == chapter.Id && r.UserId == user.Id);
        Assert.IsNotNull(updatedRead);
        Assert.AreEqual(15, updatedRead.Page);
        Assert.IsTrue(updatedRead.Date > start);
    }

    [TestMethod]
    public async Task Read_WithInvalidChapter_ReturnsNotFound()
    {
        ReadCreateDto readCreateDto = new()
        {
            ChapterId = 999999,
            Page = 10
        };

        IActionResult result = await _controller.Read(readCreateDto);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task Read_WithoutAuthentication_ReturnsUnauthorized()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        ReadCreateDto readCreateDto = new()
        {
            ChapterId = 1,
            Page = 10
        };

        IActionResult result = await _controller.Read(readCreateDto);

        Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
    }

    [TestMethod]
    public async Task Read_VerifyDateIsSetToUtcNow()
    {
        Chapter chapter = _context.Chapters.First();
        ReadCreateDto readCreateDto = new()
        {
            ChapterId = chapter.Id,
            Page = 10
        };

        DateTime beforeRead = DateTime.UtcNow;
        IActionResult result = await _controller.Read(readCreateDto);
        DateTime afterRead = DateTime.UtcNow;

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Read? createdRead = await _context.Reads
            .FirstOrDefaultAsync(r => r.ChapterId == chapter.Id && r.UserId == 1);
        Assert.IsNotNull(createdRead);
        Assert.IsTrue(createdRead.Date >= beforeRead);
        Assert.IsTrue(createdRead.Date <= afterRead);
    }

    [TestMethod]
    public async Task Read_VerifyNotificationIsSent()
    {
        Chapter chapter = _context.Chapters.First();
        ReadCreateDto readCreateDto = new()
        {
            ChapterId = chapter.Id,
            Page = 10
        };

        IActionResult result = await _controller.Read(readCreateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        _mockNotificationService.Verify(x => x.NotifyReadCreated(It.IsAny<ReadDto>()), Times.Once);
    }

    [TestMethod]
    public async Task Read_WithZeroPage_CreatesRead()
    {
        Chapter chapter = _context.Chapters.First();
        ReadCreateDto readCreateDto = new()
        {
            ChapterId = chapter.Id,
            Page = 0
        };

        IActionResult result = await _controller.Read(readCreateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Read? createdRead = await _context.Reads
            .FirstOrDefaultAsync(r => r.ChapterId == chapter.Id && r.UserId == 1);
        Assert.IsNotNull(createdRead);
        Assert.AreEqual(0, createdRead.Page);
    }

    [TestMethod]
    public async Task Read_WithNegativePage_CreatesRead()
    {
        Chapter chapter = _context.Chapters.First();
        ReadCreateDto readCreateDto = new()
        {
            ChapterId = chapter.Id,
            Page = -1
        };

        IActionResult result = await _controller.Read(readCreateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Read? createdRead = await _context.Reads
            .FirstOrDefaultAsync(r => r.ChapterId == chapter.Id && r.UserId == 1);
        Assert.IsNotNull(createdRead);
        Assert.AreEqual(-1, createdRead.Page);
    }

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
    public async Task Read_UpdateExistingRead_VerifyOnlyOneReadExists()
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

        ReadCreateDto readCreateDto = new()
        {
            ChapterId = chapter.Id,
            Page = 15
        };

        IActionResult result = await _controller.Read(readCreateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        int readCount = _context.Reads.Count(r => r.ChapterId == chapter.Id && r.UserId == 1);
        Assert.AreEqual(1, readCount);
    }

    [TestMethod]
    public async Task Read_WithDifferentUser_DoesNotAffectOtherUserReads()
    {
        Chapter chapter = _context.Chapters.First();
        User otherUser = _context.Users.Skip(1).First();

        Read otherUserRead = new()
        {
            ChapterId = chapter.Id,
            UserId = otherUser.Id,
            Page = 5,
            Date = DateTime.UtcNow,
            Chapter = chapter,
            User = otherUser
        };
        _context.Reads.Add(otherUserRead);
        await _context.SaveChangesAsync();

        ReadCreateDto readCreateDto = new()
        {
            ChapterId = chapter.Id,
            Page = 10
        };

        IActionResult result = await _controller.Read(readCreateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Read? otherUserReadAfter = await _context.Reads
            .FirstOrDefaultAsync(r => r.ChapterId == chapter.Id && r.UserId == otherUser.Id);
        Assert.IsNotNull(otherUserReadAfter);
        Assert.AreEqual(5, otherUserReadAfter.Page);

        Read? currentUserRead = await _context.Reads
            .FirstOrDefaultAsync(r => r.ChapterId == chapter.Id && r.UserId == 1);
        Assert.IsNotNull(currentUserRead);
        Assert.AreEqual(10, currentUserRead.Page);
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

    [TestMethod]
    public async Task Read_VerifyNotificationCalledWithCorrectData()
    {
        Chapter chapter = _context.Chapters.First();
        ReadCreateDto readCreateDto = new()
        {
            ChapterId = chapter.Id,
            Page = 10
        };

        IActionResult result = await _controller.Read(readCreateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        _mockNotificationService.Verify(x => x.NotifyReadCreated(It.Is<ReadDto>(dto =>
            dto.ChapterId == chapter.Id &&
            dto.Page == 10 &&
            dto.UserId == 1
        )), Times.Once);
    }

    [TestMethod]
    public async Task Read_WithLargePage_CreatesRead()
    {
        Chapter chapter = _context.Chapters.First();
        ReadCreateDto readCreateDto = new()
        {
            ChapterId = chapter.Id,
            Page = 999999
        };

        IActionResult result = await _controller.Read(readCreateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Read? createdRead = await _context.Reads
            .FirstOrDefaultAsync(r => r.ChapterId == chapter.Id && r.UserId == 1);
        Assert.IsNotNull(createdRead);
        Assert.AreEqual(999999, createdRead.Page);
    }

    [TestMethod]
    public async Task Read_MultipleReadsForDifferentChapters_CreatesMultipleReads()
    {
        List<Chapter> chapters = _context.Chapters.Take(2).ToList();

        foreach (Chapter chapter in chapters)
        {
            ReadCreateDto readCreateDto = new()
            {
                ChapterId = chapter.Id,
                Page = 10
            };

            IActionResult result = await _controller.Read(readCreateDto);
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        int totalReads = _context.Reads.Count(r => r.UserId == 1);
        Assert.AreEqual(2, totalReads);

        foreach (Chapter chapter in chapters)
        {
            Read? read = await _context.Reads
                .FirstOrDefaultAsync(r => r.ChapterId == chapter.Id && r.UserId == 1);
            Assert.IsNotNull(read);
            Assert.AreEqual(10, read.Page);
        }
    }
}