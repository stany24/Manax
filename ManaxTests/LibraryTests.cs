using ManaxLibrary.DTO.Library;
using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Models.Library;
using ManaxServer.Models.Serie;
using ManaxServer.Services.Mapper;
using ManaxServer.Services.Notification;
using ManaxTests.Mocks;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ManaxTests;

[TestClass]
public class TestLibraryController
{
    private ManaxContext _context = null!;
    private LibraryController _controller = null!;
    private ManaxMapper _mapper = null!;
    private Mock<INotificationService> _mockNotificationService = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = SqliteTestDbContextFactory.CreateTestContext();

        _mapper = new ManaxMapper(new ManaxMapping());
        _mockNotificationService = new Mock<INotificationService>();

        _controller = new LibraryController(_context, _mapper, _mockNotificationService.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        SqliteTestDbContextFactory.CleanupTestDatabase(_context);
    }

    [TestMethod]
    public async Task GetLibraries_ReturnsAllLibraryIds()
    {
        ActionResult<IEnumerable<long>> result = await _controller.GetLibraries();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<long>? returnedIds = result.Value as List<long>;
        Assert.IsNotNull(returnedIds);
        Assert.AreEqual(_context.Libraries.Count(), returnedIds.Count);
        foreach (Library library in _context.Libraries) Assert.Contains(library.Id, returnedIds);
    }

    [TestMethod]
    public async Task GetLibrary_WithValidId_ReturnsLibrary()
    {
        Library library = _context.Libraries.First();
        ActionResult<LibraryDto> result = await _controller.GetLibrary(library.Id);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        LibraryDto? returnedLibrary = result.Value;
        Assert.IsNotNull(returnedLibrary);
        Assert.AreEqual(library.Id, returnedLibrary.Id);
        Assert.AreEqual(library.Name, returnedLibrary.Name);
        Assert.AreEqual(library.Creation, returnedLibrary.Creation);
    }

    [TestMethod]
    public async Task GetLibrary_WithInvalidId_ReturnsNotFound()
    {
        ActionResult<LibraryDto> result = await _controller.GetLibrary(999999);

        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task PostLibrary_WithValidData_CreatesLibrary()
    {
        LibraryCreateDto createDto = new()
        {
            Name = "New Library"
        };

        ActionResult<long> result = await _controller.PostLibrary(createDto);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        long? libraryId = result.Value;
        Assert.IsNotNull(libraryId);

        Library? createdLibrary = await _context.Libraries.FindAsync(libraryId);
        Assert.IsNotNull(createdLibrary);
        Assert.AreEqual(createDto.Name, createdLibrary.Name);
    }

    [TestMethod]
    public async Task PostLibrary_WithDuplicateName_ReturnsConflict()
    {
        Library existingLibrary = _context.Libraries.First();
        LibraryCreateDto createDto = new()
        {
            Name = existingLibrary.Name
        };

        ActionResult<long> result = await _controller.PostLibrary(createDto);

        Assert.IsInstanceOfType(result.Result, typeof(ConflictObjectResult));
    }

    [TestMethod]
    public async Task PostLibrary_WithEmptyName_ReturnsBadRequest()
    {
        LibraryCreateDto createDto = new()
        {
            Name = ""
        };

        ActionResult<long> result = await _controller.PostLibrary(createDto);

        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task PutLibrary_WithValidId_UpdatesLibrary()
    {
        Library library = _context.Libraries.First();
        LibraryUpdateDto updateDto = new()
        {
            Name = "Updated Library Name"
        };

        IActionResult result = await _controller.PutLibrary(library.Id, updateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Library? updatedLibrary = await _context.Libraries.FindAsync(library.Id);
        Assert.IsNotNull(updatedLibrary);
        Assert.AreEqual(updateDto.Name, updatedLibrary.Name);
    }

    [TestMethod]
    public async Task PutLibrary_WithInvalidId_ReturnsNotFound()
    {
        LibraryUpdateDto updateDto = new()
        {
            Name = "Updated Library Name"
        };

        IActionResult result = await _controller.PutLibrary(999999, updateDto);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task PutLibrary_WithDuplicateName_ReturnsConflict()
    {
        Library firstLibrary = _context.Libraries.First();
        Library secondLibrary = _context.Libraries.Skip(1).First();

        LibraryUpdateDto updateDto = new()
        {
            Name = secondLibrary.Name
        };

        IActionResult result = await _controller.PutLibrary(firstLibrary.Id, updateDto);

        Assert.IsInstanceOfType(result, typeof(ConflictObjectResult));
    }

    [TestMethod]
    public async Task PutLibrary_WithEmptyName_ReturnsBadRequest()
    {
        Library library = _context.Libraries.First();
        LibraryUpdateDto updateDto = new()
        {
            Name = ""
        };

        IActionResult result = await _controller.PutLibrary(library.Id, updateDto);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task DeleteLibrary_WithValidId_RemovesLibrary()
    {
        Library library = _context.Libraries.First();
        IActionResult result = await _controller.DeleteLibrary(library.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Library? deletedLibrary = await _context.Libraries.FindAsync(library.Id);
        Assert.IsNull(deletedLibrary);
    }

    [TestMethod]
    public async Task DeleteLibrary_WithInvalidId_ReturnsNotFound()
    {
        IActionResult result = await _controller.DeleteLibrary(999999);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task DeleteLibrary_WithAssociatedSeries_RemovesLibraryButKeepsSeries()
    {
        Library library = _context.Libraries.First();
        List<Serie> associatedSeries = _context.Series.Where(s => s.LibraryId == library.Id).ToList();
        int initialSeriesCount = associatedSeries.Count;
        List<long> seriesIds = associatedSeries.Select(s => s.Id).ToList();

        IActionResult result = await _controller.DeleteLibrary(library.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Library? deletedLibrary = await _context.Libraries.FindAsync(library.Id);
        Assert.IsNull(deletedLibrary);

        List<Serie> updatedSeries = _context.Series.Where(s => seriesIds.Contains(s.Id)).ToList();
        Assert.AreEqual(initialSeriesCount, updatedSeries.Count);

        foreach (Serie serie in updatedSeries) Assert.IsNull(serie.LibraryId);
    }

    [TestMethod]
    public async Task PostLibrary_CreationDateSetCorrectly()
    {
        LibraryCreateDto createDto = new()
        {
            Name = "New Library with Date"
        };

        DateTime beforeCreation = DateTime.UtcNow;
        ActionResult<long> result = await _controller.PostLibrary(createDto);
        DateTime afterCreation = DateTime.UtcNow;

        long? libraryId = result.Value;
        Assert.IsNotNull(libraryId);

        Library? createdLibrary = await _context.Libraries.FindAsync(libraryId);
        Assert.IsNotNull(createdLibrary);
        Assert.IsTrue(createdLibrary.Creation >= beforeCreation);
        Assert.IsTrue(createdLibrary.Creation <= afterCreation);
    }
}