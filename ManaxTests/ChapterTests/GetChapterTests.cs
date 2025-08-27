using ManaxLibrary.DTO.Chapter;
using ManaxServer.Models.Chapter;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.ChapterTests;

[TestClass]
public class GetChapterTests: ChapterTestsSetup
{
    [TestMethod]
    public async Task GetChapters_ReturnsAllChapterIds()
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
    public async Task GetChapter_WithValidId_ReturnsChapter()
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
    public async Task GetChapter_WithInvalidId_ReturnsNotFound()
    {
        ActionResult<ChapterDto> result = await Controller.GetChapter(999999);

        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task GetChapterPage_WithInvalidId_ReturnsNotFound()
    {
        IActionResult result = await Controller.GetChapterPage(999999, 0);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task GetChapterPages_WithInvalidId_ReturnsNotFound()
    {
        IActionResult result = await Controller.GetChapterPages(999999);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task GetChapterPage_WithValidIdAndInvalidPageNumber_ReturnsNotFound()
    {
        Chapter chapter = Context.Chapters.First();
        IActionResult result = await Controller.GetChapterPage(chapter.Id, 999);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task GetChapterPage_WithNegativePageNumber_ReturnsNotFound()
    {
        Chapter chapter = Context.Chapters.First();
        IActionResult result = await Controller.GetChapterPage(chapter.Id, -1);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task GetChapter_VerifyAllPropertiesMapping()
    {
        Chapter chapter = Context.Chapters.First();
        ActionResult<ChapterDto> result = await Controller.GetChapter(chapter.Id);

        ChapterDto? returnedChapter = result.Value;
        Assert.IsNotNull(returnedChapter);
        Assert.AreEqual(chapter.Id, returnedChapter.Id);
        Assert.AreEqual(chapter.SerieId, returnedChapter.SerieId);
        Assert.AreEqual(chapter.FileName, returnedChapter.FileName);
        Assert.AreEqual(chapter.Number, returnedChapter.Number);
        Assert.AreEqual(chapter.Pages, returnedChapter.Pages);
        Assert.AreEqual(chapter.Creation, returnedChapter.Creation);
        Assert.AreEqual(chapter.LastModification, returnedChapter.LastModification);
    }

    [TestMethod]
    public async Task GetChapterPage_WithSubZeroNumber_ReturnsNotFound()
    {
        Chapter chapter = Context.Chapters.First();
        IActionResult result = await Controller.GetChapterPage(chapter.Id, -1);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task GetChapters_VerifyCorrectCount()
    {
        ActionResult<IEnumerable<long>> result = await Controller.GetChapters();

        List<long>? returnedIds = result.Value as List<long>;
        Assert.IsNotNull(returnedIds);
        Assert.HasCount(3, returnedIds);
    }
}