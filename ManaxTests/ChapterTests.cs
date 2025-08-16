using ManaxLibrary.DTO.Chapter;
using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using ManaxTests.Mocks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ManaxTests;

[TestClass]
public class TestChapterController
{
    private ManaxContext _context = null!;
    private ManaxMapper _mapper = null!;
    private Mock<INotificationService> _mockNotificationService = null!;
    private ChapterController _controller = null!;

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
        foreach (Chapter chapter in _context.Chapters)
        {
            Assert.Contains(chapter.Id, returnedIds);
        }
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
        Assert.AreEqual(chapter.Id, returnedChapter.SerieId);
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
}