using ManaxLibrary.DTO.SavePoint;
using ManaxServer.Models.SavePoint;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.SavePointTests;

[TestClass]
public class PostSavePointTests : SavePointTestsSetup
{
    [TestMethod]
    public async Task PostSavePointWithValidDataCreatesSavePoint()
    {
        SavePointCreateDto createDto = new()
        {
            Path = TestDirectory
        };

        ActionResult<long> result = await Controller.PostSavePoint(createDto);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        long? savePointId = result.Value;
        Assert.IsNotNull(savePointId);

        SavePoint? createdSavePoint = await Context.SavePoints.FindAsync(savePointId);
        Assert.IsNotNull(createdSavePoint);
        Assert.AreEqual(createDto.Path, createdSavePoint.Path);
    }

    [TestMethod]
    public async Task PostSavePointWithDuplicatePathReturnsConflict()
    {
        SavePoint existingSavePoint = Context.SavePoints.First();
        SavePointCreateDto createDto = new()
        {
            Path = existingSavePoint.Path
        };

        ActionResult<long> result = await Controller.PostSavePoint(createDto);

        Assert.IsInstanceOfType(result.Result, typeof(ConflictObjectResult));
    }

    [TestMethod]
    public async Task PostSavePointWithNonExistentPathReturnsConflict()
    {
        SavePointCreateDto createDto = new()
        {
            Path = "/non/existent/path"
        };

        ActionResult<long> result = await Controller.PostSavePoint(createDto);

        Assert.IsInstanceOfType(result.Result, typeof(ConflictObjectResult));
    }

    [TestMethod]
    public async Task PostSavePointCreationDateSetCorrectly()
    {
        SavePointCreateDto createDto = new()
        {
            Path = TestDirectory
        };

        DateTime beforeCreation = DateTime.UtcNow;
        ActionResult<long> result = await Controller.PostSavePoint(createDto);
        DateTime afterCreation = DateTime.UtcNow;

        long? savePointId = result.Value;
        Assert.IsNotNull(savePointId);

        SavePoint? createdSavePoint = await Context.SavePoints.FindAsync(savePointId);
        Assert.IsNotNull(createdSavePoint);
        Assert.IsTrue(createdSavePoint.Creation >= beforeCreation);
        Assert.IsTrue(createdSavePoint.Creation <= afterCreation);
    }

    [TestMethod]
    public async Task PostSavePointVerifySavePointCountIncreases()
    {
        int initialCount = Context.SavePoints.Count();
        SavePointCreateDto createDto = new()
        {
            Path = TestDirectory
        };

        ActionResult<long> result = await Controller.PostSavePoint(createDto);

        long? savePointId = result.Value;
        Assert.IsNotNull(savePointId);

        int finalCount = Context.SavePoints.Count();
        Assert.AreEqual(initialCount + 1, finalCount);
    }

    [TestMethod]
    public async Task PostSavePointWithSpecialCharactersInPathCreatesSavePoint()
    {
        string specialDirectory = Path.Combine(TestDirectory, "spécial-chars_123");
        Directory.CreateDirectory(specialDirectory);

        SavePointCreateDto createDto = new()
        {
            Path = specialDirectory
        };

        ActionResult<long> result = await Controller.PostSavePoint(createDto);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        long? savePointId = result.Value;
        Assert.IsNotNull(savePointId);

        SavePoint? createdSavePoint = await Context.SavePoints.FindAsync(savePointId);
        Assert.IsNotNull(createdSavePoint);
        Assert.AreEqual(createDto.Path, createdSavePoint.Path);
    }

    [TestMethod]
    public async Task PostSavePointMultipleDifferentPathsCreatesMultipleSavePoints()
    {
        string[] directories = ["subdir1", "subdir2", "subdir3"];
        List<long> createdIds = [];

        foreach (string dir in directories)
        {
            string fullPath = Path.Combine(TestDirectory, dir);
            Directory.CreateDirectory(fullPath);

            SavePointCreateDto createDto = new()
            {
                Path = fullPath
            };

            ActionResult<long> result = await Controller.PostSavePoint(createDto);

            long? savePointId = result.Value;
            Assert.IsNotNull(savePointId);
            createdIds.Add(savePointId.Value);
        }

        Assert.HasCount(3, createdIds);
        Assert.AreEqual(createdIds.Count, createdIds.Distinct().Count());

        foreach (long id in createdIds)
        {
            SavePoint? savePoint = await Context.SavePoints.FindAsync(id);
            Assert.IsNotNull(savePoint);
        }
    }

    [TestMethod]
    public async Task PostSavePointWithRelativePathCreatesConflict()
    {
        SavePointCreateDto createDto = new()
        {
            Path = "./relative/path/to/savepoint"
        };

        ActionResult<long> result = await Controller.PostSavePoint(createDto);

        Assert.IsInstanceOfType(result.Result, typeof(ConflictObjectResult));
    }

    [TestMethod]
    public async Task PostSavePointPathCaseSensitivityCreatesDistinctSavePoints()
    {
        string lowerDir = Path.Combine(TestDirectory, "test");
        string upperDir = Path.Combine(TestDirectory, "TEST");

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

        ActionResult<long> result1 = await Controller.PostSavePoint(createDto1);
        ActionResult<long> result2 = await Controller.PostSavePoint(createDto2);

        long? savePointId1 = result1.Value;
        long? savePointId2 = result2.Value;

        Assert.IsNotNull(savePointId1);
        Assert.IsNotNull(savePointId2);
        Assert.AreNotEqual(savePointId1, savePointId2);

        SavePoint? savePoint1 = await Context.SavePoints.FindAsync(savePointId1);
        SavePoint? savePoint2 = await Context.SavePoints.FindAsync(savePointId2);

        Assert.IsNotNull(savePoint1);
        Assert.IsNotNull(savePoint2);
        Assert.AreEqual(lowerDir, savePoint1.Path);
        Assert.AreEqual(upperDir, savePoint2.Path);
    }

    [TestMethod]
    public async Task PostSavePointWithEmptyPathReturnsConflict()
    {
        SavePointCreateDto createDto = new()
        {
            Path = ""
        };

        ActionResult<long> result = await Controller.PostSavePoint(createDto);

        Assert.IsInstanceOfType(result.Result, typeof(ConflictObjectResult));
    }
}