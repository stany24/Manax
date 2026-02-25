using ManaxLibrary;
using ManaxServer.Models.Library;
using ManaxServer.Models.Serie;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManaxTests.Server.LibraryTests;

[TestClass]
public class DeleteLibraryTests : LibraryTestsSetup
{
    [TestMethod]
    public async Task DeleteLibraryWithValidIdRemovesLibrary()
    {
        Library library = Context.Libraries.First();
        ActionResult result = await Controller.DeleteLibrary(library.Id);

        Assert.IsInstanceOfType<OkResult>(result);

        Library? deletedLibrary = await Context.Libraries.FindAsync(library.Id);
        Assert.IsNull(deletedLibrary);
    }

    [TestMethod]
    public async Task DeleteLibraryWithInvalidIdReturnsNotFound()
    {
        ActionResult result = await Controller.DeleteLibrary(999999);

        CheckTypeAndErrorCode<NotFoundObjectResult>(result, ErrorCode.LibraryDoesNotExist);
    }

    [TestMethod]
    public async Task DeleteLibraryWithAssociatedSeriesRemovesLibraryButKeepsSeries()
    {
        Library library = Context.Libraries.First();
        List<Serie> associatedSeries =
            Context.Series.Where(s => s.Library != null && s.Library.Id == library.Id).ToList();
        int initialSeriesCount = associatedSeries.Count;
        List<long> seriesIds = associatedSeries.Select(s => s.Id).ToList();

        ActionResult result = await Controller.DeleteLibrary(library.Id);

        Assert.IsInstanceOfType<OkResult>(result);

        Library? deletedLibrary = await Context.Libraries.FindAsync(library.Id);
        Assert.IsNull(deletedLibrary);

        List<Serie> updatedSeries = Context.Series.Where(s => seriesIds.Contains(s.Id))
            .Include(serie => serie.Library).ToList();
        Assert.HasCount(initialSeriesCount, updatedSeries);

        foreach (Serie serie in updatedSeries) Assert.IsNull(serie.Library?.Id);
    }
}