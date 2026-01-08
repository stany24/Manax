using ManaxLibrary;
using ManaxLibrary.DTO.Library;
using ManaxServer.Models.Library;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.LibraryTests;

[TestClass]
public class UpdateLibraryTests : LibraryTestsSetup
{
    [TestMethod]
    public async Task PutLibraryWithValidIdUpdatesLibrary()
    {
        Library library = Context.Libraries.First();
        LibraryUpdateDto updateDto = new()
        {
            Name = "Updated Library Name"
        };

        ActionResult result = await Controller.PutLibrary(library.Id, updateDto);

        Assert.IsInstanceOfType<OkResult>(result);

        Library? updatedLibrary = await Context.Libraries.FindAsync(library.Id);
        Assert.IsNotNull(updatedLibrary);
        Assert.AreEqual(updateDto.Name, updatedLibrary.Name);
    }

    [TestMethod]
    public async Task PutLibraryWithInvalidIdReturnsNotFound()
    {
        LibraryUpdateDto updateDto = new()
        {
            Name = "Updated Library Name"
        };

        ActionResult result = await Controller.PutLibrary(999999, updateDto);

        CheckTypeAndErrorCode<NotFoundObjectResult>(result, ErrorCode.LibraryDoesNotExist);
    }

    [TestMethod]
    public async Task PutLibraryWithDuplicateNameReturnsConflict()
    {
        Library firstLibrary = Context.Libraries.First();
        Library secondLibrary = Context.Libraries.Skip(1).First();

        LibraryUpdateDto updateDto = new()
        {
            Name = secondLibrary.Name
        };

        ActionResult result = await Controller.PutLibrary(firstLibrary.Id, updateDto);

        CheckTypeAndErrorCode<ConflictObjectResult>(result, ErrorCode.InvalidLibraryData);
    }

    [TestMethod]
    public async Task PutLibraryWithEmptyNameReturnsBadRequest()
    {
        Library library = Context.Libraries.First();
        LibraryUpdateDto updateDto = new()
        {
            Name = ""
        };

        ActionResult result = await Controller.PutLibrary(library.Id, updateDto);

        CheckTypeAndErrorCode<BadRequestObjectResult>(result, ErrorCode.InvalidLibraryData);
    }
}