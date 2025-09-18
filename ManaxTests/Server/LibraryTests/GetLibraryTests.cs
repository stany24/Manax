using ManaxLibrary.DTO.Library;
using ManaxServer.Models.Library;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.LibraryTests;

[TestClass]
public class GetLibraryTests : LibraryTestsSetup
{
    [TestMethod]
    public async Task GetLibrariesReturnsAllLibraryIds()
    {
        ActionResult<IEnumerable<long>> result = await Controller.GetLibraries();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<long>? returnedIds = result.Value as List<long>;
        Assert.IsNotNull(returnedIds);
        Assert.AreEqual(Context.Libraries.Count(), returnedIds.Count);
        foreach (Library library in Context.Libraries) Assert.Contains(library.Id, returnedIds);
    }

    [TestMethod]
    public async Task GetLibraryWithValidIdReturnsLibrary()
    {
        Library library = Context.Libraries.First();
        ActionResult<LibraryDto> result = await Controller.GetLibrary(library.Id);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        LibraryDto? returnedLibrary = result.Value;
        Assert.IsNotNull(returnedLibrary);
        Assert.AreEqual(library.Id, returnedLibrary.Id);
        Assert.AreEqual(library.Name, returnedLibrary.Name);
        Assert.AreEqual(library.Creation, returnedLibrary.Creation);
    }

    [TestMethod]
    public async Task GetLibraryWithInvalidIdReturnsNotFound()
    {
        ActionResult<LibraryDto> result = await Controller.GetLibrary(999999);

        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
    }
}