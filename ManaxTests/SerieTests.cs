using AutoMapper;
using ManaxLibrary.DTOs.Serie;
using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.Serie;
using ManaxTests.Mocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Status = ManaxLibrary.DTOs.Serie.Status;

namespace ManaxTests
{
    [TestClass]
    public class TestSerieController
    {
        private Mock<ManaxContext> _mockContext = null!;
        private Mock<IMapper> _mockMapper = null!;
        private SerieController _controller = null!;
        private List<Serie> _series = null!;

        [TestInitialize]
        public void Setup()
        {
            (Mock<ManaxContext> mockContext, List<Chapter> _, List<Serie> series) = TestDbContextFactory.CreateMockContext();
            _mockContext = mockContext;
            _series = series;
            
            _mockMapper = TestDbContextFactory.CreateMockMapper();


            _mockMapper.Setup(m => m.Map(It.IsAny<SerieUpdateDTO>(), It.IsAny<Serie>()))
                .Callback<SerieUpdateDTO, Serie>((updateDto, serie) =>
                {
                    serie.Title = updateDto.Title;
                    serie.Description = updateDto.Description;
                    serie.Status = updateDto.Status;
                });

            _controller = new SerieController(_mockContext.Object, _mockMapper.Object);
        }

        [TestMethod]
        public async Task GetSeries_ReturnsAllSerieIds()
        {
            ActionResult<IEnumerable<long>> result = await _controller.GetSeries();
            
            OkObjectResult? okResult = result.Result as OkObjectResult;
            Assert.IsNull(okResult);
            
            List<long>? returnedIds = result.Value as List<long>;
            Assert.IsNotNull(returnedIds);
            Assert.AreEqual(2, returnedIds.Count);
            Assert.IsTrue(returnedIds.Contains(1));
            Assert.IsTrue(returnedIds.Contains(2));
        }
        
        [TestMethod]
        public async Task GetSerie_WithValidId_ReturnsSerie()
        {
            ActionResult<SerieDTO> result = await _controller.GetSerie(1);
            
            SerieDTO? returnedSerie = result.Value;
            Assert.IsNotNull(returnedSerie);
            Assert.AreEqual(1, returnedSerie.Id);
            Assert.AreEqual(1, returnedSerie.LibraryId);
            Assert.AreEqual("Serie 1", returnedSerie.Title);
            Assert.AreEqual("Description for Serie 1", returnedSerie.Description);
            Assert.AreEqual(Status.Ongoing, returnedSerie.Status);
        }
        
        [TestMethod]
        public async Task GetSerie_WithInvalidId_ReturnsNotFound()
        {
            ActionResult<SerieDTO> result = await _controller.GetSerie(99);
            
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        }
        
        [TestMethod]
        public void GetSerieChapters_WithValidId_ReturnsChapterIds()
        {
            IEnumerable<long> result = _controller.GetSerieChapters(1);
            
            List<long> returnedIds = result.ToList();
            Assert.AreEqual(2, returnedIds.Count);
            Assert.IsTrue(returnedIds.Contains(1));
            Assert.IsTrue(returnedIds.Contains(2));
        }
        
        [TestMethod]
        public void GetSerieChapters_WithValidIdButNoChapters_ReturnsEmptyList()
        {
            IEnumerable<long> result = _controller.GetSerieChapters(99);
            
            List<long> returnedIds = result.ToList();
            Assert.AreEqual(0, returnedIds.Count);
        }
        
        [TestMethod]
        public async Task PutSerie_WithValidId_UpdatesSerie()
        {
            SerieUpdateDTO updateDto = new()
            {
                Title = "Updated Title",
                Description = "Updated Description",
                Status = Status.Completed
            };
            
            IActionResult result = await _controller.PutSerie(1, updateDto);
            
            Assert.IsInstanceOfType(result, typeof(OkResult));
            
            Serie updatedSerie = _series.First(s => s.Id == 1);
            Assert.AreEqual("Updated Title", updatedSerie.Title);
            Assert.AreEqual("Updated Description", updatedSerie.Description);
            Assert.AreEqual(Status.Completed, updatedSerie.Status);
            
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [TestMethod]
        public async Task PutSerie_WithInvalidId_ReturnsNotFound()
        {
            SerieUpdateDTO updateDto = new()
            {
                Title = "Updated Title",
                Description = "Updated Description",
                Status = Status.Completed
            };
            
            IActionResult result = await _controller.PutSerie(99, updateDto);
            
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
