using ManaxLibrary.DTO.SavePoint;
using ManaxServer.Controllers;
using ManaxServer.Models;
using ManaxServer.Models.SavePoint;
using ManaxServer.Services.Mapper;
using ManaxTests.Mocks;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests;

[TestClass]
public class TestSavePointController
{
    private ManaxContext _context = null!;
    private SavePointController _controller = null!;
    private ManaxMapper _mapper = null!;
    private string _testDirectory = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = SqliteTestDbContextFactory.CreateTestContext();
        _mapper = new ManaxMapper(new ManaxMapping());
        _controller = new SavePointController(_context, _mapper);

        _testDirectory = Path.Combine(Path.GetTempPath(), "SavePointTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    [TestCleanup]
    public void Cleanup()
    {
        SqliteTestDbContextFactory.CleanupTestDatabase(_context);

        if (!Directory.Exists(_testDirectory)) return;
        try
        {
            Directory.Delete(_testDirectory, true);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [TestMethod]
    public async Task PostSavePoint_WithValidData_CreatesSavePoint()
    {
        SavePointCreateDto createDto = new()
        {
            Path = _testDirectory
        };

        ActionResult<long> result = await _controller.PostSavePoint(createDto);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        long? savePointId = result.Value;
        Assert.IsNotNull(savePointId);

        SavePoint? createdSavePoint = await _context.SavePoints.FindAsync(savePointId);
        Assert.IsNotNull(createdSavePoint);
        Assert.AreEqual(createDto.Path, createdSavePoint.Path);
    }

    [TestMethod]
    public async Task PostSavePoint_WithDuplicatePath_ReturnsConflict()
    {
        SavePoint existingSavePoint = _context.SavePoints.First();
        SavePointCreateDto createDto = new()
        {
            Path = existingSavePoint.Path
        };

        ActionResult<long> result = await _controller.PostSavePoint(createDto);

        Assert.IsInstanceOfType(result.Result, typeof(ConflictObjectResult));
    }

    [TestMethod]
    public async Task PostSavePoint_WithNonExistentPath_ReturnsConflict()
    {
        SavePointCreateDto createDto = new()
        {
            Path = "/non/existent/path"
        };

        ActionResult<long> result = await _controller.PostSavePoint(createDto);

        Assert.IsInstanceOfType(result.Result, typeof(ConflictObjectResult));
    }

    [TestMethod]
    public async Task PostSavePoint_CreationDateSetCorrectly()
    {
        SavePointCreateDto createDto = new()
        {
            Path = _testDirectory
        };

        DateTime beforeCreation = DateTime.UtcNow;
        ActionResult<long> result = await _controller.PostSavePoint(createDto);
        DateTime afterCreation = DateTime.UtcNow;

        long? savePointId = result.Value;
        Assert.IsNotNull(savePointId);

        SavePoint? createdSavePoint = await _context.SavePoints.FindAsync(savePointId);
        Assert.IsNotNull(createdSavePoint);
        Assert.IsTrue(createdSavePoint.Creation >= beforeCreation);
        Assert.IsTrue(createdSavePoint.Creation <= afterCreation);
    }

    [TestMethod]
    public async Task PostSavePoint_VerifySavePointCountIncreases()
    {
        int initialCount = _context.SavePoints.Count();
        SavePointCreateDto createDto = new()
        {
            Path = _testDirectory
        };

        ActionResult<long> result = await _controller.PostSavePoint(createDto);

        long? savePointId = result.Value;
        Assert.IsNotNull(savePointId);

        int finalCount = _context.SavePoints.Count();
        Assert.AreEqual(initialCount + 1, finalCount);
    }

    [TestMethod]
    public async Task PostSavePoint_WithSpecialCharactersInPath_CreatesSavePoint()
    {
        string specialDirectory = Path.Combine(_testDirectory, "spécial-chars_123");
        Directory.CreateDirectory(specialDirectory);

        SavePointCreateDto createDto = new()
        {
            Path = specialDirectory
        };

        ActionResult<long> result = await _controller.PostSavePoint(createDto);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        long? savePointId = result.Value;
        Assert.IsNotNull(savePointId);

        SavePoint? createdSavePoint = await _context.SavePoints.FindAsync(savePointId);
        Assert.IsNotNull(createdSavePoint);
        Assert.AreEqual(createDto.Path, createdSavePoint.Path);
    }

    [TestMethod]
    public async Task PostSavePoint_MultipleDifferentPaths_CreatesMultipleSavePoints()
    {
        string[] directories = ["subdir1", "subdir2", "subdir3"];
        List<long> createdIds = [];

        foreach (string dir in directories)
        {
            string fullPath = Path.Combine(_testDirectory, dir);
            Directory.CreateDirectory(fullPath);

            SavePointCreateDto createDto = new()
            {
                Path = fullPath
            };

            ActionResult<long> result = await _controller.PostSavePoint(createDto);

            long? savePointId = result.Value;
            Assert.IsNotNull(savePointId);
            createdIds.Add(savePointId.Value);
        }

        Assert.HasCount(3, createdIds);
        Assert.AreEqual(createdIds.Count, createdIds.Distinct().Count());

        foreach (long id in createdIds)
        {
            SavePoint? savePoint = await _context.SavePoints.FindAsync(id);
            Assert.IsNotNull(savePoint);
        }
    }

    [TestMethod]
    public async Task PostSavePoint_WithRelativePath_CreatesConflict()
    {
        SavePointCreateDto createDto = new()
        {
            Path = "./relative/path/to/savepoint"
        };

        ActionResult<long> result = await _controller.PostSavePoint(createDto);

        Assert.IsInstanceOfType(result.Result, typeof(ConflictObjectResult));
    }

    [TestMethod]
    public async Task PostSavePoint_PathCaseSensitivity_CreatesDistinctSavePoints()
    {
        string lowerDir = Path.Combine(_testDirectory, "test");
        string upperDir = Path.Combine(_testDirectory, "TEST");

        Directory.CreateDirectory(lowerDir);
        Directory.CreateDirectory(upperDir);

        SavePointCreateDto createDto1 = new()
        {
            Path = lowerDir
        };

        SavePointCreateDto createDto2 = new()
        {
            Path = upperDir
        };

        ActionResult<long> result1 = await _controller.PostSavePoint(createDto1);
        ActionResult<long> result2 = await _controller.PostSavePoint(createDto2);

        long? savePointId1 = result1.Value;
        long? savePointId2 = result2.Value;

        Assert.IsNotNull(savePointId1);
        Assert.IsNotNull(savePointId2);
        Assert.AreNotEqual(savePointId1, savePointId2);

        SavePoint? savePoint1 = await _context.SavePoints.FindAsync(savePointId1);
        SavePoint? savePoint2 = await _context.SavePoints.FindAsync(savePointId2);

        Assert.IsNotNull(savePoint1);
        Assert.IsNotNull(savePoint2);
        Assert.AreEqual(lowerDir, savePoint1.Path);
        Assert.AreEqual(upperDir, savePoint2.Path);
    }

    [TestMethod]
    public async Task PostSavePoint_WithEmptyPath_ReturnsConflict()
    {
        SavePointCreateDto createDto = new()
        {
            Path = ""
        };

        ActionResult<long> result = await _controller.PostSavePoint(createDto);

        Assert.IsInstanceOfType(result.Result, typeof(ConflictObjectResult));
    }
}