using ManaxLibrary.DTO.Library;
using ManaxServer.Models.Library;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.LibraryTests;

[TestClass]
public class CreateLibraryTests: LibraryTestsSetup
{
    [TestMethod]
    public async Task PostLibrary_WithValidData_CreatesLibrary()
    {
        LibraryCreateDto createDto = new()
        {
            Name = "New Library"
        };

        ActionResult<long> result = await Controller.PostLibrary(createDto);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        long? libraryId = result.Value;
        Assert.IsNotNull(libraryId);

        Library? createdLibrary = await Context.Libraries.FindAsync(libraryId);
        Assert.IsNotNull(createdLibrary);
        Assert.AreEqual(createDto.Name, createdLibrary.Name);
    }

    [TestMethod]
    public async Task PostLibrary_WithDuplicateName_ReturnsConflict()
    {
        Library existingLibrary = Context.Libraries.First();
        LibraryCreateDto createDto = new()
        {
            Name = existingLibrary.Name
        };

        ActionResult<long> result = await Controller.PostLibrary(createDto);

        Assert.IsInstanceOfType(result.Result, typeof(ConflictObjectResult));
    }

    [TestMethod]
    public async Task PostLibrary_WithEmptyName_ReturnsBadRequest()
    {
        LibraryCreateDto createDto = new()
        {
            Name = ""
        };

        ActionResult<long> result = await Controller.PostLibrary(createDto);

        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task PostLibrary_CreationDateSetCorrectly()
    {
        LibraryCreateDto createDto = new()
        {
            Name = "New Library with Date"
        };

        DateTime beforeCreation = DateTime.UtcNow;
        ActionResult<long> result = await Controller.PostLibrary(createDto);
        DateTime afterCreation = DateTime.UtcNow;

        long? libraryId = result.Value;
        Assert.IsNotNull(libraryId);

        Library? createdLibrary = await Context.Libraries.FindAsync(libraryId);
        Assert.IsNotNull(createdLibrary);
        Assert.IsTrue(createdLibrary.Creation >= beforeCreation);
        Assert.IsTrue(createdLibrary.Creation <= afterCreation);
    }
}