using ManaxServer.Models.Chapter;
using ManaxServer.Models.Serie;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.SerieTests;

[TestClass]
public class DeleteSerieTests : SerieTestsSetup
{
    [TestMethod]
    public async Task DeleteSerie_WithValidId_RemovesSerie()
    {
        Serie serie = Context.Series.First();
        IActionResult result = await Controller.DeleteSerie(serie.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Serie? deletedSerie = await Context.Series.FindAsync(serie.Id);
        Assert.IsNull(deletedSerie);
    }

    [TestMethod]
    public async Task DeleteSerie_WithInvalidId_ReturnsNotFound()
    {
        IActionResult result = await Controller.DeleteSerie(999999);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task DeleteSerie_WithAssociatedChapters_RemovesSerieAndChapters()
    {
        Serie serie = Context.Series.First();
        List<Chapter> associatedChapters = Context.Chapters.Where(c => c.SerieId == serie.Id).ToList();
        List<long> chapterIds = associatedChapters.Select(c => c.Id).ToList();

        IActionResult result = await Controller.DeleteSerie(serie.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Serie? deletedSerie = await Context.Series.FindAsync(serie.Id);
        Assert.IsNull(deletedSerie);

        int remainingChaptersCount = Context.Chapters.Count(c => chapterIds.Contains(c.Id));
        Assert.AreEqual(0, remainingChaptersCount);
    }

    [TestMethod]
    public async Task DeleteSerie_VerifySerieCountDecreases()
    {
        int initialCount = Context.Series.Count();
        Serie serie = Context.Series.First();

        IActionResult result = await Controller.DeleteSerie(serie.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        int finalCount = Context.Series.Count();
        Assert.AreEqual(initialCount - 1, finalCount);
    }
}