using ManaxServer.Models.Library;
using ManaxServer.Models.Serie;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.LibraryTests;

[TestClass]
public class DeleteLibraryTests : LibraryTestsSetup
{
    [TestMethod]
    public async Task DeleteLibraryWithValidIdRemovesLibrary()
    {
        Library library = Context.Libraries.First();
        IActionResult result = await Controller.DeleteLibrary(library.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Library? deletedLibrary = await Context.Libraries.FindAsync(library.Id);
        Assert.IsNull(deletedLibrary);
    }

    [TestMethod]
    public async Task DeleteLibraryWithInvalidIdReturnsNotFound()
    {
        IActionResult result = await Controller.DeleteLibrary(999999);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task DeleteLibraryWithAssociatedSeriesRemovesLibraryButKeepsSeries()
    {
        Library library = Context.Libraries.First();
        List<Serie> associatedSeries = Context.Series.Where(s => s.LibraryId == library.Id).ToList();
        int initialSeriesCount = associatedSeries.Count;
        List<long> seriesIds = associatedSeries.Select(s => s.Id).ToList();

        IActionResult result = await Controller.DeleteLibrary(library.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Library? deletedLibrary = await Context.Libraries.FindAsync(library.Id);
        Assert.IsNull(deletedLibrary);

        List<Serie> updatedSeries = Context.Series.Where(s => seriesIds.Contains(s.Id)).ToList();
        Assert.AreEqual(initialSeriesCount, updatedSeries.Count);

        foreach (Serie serie in updatedSeries) Assert.IsNull(serie.LibraryId);
    }
}