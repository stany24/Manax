using AutoMapper;
using ManaxLibrary.DTOs.Chapter;
using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxTests.Mocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ManaxTests
{
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
            (Mock<ManaxContext> mockContext, List<Chapter> chapters, _) = TestDbContextFactory.CreateMockContext();
            _mockContext = mockContext;
            _chapters = chapters;
            
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
            Assert.IsTrue(returnedIds.Contains(1));
            Assert.IsTrue(returnedIds.Contains(2));
            Assert.IsTrue(returnedIds.Contains(3));
        }
        
        [TestMethod]
        public async Task GetChapter_WithValidId_ReturnsChapter()
        {
            ActionResult<ChapterDTO> result = await _controller.GetChapter(1);
            
            OkObjectResult? okResult = result.Result as OkObjectResult;
            Assert.IsNull(okResult);
            
            ChapterDTO? returnedChapter = result.Value;
            Assert.IsNotNull(returnedChapter);
            Assert.AreEqual(1, returnedChapter.Id);
            Assert.AreEqual(1, returnedChapter.SerieId);
            Assert.AreEqual("chapter1.cbz", returnedChapter.FileName);
        }
        
        [TestMethod]
        public async Task GetChapter_WithInvalidId_ReturnsNotFound()
        {
            ActionResult<ChapterDTO> result = await _controller.GetChapter(99);
            
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        }
        
        [TestMethod]
        public async Task DeleteChapter_WithValidId_RemovesChapter()
        {
            IActionResult result = await _controller.DeleteChapter(1);
            
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            
            Assert.AreEqual(2, _chapters.Count);
            Assert.IsFalse(_chapters.Any(c => c.Id == 1));
        }
        
        [TestMethod]
        public async Task DeleteChapter_WithInvalidId_ReturnsNotFound()
        {
            IActionResult result = await _controller.DeleteChapter(99);
            
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [TestMethod]
        public async Task GetChapterPage_WithInvalidId_ReturnsNotFound()
        {
            IActionResult result = await _controller.GetChapterPage(99, 0);
            
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }
        
        [TestMethod]
        public async Task GetChapterPages_WithInvalidId_ReturnsNotFound()
        {
            IActionResult result = await _controller.GetChapterPages(99);
            
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }
    }
}
