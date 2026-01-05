using ManaxLibrary;
using ManaxServer.Models.Chapter;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.ChapterTests;

[TestClass]
public class DeleteChapterTests : ChapterTestsSetup
{
    [TestMethod]
    public async Task DeleteChapterWithValidIdRemovesChapter()
    {
        Chapter chapter = Context.Chapters.First();
        ActionResult result = await Controller.DeleteChapter(chapter.Id);

        Assert.IsInstanceOfType<OkResult>(result);

        Chapter? deletedChapter = await Context.Chapters.FindAsync(chapter.Id);
        Assert.IsNull(deletedChapter);
    }

    [TestMethod]
    public async Task DeleteChapterWithInvalidIdReturnsNotFound()
    {
        ActionResult result = await Controller.DeleteChapter(999999);
        CheckTypeAndErrorCode<NotFoundObjectResult>(result, ErrorCode.ChapterDoesNotExist);
    }

    [TestMethod]
    public async Task DeleteChapterVerifyChapterCountDecreases()
    {
        int initialCount = Context.Chapters.Count();
        Chapter chapter = Context.Chapters.First();

        ActionResult result = await Controller.DeleteChapter(chapter.Id);
        int finalCount = Context.Chapters.Count();

        Assert.IsInstanceOfType<OkResult>(result);
        Assert.AreEqual(initialCount - 1, finalCount);
    }

    [TestMethod]
    public async Task DeleteChapterFromSpecificSerieOnlyRemovesThatChapter()
    {
        Chapter chapter = Context.Chapters.First(c => c.SerieId == 1);
        int initialSerieChaptersCount = Context.Chapters.Count(c => c.SerieId == 1);

        ActionResult result = await Controller.DeleteChapter(chapter.Id);
        int finalSerieChaptersCount = Context.Chapters.Count(c => c.SerieId == 1);
        int otherSeriesChaptersCount = Context.Chapters.Count(c => c.SerieId != 1);

        Assert.IsInstanceOfType<OkResult>(result);
        Assert.AreEqual(initialSerieChaptersCount - 1, finalSerieChaptersCount);
        Assert.IsGreaterThan(0, otherSeriesChaptersCount);
    }
}