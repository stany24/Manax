using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxServer.Models.Issue.Automatic;
using ManaxServer.Models.Issue.Reported;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.IssueTests;

[TestClass]
public class GetIssueTests : IssueTestsSetup
{
    [TestMethod]
    public async Task GetAllAutomaticChapterIssuesReturnsAllIssues()
    {
        ActionResult<IEnumerable<IssueChapterAutomaticDto>> result = await Controller.GetAllAutomaticChapterIssues();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<IssueChapterAutomaticDto>? returnedIssues = result.Value as List<IssueChapterAutomaticDto>;
        Assert.IsNotNull(returnedIssues);
        Assert.AreEqual(Context.AutomaticIssuesChapter.Count(), returnedIssues.Count);

        foreach (IssueChapterAutomatic issue in Context.AutomaticIssuesChapter)
        {
            IssueChapterAutomaticDto? returnedIssue =
                returnedIssues.FirstOrDefault(i => i.ChapterId == issue.ChapterId);
            Assert.IsNotNull(returnedIssue);
            Assert.AreEqual(issue.Problem, returnedIssue.Problem);
        }
    }

    [TestMethod]
    public async Task GetAllAutomaticSerieIssuesReturnsAllIssues()
    {
        ActionResult<IEnumerable<IssueSerieAutomaticDto>> result = await Controller.GetAllAutomaticSerieIssues();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<IssueSerieAutomaticDto>? returnedIssues = result.Value as List<IssueSerieAutomaticDto>;
        Assert.IsNotNull(returnedIssues);
        Assert.AreEqual(Context.AutomaticIssuesSerie.Count(), returnedIssues.Count);

        foreach (AutomaticIssueSerie issue in Context.AutomaticIssuesSerie)
        {
            IssueSerieAutomaticDto? returnedIssue = returnedIssues.FirstOrDefault(i => i.SerieId == issue.SerieId);
            Assert.IsNotNull(returnedIssue);
            Assert.AreEqual(issue.Problem, returnedIssue.Problem);
        }
    }

    [TestMethod]
    public async Task GetAllReportedChapterIssuesReturnsAllIssues()
    {
        ActionResult<IEnumerable<IssueChapterReportedDto>> result = await Controller.GetAllReportedChapterIssues();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<IssueChapterReportedDto>? returnedIssues = result.Value as List<IssueChapterReportedDto>;
        Assert.IsNotNull(returnedIssues);
        Assert.AreEqual(Context.ReportedIssuesChapter.Count(), returnedIssues.Count);

        foreach (IssueChapterReported issue in Context.ReportedIssuesChapter)
        {
            IssueChapterReportedDto? returnedIssue = returnedIssues.FirstOrDefault(i => i.Id == issue.Id);
            Assert.IsNotNull(returnedIssue);
            Assert.AreEqual(issue.ChapterId, returnedIssue.ChapterId);
            Assert.AreEqual(issue.UserId, returnedIssue.UserId);
            Assert.AreEqual(issue.ProblemId, returnedIssue.ProblemId);
        }
    }

    [TestMethod]
    public async Task GetAllReportedChapterIssuesTypesReturnsAllTypes()
    {
        List<IssueChapterReportedTypeDto> result = await Controller.GetAllReportedChapterIssuesTypes();

        Assert.IsNotNull(result);
        Assert.AreEqual(Context.ReportedIssueChapterTypes.Count(), result.Count);

        foreach (IssueChapterReportedType type in Context.ReportedIssueChapterTypes)
        {
            IssueChapterReportedTypeDto? returnedType = result.FirstOrDefault(t => t.Id == type.Id);
            Assert.IsNotNull(returnedType);
            Assert.AreEqual(type.Name, returnedType.Name);
        }
    }

    [TestMethod]
    public async Task GetAllReportedSerieIssuesReturnsAllIssues()
    {
        ActionResult<IEnumerable<IssueSerieReportedDto>> result = await Controller.GetAllReportedSerieIssues();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<IssueSerieReportedDto>? returnedIssues = result.Value as List<IssueSerieReportedDto>;
        Assert.IsNotNull(returnedIssues);
        Assert.AreEqual(Context.ReportedIssuesSerie.Count(), returnedIssues.Count);

        foreach (IssueSerieReported issue in Context.ReportedIssuesSerie)
        {
            IssueSerieReportedDto? returnedIssue = returnedIssues.FirstOrDefault(i => i.Id == issue.Id);
            Assert.IsNotNull(returnedIssue);
            Assert.AreEqual(issue.SerieId, returnedIssue.SerieId);
            Assert.AreEqual(issue.UserId, returnedIssue.UserId);
            Assert.AreEqual(issue.ProblemId, returnedIssue.ProblemId);
        }
    }

    [TestMethod]
    public async Task GetAllReportedSerieIssuesTypesReturnsAllTypes()
    {
        ActionResult<IEnumerable<IssueSerieReportedTypeDto>> result = await Controller.GetAllReportedSerieIssuesTypes();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<IssueSerieReportedTypeDto>? returnedTypes = result.Value as List<IssueSerieReportedTypeDto>;
        Assert.IsNotNull(returnedTypes);
        Assert.AreEqual(Context.ReportedIssueSerieTypes.Count(), returnedTypes.Count);

        foreach (IssueSerieReportedType type in Context.ReportedIssueSerieTypes)
        {
            IssueSerieReportedTypeDto? returnedType = returnedTypes.FirstOrDefault(t => t.Id == type.Id);
            Assert.IsNotNull(returnedType);
            Assert.AreEqual(type.Name, returnedType.Name);
        }
    }

    [TestMethod]
    public async Task GetAllAutomaticChapterIssuesVerifyCorrectMapping()
    {
        ActionResult<IEnumerable<IssueChapterAutomaticDto>> result = await Controller.GetAllAutomaticChapterIssues();

        List<IssueChapterAutomaticDto>? returnedIssues = result.Value as List<IssueChapterAutomaticDto>;
        Assert.IsNotNull(returnedIssues);

        IssueChapterAutomatic expectedIssue = Context.AutomaticIssuesChapter.First();
        IssueChapterAutomaticDto? returnedIssue =
            returnedIssues.FirstOrDefault(i => i.ChapterId == expectedIssue.ChapterId);
        Assert.IsNotNull(returnedIssue);
        Assert.AreEqual(expectedIssue.ChapterId, returnedIssue.ChapterId);
        Assert.AreEqual(expectedIssue.Problem, returnedIssue.Problem);
        Assert.AreEqual(expectedIssue.CreatedAt, returnedIssue.CreatedAt);
    }

    [TestMethod]
    public async Task GetAllAutomaticSerieIssuesVerifyCorrectMapping()
    {
        ActionResult<IEnumerable<IssueSerieAutomaticDto>> result = await Controller.GetAllAutomaticSerieIssues();

        List<IssueSerieAutomaticDto>? returnedIssues = result.Value as List<IssueSerieAutomaticDto>;
        Assert.IsNotNull(returnedIssues);

        AutomaticIssueSerie expectedIssue = Context.AutomaticIssuesSerie.First();
        IssueSerieAutomaticDto? returnedIssue = returnedIssues.FirstOrDefault(i => i.SerieId == expectedIssue.SerieId);
        Assert.IsNotNull(returnedIssue);
        Assert.AreEqual(expectedIssue.SerieId, returnedIssue.SerieId);
        Assert.AreEqual(expectedIssue.Problem, returnedIssue.Problem);
        Assert.AreEqual(expectedIssue.CreatedAt, returnedIssue.CreatedAt);
    }

    [TestMethod]
    public async Task GetAllReportedChapterIssuesTypesVerifyCorrectCount()
    {
        List<IssueChapterReportedTypeDto> result = await Controller.GetAllReportedChapterIssuesTypes();

        Assert.IsNotNull(result);
        Assert.HasCount(3, result);
    }

    [TestMethod]
    public async Task GetAllReportedSerieIssuesTypesVerifyCorrectCount()
    {
        ActionResult<IEnumerable<IssueSerieReportedTypeDto>> result = await Controller.GetAllReportedSerieIssuesTypes();

        List<IssueSerieReportedTypeDto>? returnedTypes = result.Value as List<IssueSerieReportedTypeDto>;
        Assert.IsNotNull(returnedTypes);
        Assert.HasCount(3, returnedTypes);
    }
}