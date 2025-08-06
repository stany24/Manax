using AutoMapper;
using ManaxLibrary.DTOs.Chapter;
using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxTests.Mocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ManaxTests;

[TestClass]
public class TestChapterController
{
    private Mock<ManaxContext> _mockContext = null!;
    private Mock<IMapper> _mockMapper = null!;
    private ChapterController _controller = null!;
    private List<Chapter> _chapters = null!;

    [TestInitialize]
    public void Setup()
    {
        TestData data = TestDbContextFactory.CreateMockContext();
        _mockContext = data.Context;
        _chapters = data.Chapters;
            
        _mockMapper = TestDbContextFactory.CreateMockMapper();

        _controller = new ChapterController(_mockContext.Object, _mockMapper.Object);
    }

    [TestMethod]
    public async Task GetChapters_ReturnsAllChapterIds()
    {
        ActionResult<IEnumerable<long>> result = await _controller.GetChapters();
            
        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);
            
        List<long>? returnedIds = result.Value as List<long>;
        Assert.IsNotNull(returnedIds);
        Assert.AreEqual(3, returnedIds.Count);
        foreach (Chapter chapter in _chapters)
        {
            Assert.IsTrue(returnedIds.Contains(chapter.Id));
        }
    }
        
    [TestMethod]
    public async Task GetChapter_WithValidId_ReturnsChapter()
    {
        Chapter chapter = _chapters[0];
        ActionResult<ChapterDTO> result = await _controller.GetChapter(chapter.Id);
            
        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);
            
        ChapterDTO? returnedChapter = result.Value;
        Assert.IsNotNull(returnedChapter);
        Assert.AreEqual(chapter.Id, returnedChapter.Id);
        Assert.AreEqual(chapter.Id, returnedChapter.SerieId);
        Assert.AreEqual(chapter.FileName, returnedChapter.FileName);
    }
        
    [TestMethod]
    public async Task GetChapter_WithInvalidId_ReturnsNotFound()
    {
        ActionResult<ChapterDTO> result = await _controller.GetChapter(999999);
            
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
    }
        
    [TestMethod]
    public async Task DeleteChapter_WithValidId_RemovesChapter()
    {
        Chapter chapter = _chapters[0];
        int startNumber = _chapters.Count;
        IActionResult result = await _controller.DeleteChapter(chapter.Id);
            
        Assert.IsInstanceOfType(result, typeof(NoContentResult));
        _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            
        Assert.AreEqual(startNumber-1, _chapters.Count);
        Assert.IsFalse(_chapters.Any(c => c.Id == chapter.Id));
    }
        
    [TestMethod]
    public async Task DeleteChapter_WithInvalidId_ReturnsNotFound()
    {
        IActionResult result = await _controller.DeleteChapter(999999);
            
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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