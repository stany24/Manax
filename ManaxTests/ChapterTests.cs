using ManaxLibrary.DTO.Chapter;
using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using ManaxTests.Mocks;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ManaxTests;

[TestClass]
public class TestChapterController
{
    private ManaxContext _context = null!;
    private ChapterController _controller = null!;
    private ManaxMapper _mapper = null!;
    private Mock<INotificationService> _mockNotificationService = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = InMemoryTestDbContextFactory.CreateTestContext();

        _mapper = new ManaxMapper(new ManaxMapping());
        _mockNotificationService = new Mock<INotificationService>();

        _controller = new ChapterController(_context, _mapper, _mockNotificationService.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Dispose();
    }

    [TestMethod]
    public async Task GetChapters_ReturnsAllChapterIds()
    {
        ActionResult<IEnumerable<long>> result = await _controller.GetChapters();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<long>? returnedIds = result.Value as List<long>;
        Assert.IsNotNull(returnedIds);
        Assert.HasCount(3, returnedIds);
        foreach (Chapter chapter in _context.Chapters) Assert.Contains(chapter.Id, returnedIds);
    }

    [TestMethod]
    public async Task GetChapter_WithValidId_ReturnsChapter()
    {
        Chapter chapter = _context.Chapters.First();
        ActionResult<ChapterDto> result = await _controller.GetChapter(chapter.Id);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        ChapterDto? returnedChapter = result.Value;
        Assert.IsNotNull(returnedChapter);
        Assert.AreEqual(chapter.Id, returnedChapter.Id);
        Assert.AreEqual(chapter.SerieId, returnedChapter.SerieId);
        Assert.AreEqual(chapter.FileName, returnedChapter.FileName);
    }

    [TestMethod]
    public async Task GetChapter_WithInvalidId_ReturnsNotFound()
    {
        ActionResult<ChapterDto> result = await _controller.GetChapter(999999);

        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task DeleteChapter_WithValidId_RemovesChapter()
    {
        Chapter chapter = _context.Chapters.First();
        IActionResult result = await _controller.DeleteChapter(chapter.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Chapter? deletedChapter = await _context.Chapters.FindAsync(chapter.Id);
        Assert.IsNull(deletedChapter);
    }

    [TestMethod]
    public async Task DeleteChapter_WithInvalidId_ReturnsNotFound()
    {
        IActionResult result = await _controller.DeleteChapter(999999);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task GetChapterPage_WithInvalidId_ReturnsNotFound()
    {
        IActionResult result = await _controller.GetChapterPage(999999, 0);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task GetChapterPages_WithInvalidId_ReturnsNotFound()
    {
        IActionResult result = await _controller.GetChapterPages(999999);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task GetChapterPage_WithValidIdAndInvalidPageNumber_ReturnsNotFound()
    {
        Chapter chapter = _context.Chapters.First();
        IActionResult result = await _controller.GetChapterPage(chapter.Id, 999);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task GetChapterPage_WithNegativePageNumber_ReturnsNotFound()
    {
        Chapter chapter = _context.Chapters.First();
        IActionResult result = await _controller.GetChapterPage(chapter.Id, -1);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task DeleteChapter_VerifyChapterCountDecreases()
    {
        int initialCount = _context.Chapters.Count();
        Chapter chapter = _context.Chapters.First();
        
        IActionResult result = await _controller.DeleteChapter(chapter.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        
        int finalCount = _context.Chapters.Count();
        Assert.AreEqual(initialCount - 1, finalCount);
    }

    [TestMethod]
    public async Task GetChapter_VerifyAllPropertiesMapping()
    {
        Chapter chapter = _context.Chapters.First();
        ActionResult<ChapterDto> result = await _controller.GetChapter(chapter.Id);

        ChapterDto? returnedChapter = result.Value;
        Assert.IsNotNull(returnedChapter);
        Assert.AreEqual(chapter.Id, returnedChapter.Id);
        Assert.AreEqual(chapter.SerieId, returnedChapter.SerieId);
        Assert.AreEqual(chapter.FileName, returnedChapter.FileName);
        Assert.AreEqual(chapter.Number, returnedChapter.Number);
        Assert.AreEqual(chapter.Pages, returnedChapter.Pages);
        Assert.AreEqual(chapter.Creation, returnedChapter.Creation);
        Assert.AreEqual(chapter.LastModification, returnedChapter.LastModification);
    }

    [TestMethod]
    public async Task GetChapterPage_WithSubZeroNumber_ReturnsNotFound()
    {
        Chapter chapter = _context.Chapters.First();
        IActionResult result = await _controller.GetChapterPage(chapter.Id, -1);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task GetChapters_VerifyCorrectCount()
    {
        ActionResult<IEnumerable<long>> result = await _controller.GetChapters();

        List<long>? returnedIds = result.Value as List<long>;
        Assert.IsNotNull(returnedIds);
        Assert.HasCount(3, returnedIds);
    }
    
    [TestMethod]
    public async Task DeleteChapter_FromSpecificSerie_OnlyRemovesThatChapter()
    {
        Chapter chapter = _context.Chapters.First(c => c.SerieId == 1);
        int initialSerieChaptersCount = _context.Chapters.Count(c => c.SerieId == 1);
        
        IActionResult result = await _controller.DeleteChapter(chapter.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        
        int finalSerieChaptersCount = _context.Chapters.Count(c => c.SerieId == 1);
        Assert.AreEqual(initialSerieChaptersCount - 1, finalSerieChaptersCount);
        
        int otherSeriesChaptersCount = _context.Chapters.Count(c => c.SerieId != 1);
        Assert.IsGreaterThan(0, otherSeriesChaptersCount);
    }
}