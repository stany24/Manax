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
        Serie serie = _context.Series.First();
        IActionResult result = await _controller.DeleteSerie(serie.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Serie? deletedSerie = await _context.Series.FindAsync(serie.Id);
        Assert.IsNull(deletedSerie);
    }

    [TestMethod]
    public async Task DeleteSerie_WithInvalidId_ReturnsNotFound()
    {
        IActionResult result = await _controller.DeleteSerie(999999);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task DeleteSerie_WithAssociatedChapters_RemovesSerieAndChapters()
    {
        Serie serie = _context.Series.First();
        List<Chapter> associatedChapters = _context.Chapters.Where(c => c.SerieId == serie.Id).ToList();
        List<long> chapterIds = associatedChapters.Select(c => c.Id).ToList();

        IActionResult result = await _controller.DeleteSerie(serie.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Serie? deletedSerie = await _context.Series.FindAsync(serie.Id);
        Assert.IsNull(deletedSerie);

        int remainingChaptersCount = _context.Chapters.Count(c => chapterIds.Contains(c.Id));
        Assert.AreEqual(0, remainingChaptersCount);
    }

    [TestMethod]
    public async Task DeleteSerie_VerifySerieCountDecreases()
    {
        int initialCount = _context.Series.Count();
        Serie serie = _context.Series.First();

        IActionResult result = await _controller.DeleteSerie(serie.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        int finalCount = _context.Series.Count();
        Assert.AreEqual(initialCount - 1, finalCount);
    }
}