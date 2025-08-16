using ManaxLibrary.DTO.Serie;
using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.Serie;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using ManaxTests.Mocks;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ManaxTests;

[TestClass]
public class TestSerieController
{
    private ManaxContext _context = null!;
    private SerieController _controller = null!;
    private ManaxMapper _mapper = null!;
    private Mock<INotificationService> _mockNotificationService = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = InMemoryTestDbContextFactory.CreateTestContext();

        _mapper = new ManaxMapper(new ManaxMapping());
        _mockNotificationService = new Mock<INotificationService>();

        _controller = new SerieController(_context, _mapper, _mockNotificationService.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Dispose();
    }

    [TestMethod]
    public async Task GetSeries_ReturnsAllSerieIds()
    {
        ActionResult<IEnumerable<long>> result = await _controller.GetSeries();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<long>? returnedIds = result.Value as List<long>;
        Assert.IsNotNull(returnedIds);

        Assert.AreEqual(_context.Series.Count(), returnedIds.Count);
        foreach (Serie serie in _context.Series) Assert.Contains(serie.Id, returnedIds);
    }

    [TestMethod]
    public async Task GetSerie_WithValidId_ReturnsSerie()
    {
        Serie serie = _context.Series.First();
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
        Serie serie = _context.Series.First();
        List<Chapter> chaptersOfSerie = _context.Chapters.Where(c => c.SerieId == serie.Id).ToList();

        ActionResult<List<long>> result = _controller.GetSerieChapters(serie.Id);

        Assert.IsNull(result.Result);

        Assert.IsNotNull(result.Value);

        List<long> returnedIds = result.Value.ToList();
        Assert.AreEqual(chaptersOfSerie.Count, returnedIds.Count);
        foreach (Chapter chapter in chaptersOfSerie) Assert.Contains(chapter.Id, returnedIds);
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
        Serie serie = _context.Series.First();
        SerieUpdateDto updateDto = new()
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Status = Status.Completed
        };

        IActionResult result = await _controller.PutSerie(serie.Id, updateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Serie? updatedSerie = await _context.Series.FindAsync(serie.Id);
        Assert.IsNotNull(updatedSerie);
        Assert.AreEqual(updateDto.Title, updatedSerie.Title);
        Assert.AreEqual(updateDto.Description, updatedSerie.Description);
        Assert.AreEqual(updateDto.Status, updatedSerie.Status);
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
    }
}