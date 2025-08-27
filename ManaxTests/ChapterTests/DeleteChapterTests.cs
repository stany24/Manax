using ManaxServer.Models.Chapter;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.ChapterTests;

[TestClass]
public class DeleteChapterTests: ChapterTestsSetup
{
    [TestMethod]
    public async Task DeleteChapter_WithValidId_RemovesChapter()
    {
        Chapter chapter = Context.Chapters.First();
        IActionResult result = await Controller.DeleteChapter(chapter.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        Chapter? deletedChapter = await Context.Chapters.FindAsync(chapter.Id);
        Assert.IsNull(deletedChapter);
    }

    [TestMethod]
    public async Task DeleteChapter_WithInvalidId_ReturnsNotFound()
    {
        IActionResult result = await Controller.DeleteChapter(999999);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task DeleteChapter_VerifyChapterCountDecreases()
    {
        int initialCount = Context.Chapters.Count();
        Chapter chapter = Context.Chapters.First();

        IActionResult result = await Controller.DeleteChapter(chapter.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        int finalCount = Context.Chapters.Count();
        Assert.AreEqual(initialCount - 1, finalCount);
    }

    [TestMethod]
    public async Task DeleteChapter_FromSpecificSerie_OnlyRemovesThatChapter()
    {
        Chapter chapter = Context.Chapters.First(c => c.SerieId == 1);
        int initialSerieChaptersCount = Context.Chapters.Count(c => c.SerieId == 1);

        IActionResult result = await Controller.DeleteChapter(chapter.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        int finalSerieChaptersCount = Context.Chapters.Count(c => c.SerieId == 1);
        Assert.AreEqual(initialSerieChaptersCount - 1, finalSerieChaptersCount);

        int otherSeriesChaptersCount = Context.Chapters.Count(c => c.SerieId != 1);
        Assert.IsGreaterThan(0, otherSeriesChaptersCount);
    }
}