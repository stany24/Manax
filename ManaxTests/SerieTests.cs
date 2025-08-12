using AutoMapper;
using ManaxLibrary.DTO.Serie;
using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.Serie;
using ManaxServer.Services.Notification;
using ManaxTests.Mocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ManaxTests;

[TestClass]
public class TestSerieController
{
    private Mock<ManaxContext> _mockContext = null!;
    private Mock<IMapper> _mockMapper = null!;
    private Mock<INotificationService> _mockNotificationService = null!;
    private SerieController _controller = null!;
    private List<Serie> _series = null!;
    private List<Chapter> _chapters = null!;

    [TestInitialize]
    public void Setup()
    {
        TestData data = TestDbContextFactory.CreateMockContext();
        _mockContext = data.Context;
        _series = data.Series;
        _chapters = data.Chapters;
            
        _mockMapper = TestDbContextFactory.CreateMockMapper();
        _mockNotificationService = new Mock<INotificationService>();

        _mockMapper.Setup(m => m.Map(It.IsAny<SerieUpdateDto>(), It.IsAny<Serie>()))
            .Callback<SerieUpdateDto, Serie>((updateDto, serie) =>
            {
                serie.Title = updateDto.Title;
                serie.Description = updateDto.Description;
                serie.Status = updateDto.Status;
            });

        _controller = new SerieController(_mockContext.Object, _mockMapper.Object, _mockNotificationService.Object);
    }

    [TestMethod]
    public async Task GetSeries_ReturnsAllSerieIds()
    {
        ActionResult<IEnumerable<long>> result = await _controller.GetSeries();
            
        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);
            
        List<long>? returnedIds = result.Value as List<long>;
        Assert.IsNotNull(returnedIds);
        Assert.AreEqual(_series.Count, returnedIds.Count);
        foreach (Serie serie in _series)
        {
            Assert.IsTrue(returnedIds.Contains(serie.Id));
        }
    }
        
    [TestMethod]
    public async Task GetSerie_WithValidId_ReturnsSerie()
    {
        Serie serie = _series[0];
        ActionResult<SerieDto> result = await _controller.GetSerie(serie.Id);
            
        SerieDto? returnedSerie = result.Value;
        Assert.IsNotNull(returnedSerie);
        Assert.AreEqual(serie.Id, returnedSerie.Id);
        Assert.AreEqual(serie.Id, returnedSerie.LibraryId);
        Assert.AreEqual(serie.Title, returnedSerie.Title);
        Assert.AreEqual(serie.Description, returnedSerie.Description);
        Assert.AreEqual(serie.Status, returnedSerie.Status);
    }
        
    [TestMethod]
    public async Task GetSerie_WithInvalidId_ReturnsNotFound()
    {
        ActionResult<SerieDto> result = await _controller.GetSerie(999999);
            
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
    }
        
    [TestMethod]
    public void GetSerieChapters_WithValidId_ReturnsChapterIds()
    {
        Serie serie = _series[0];
        List<Chapter> chaptersOfSerie = _chapters.Where(c => c.SerieId == serie.Id).ToList();
        
        ActionResult<List<long>> result = _controller.GetSerieChapters(serie.Id);
        
        Assert.IsNull(result.Result);
        
        Assert.IsNotNull(result.Value);
            
        List<long> returnedIds = result.Value.ToList();
        Assert.AreEqual(chaptersOfSerie.Count, returnedIds.Count);
        foreach (Chapter chapter in chaptersOfSerie)
        {
            Assert.IsTrue(returnedIds.Contains(chapter.Id));
        }
    }
        
    [TestMethod]
    public void GetSerieChapters_WithInvalidId_ReturnsNotFound()
    {
       ActionResult<List<long>> result = _controller.GetSerieChapters(999999);
            
       NotFoundResult? notFoundResult = result.Result as NotFoundResult;
       Assert.IsNotNull(notFoundResult);
       
        Assert.IsNull(result.Value);
    }
        
    [TestMethod]
    public async Task PutSerie_WithValidId_UpdatesSerie()
    {
        Serie serie = _series[0];
        SerieUpdateDto updateDto = new()
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Status = Status.Completed
        };
            
        IActionResult result = await _controller.PutSerie(serie.Id, updateDto);
            
        Assert.IsInstanceOfType(result, typeof(OkResult));
            
        Serie updatedSerie = _series.First(s => s.Id == serie.Id);
        Assert.AreEqual(updateDto.Title, updatedSerie.Title);
        Assert.AreEqual(updateDto.Description, updatedSerie.Description);
        Assert.AreEqual(updateDto.Status, updatedSerie.Status);
            
        _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
        
    [TestMethod]
    public async Task PutSerie_WithInvalidId_ReturnsNotFound()
    {
        SerieUpdateDto updateDto = new()
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Status = Status.Completed
        };
            
        IActionResult result = await _controller.PutSerie(999999, updateDto);
            
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}