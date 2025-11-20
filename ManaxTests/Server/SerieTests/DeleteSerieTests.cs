using ManaxServer.Models.Chapter;
using ManaxServer.Models.Serie;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.SerieTests;

[TestClass]
public class DeleteSerieTests : SerieTestsSetup
{
    [TestMethod]
    public async Task DeleteSerieWithValidIdRemovesSerie()
    {
        Serie serie = Context.Series.First();
        IActionResult result = await Controller.DeleteSerie(serie.Id);

        Assert.IsInstanceOfType<OkResult>(result);

        Serie? deletedSerie = await Context.Series.FindAsync(serie.Id);
        Assert.IsNull(deletedSerie);
    }

    [TestMethod]
    public async Task DeleteSerieWithInvalidIdReturnsNotFound()
    {
        IActionResult result = await Controller.DeleteSerie(999999);

        Assert.IsInstanceOfType<NotFoundResult>(result);
    }

    [TestMethod]
    public async Task DeleteSerieWithAssociatedChaptersRemovesSerieAndChapters()
    {
        Serie serie = Context.Series.First();
        List<Chapter> associatedChapters = Context.Chapters.Where(c => c.SerieId == serie.Id).ToList();
        List<long> chapterIds = associatedChapters.Select(c => c.Id).ToList();

        IActionResult result = await Controller.DeleteSerie(serie.Id);

        Assert.IsInstanceOfType<OkResult>(result);

        Serie? deletedSerie = await Context.Series.FindAsync(serie.Id);
        Assert.IsNull(deletedSerie);

        int remainingChaptersCount = Context.Chapters.Count(c => chapterIds.Contains(c.Id));
        Assert.AreEqual(0, remainingChaptersCount);
    }

    [TestMethod]
    public async Task DeleteSerieVerifySerieCountDecreases()
    {
        int initialCount = Context.Series.Count();
        Serie serie = Context.Series.First();

        IActionResult result = await Controller.DeleteSerie(serie.Id);

        Assert.IsInstanceOfType<OkResult>(result);

        int finalCount = Context.Series.Count();
        Assert.AreEqual(initialCount - 1, finalCount);
    }
}