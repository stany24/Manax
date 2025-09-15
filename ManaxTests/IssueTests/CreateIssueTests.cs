using System.Security.Claims;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxServer.Models.Issue.Reported;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.IssueTests;

[TestClass]
public class CreateIssueTests : IssueTestsSetup
{
    [TestMethod]
    public async Task CreateChapterIssueWithValidDataCreatesIssue()
    {
        ReportedIssueChapterCreateDto createDto = new()
        {
            ChapterId = 3,
            ProblemId = 1
        };

        ActionResult result = await Controller.CreateChapterIssue(createDto);

        Assert.IsInstanceOfType(result, typeof(CreatedResult));

        ReportedIssueChapter? createdIssue = Context.ReportedIssuesChapter
            .FirstOrDefault(i =>
                i.ChapterId == createDto.ChapterId && i.UserId == 1 && i.ProblemId == createDto.ProblemId);
        Assert.IsNotNull(createdIssue);
        Assert.AreEqual(createDto.ChapterId, createdIssue.ChapterId);
        Assert.AreEqual(createDto.ProblemId, createdIssue.ProblemId);
        Assert.AreEqual(1, createdIssue.UserId);
    }

    [TestMethod]
    public async Task CreateChapterIssueWithoutAuthenticationReturnsUnauthorized()
    {
        Controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        ReportedIssueChapterCreateDto createDto = new()
        {
            ChapterId = 1,
            ProblemId = 1
        };

        ActionResult result = await Controller.CreateChapterIssue(createDto);

        Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
    }

    [TestMethod]
    public async Task CreateChapterIssueVerifyCreationDate()
    {
        ReportedIssueChapterCreateDto createDto = new()
        {
            ChapterId = 3,
            ProblemId = 2
        };

        DateTime before = DateTime.UtcNow;
        ActionResult result = await Controller.CreateChapterIssue(createDto);
        DateTime after = DateTime.UtcNow;

        Assert.IsInstanceOfType(result, typeof(CreatedResult));

        ReportedIssueChapter? createdIssue = Context.ReportedIssuesChapter
            .FirstOrDefault(i => i.ChapterId == createDto.ChapterId && i.UserId == 1);
        Assert.IsNotNull(createdIssue);
        Assert.IsTrue(createdIssue.CreatedAt >= before);
        Assert.IsTrue(createdIssue.CreatedAt <= after);
    }

    [TestMethod]
    public async Task CreateSerieIssueWithValidDataCreatesIssue()
    {
        ReportedIssueSerieCreateDto createDto = new()
        {
            SerieId = 3,
            ProblemId = 1
        };

        ActionResult result = await Controller.CreateSerieIssue(createDto);

        Assert.IsInstanceOfType(result, typeof(CreatedResult));

        ReportedIssueSerie? createdIssue = Context.ReportedIssuesSerie
            .FirstOrDefault(i => i.SerieId == createDto.SerieId && i.ProblemId == createDto.ProblemId && i.UserId == 1);
        Assert.IsNotNull(createdIssue);
        Assert.AreEqual(createDto.SerieId, createdIssue.SerieId);
        Assert.AreEqual(createDto.ProblemId, createdIssue.ProblemId);
        Assert.AreEqual(1, createdIssue.UserId);
    }

    [TestMethod]
    public async Task CreateSerieIssueWithoutAuthenticationReturnsUnauthorized()
    {
        Controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        ReportedIssueSerieCreateDto createDto = new()
        {
            SerieId = 1,
            ProblemId = 1
        };

        ActionResult result = await Controller.CreateSerieIssue(createDto);

        Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
    }

    [TestMethod]
    public async Task CreateSerieIssueVerifyCreationDate()
    {
        ReportedIssueSerieCreateDto createDto = new()
        {
            SerieId = 3,
            ProblemId = 2
        };

        DateTime before = DateTime.UtcNow;
        ActionResult result = await Controller.CreateSerieIssue(createDto);
        DateTime after = DateTime.UtcNow;

        Assert.IsInstanceOfType(result, typeof(CreatedResult));

        ReportedIssueSerie? createdIssue = Context.ReportedIssuesSerie
            .FirstOrDefault(i => i.SerieId == createDto.SerieId && i.ProblemId == createDto.ProblemId && i.UserId == 1);
        Assert.IsNotNull(createdIssue);
        Assert.IsTrue(createdIssue.CreatedAt >= before);
        Assert.IsTrue(createdIssue.CreatedAt <= after);
    }

    [TestMethod]
    public async Task CreateChapterIssueVerifyUserIdSetCorrectly()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "2"),
            new Claim(ClaimTypes.Name, "TestAdmin"),
            new Claim(ClaimTypes.Role, "Admin")
        ], "test"));

        Controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user
            }
        };

        ReportedIssueChapterCreateDto createDto = new()
        {
            ChapterId = 1,
            ProblemId = 1
        };

        ActionResult result = await Controller.CreateChapterIssue(createDto);

        Assert.IsInstanceOfType(result, typeof(CreatedResult));

        ReportedIssueChapter? createdIssue = Context.ReportedIssuesChapter
            .FirstOrDefault(i => i.ChapterId == createDto.ChapterId && i.UserId == 2);
        Assert.IsNotNull(createdIssue);
        Assert.AreEqual(2, createdIssue.UserId);
    }

    [TestMethod]
    public async Task CreateChapterIssueMultipleIssuesForSameChapterCreatesMultipleIssues()
    {
        ReportedIssueChapterCreateDto firstDto = new()
        {
            ChapterId = 3,
            ProblemId = 1
        };

        ReportedIssueChapterCreateDto secondDto = new()
        {
            ChapterId = 3,
            ProblemId = 2
        };

        ActionResult firstResult = await Controller.CreateChapterIssue(firstDto);
        ActionResult secondResult = await Controller.CreateChapterIssue(secondDto);

        Assert.IsInstanceOfType(firstResult, typeof(CreatedResult));
        Assert.IsInstanceOfType(secondResult, typeof(CreatedResult));

        int issueCount = Context.ReportedIssuesChapter.Count(i => i.ChapterId == 3 && i.UserId == 1);
        Assert.AreEqual(2, issueCount);
    }

    [TestMethod]
    public async Task CreateSerieIssueMultipleIssuesForSameSerieCreatesMultipleIssues()
    {
        ReportedIssueSerieCreateDto firstDto = new()
        {
            SerieId = 3,
            ProblemId = 1
        };

        ReportedIssueSerieCreateDto secondDto = new()
        {
            SerieId = 3,
            ProblemId = 2
        };

        ActionResult firstResult = await Controller.CreateSerieIssue(firstDto);
        ActionResult secondResult = await Controller.CreateSerieIssue(secondDto);

        Assert.IsInstanceOfType(firstResult, typeof(CreatedResult));
        Assert.IsInstanceOfType(secondResult, typeof(CreatedResult));

        int issueCount = Context.ReportedIssuesSerie.Count(i => i.SerieId == 3 && i.UserId == 1);
        Assert.AreEqual(2, issueCount);
    }

    [TestMethod]
    public async Task CreateChapterIssueWithDuplicateDataReturnsConflict()
    {
        ReportedIssueChapterCreateDto createDto = new()
        {
            ChapterId = 3,
            ProblemId = 3
        };

        ActionResult firstResult = await Controller.CreateChapterIssue(createDto);
        ActionResult secondResult = await Controller.CreateChapterIssue(createDto);

        Assert.IsInstanceOfType(firstResult, typeof(CreatedResult));
        Assert.IsInstanceOfType(secondResult, typeof(ConflictObjectResult));
    }

    [TestMethod]
    public async Task CreateSerieIssueWithDuplicateDataReturnsConflict()
    {
        ReportedIssueSerieCreateDto createDto = new()
        {
            SerieId = 3,
            ProblemId = 3
        };

        ActionResult firstResult = await Controller.CreateSerieIssue(createDto);
        ActionResult secondResult = await Controller.CreateSerieIssue(createDto);

        Assert.IsInstanceOfType(firstResult, typeof(CreatedResult));
        Assert.IsInstanceOfType(secondResult, typeof(ConflictObjectResult));
    }
}