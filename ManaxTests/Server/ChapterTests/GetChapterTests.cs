using ManaxLibrary;
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
        Assert.IsTrue(chapter.DtoEquals(returnedChapter));
    }

    [TestMethod]
    public async Task GetChapterWithInvalidIdReturnsNotFound()
    {
        ActionResult<ChapterDto> result = await Controller.GetChapter(999999);
        
        CheckTypeAndErrorCode<NotFoundObjectResult>(result.Result, ErrorCode.ChapterDoesNotExist);
    }

    [TestMethod]
    public async Task GetChapterPageWithInvalidIdReturnsNotFound()
    {
        ActionResult result = await Controller.GetChapterPage(999999, 0);

        CheckTypeAndErrorCode<NotFoundObjectResult>(result, ErrorCode.ChapterDoesNotExist);
    }

    [TestMethod]
    public async Task GetChapterPagesWithInvalidIdReturnsNotFound()
    {
        ActionResult result = await Controller.GetChapterPages(999999);

        CheckTypeAndErrorCode<NotFoundObjectResult>(result, ErrorCode.ChapterDoesNotExist);
    }

    [TestMethod]
    public async Task GetChapterPageWithValidIdAndInvalidPageNumberReturnsNotFound()
    {
        Chapter chapter = Context.Chapters.First();
        ActionResult result = await Controller.GetChapterPage(chapter.Id, 999);

        CheckTypeAndErrorCode<NotFoundObjectResult>(result, ErrorCode.PageDoesNotExist);
    }

    [TestMethod]
    public async Task GetChapterPageWithNegativePageNumberReturnsNotFound()
    {
        Chapter chapter = Context.Chapters.First();
        ActionResult result = await Controller.GetChapterPage(chapter.Id, -1);

        CheckTypeAndErrorCode<NotFoundObjectResult>(result, ErrorCode.PageDoesNotExist);
    }

    [TestMethod]
    public async Task GetChapterVerifyAllPropertiesMapping()
    {
        Chapter chapter = Context.Chapters.First();
        ActionResult<ChapterDto> result = await Controller.GetChapter(chapter.Id);

        ChapterDto? returnedChapter = result.Value;
        Assert.IsNotNull(returnedChapter);
        Assert.IsTrue(chapter.DtoEquals(returnedChapter));
    }

    [TestMethod]
    public async Task GetChapterPageWithSubZeroNumberReturnsNotFound()
    {
        Chapter chapter = Context.Chapters.First();
        ActionResult result = await Controller.GetChapterPage(chapter.Id, -1);

        CheckTypeAndErrorCode<NotFoundObjectResult>(result, ErrorCode.PageDoesNotExist);
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