using ManaxLibrary.DTO.Chapter;
using ManaxServer.Models.Chapter;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.ChapterTests;

[TestClass]
public class GetChapterTests : ChapterTestsSetup
{
    [TestMethod]
    public async Task GetChaptersReturnsAllChapterIds()
    {
        ActionResult<IEnumerable<long>> result = await Controller.GetChapters();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<long>? returnedIds = result.Value as List<long>;
        Assert.IsNotNull(returnedIds);
        Assert.HasCount(3, returnedIds);
        foreach (Chapter chapter in Context.Chapters) Assert.Contains(chapter.Id, returnedIds);
    }

    [TestMethod]
    public async Task GetChapterWithValidIdReturnsChapter()
    {
        Chapter chapter = Context.Chapters.First();
        ActionResult<ChapterDto> result = await Controller.GetChapter(chapter.Id);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        ChapterDto? returnedChapter = result.Value;
        Assert.IsNotNull(returnedChapter);
        Assert.AreEqual(chapter.Id, returnedChapter.Id);
        Assert.AreEqual(chapter.SerieId, returnedChapter.SerieId);
        Assert.AreEqual(chapter.FileName, returnedChapter.FileName);
    }

    [TestMethod]
    public async Task GetChapterWithInvalidIdReturnsNotFound()
    {
        ActionResult<ChapterDto> result = await Controller.GetChapter(999999);

        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task GetChapterPageWithInvalidIdReturnsNotFound()
    {
        IActionResult result = await Controller.GetChapterPage(999999, 0);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task GetChapterPagesWithInvalidIdReturnsNotFound()
    {
        IActionResult result = await Controller.GetChapterPages(999999);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task GetChapterPageWithValidIdAndInvalidPageNumberReturnsNotFound()
    {
        Chapter chapter = Context.Chapters.First();
        IActionResult result = await Controller.GetChapterPage(chapter.Id, 999);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task GetChapterPageWithNegativePageNumberReturnsNotFound()
    {
        Chapter chapter = Context.Chapters.First();
        IActionResult result = await Controller.GetChapterPage(chapter.Id, -1);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task GetChapterVerifyAllPropertiesMapping()
    {
        Chapter chapter = Context.Chapters.First();
        ActionResult<ChapterDto> result = await Controller.GetChapter(chapter.Id);

        ChapterDto? returnedChapter = result.Value;
        Assert.IsNotNull(returnedChapter);
        Assert.AreEqual(chapter.Id, returnedChapter.Id);
        Assert.AreEqual(chapter.SerieId, returnedChapter.SerieId);
        Assert.AreEqual(chapter.FileName, returnedChapter.FileName);
        Assert.AreEqual(chapter.Number, returnedChapter.Number);
        Assert.AreEqual(chapter.PageNumber, returnedChapter.PageNumber);
        Assert.AreEqual(chapter.Creation, returnedChapter.Creation);
        Assert.AreEqual(chapter.LastModification, returnedChapter.LastModification);
    }

    [TestMethod]
    public async Task GetChapterPageWithSubZeroNumberReturnsNotFound()
    {
        Chapter chapter = Context.Chapters.First();
        IActionResult result = await Controller.GetChapterPage(chapter.Id, -1);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task GetChaptersVerifyCorrectCount()
    {
        ActionResult<IEnumerable<long>> result = await Controller.GetChapters();

        List<long>? returnedIds = result.Value as List<long>;
        Assert.IsNotNull(returnedIds);
        Assert.HasCount(3, returnedIds);
    }
}