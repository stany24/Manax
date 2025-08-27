using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxServer.Models.Issue.Automatic;
using ManaxServer.Models.Issue.Reported;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.IssueTests;

[TestClass]
public class GetIssueTests:IssueTestsSetup
{
    [TestMethod]
    public async Task GetAllAutomaticChapterIssues_ReturnsAllIssues()
    {
        ActionResult<IEnumerable<AutomaticIssueChapterDto>> result = await Controller.GetAllAutomaticChapterIssues();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<AutomaticIssueChapterDto>? returnedIssues = result.Value as List<AutomaticIssueChapterDto>;
        Assert.IsNotNull(returnedIssues);
        Assert.AreEqual(Context.AutomaticIssuesChapter.Count(), returnedIssues.Count);

        foreach (AutomaticIssueChapter issue in Context.AutomaticIssuesChapter)
        {
            AutomaticIssueChapterDto? returnedIssue = returnedIssues.FirstOrDefault(i => i.ChapterId == issue.ChapterId);
            Assert.IsNotNull(returnedIssue);
            Assert.AreEqual(issue.Problem, returnedIssue.Problem);
        }
    }

    [TestMethod]
    public async Task GetAllAutomaticSerieIssues_ReturnsAllIssues()
    {
        ActionResult<IEnumerable<AutomaticIssueSerieDto>> result = await Controller.GetAllAutomaticSerieIssues();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<AutomaticIssueSerieDto>? returnedIssues = result.Value as List<AutomaticIssueSerieDto>;
        Assert.IsNotNull(returnedIssues);
        Assert.AreEqual(Context.AutomaticIssuesSerie.Count(), returnedIssues.Count);

        foreach (AutomaticIssueSerie issue in Context.AutomaticIssuesSerie)
        {
            AutomaticIssueSerieDto? returnedIssue = returnedIssues.FirstOrDefault(i => i.SerieId == issue.SerieId);
            Assert.IsNotNull(returnedIssue);
            Assert.AreEqual(issue.Problem, returnedIssue.Problem);
        }
    }

    [TestMethod]
    public async Task GetAllReportedChapterIssues_ReturnsAllIssues()
    {
        ActionResult<IEnumerable<ReportedIssueChapterDto>> result = await Controller.GetAllReportedChapterIssues();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<ReportedIssueChapterDto>? returnedIssues = result.Value as List<ReportedIssueChapterDto>;
        Assert.IsNotNull(returnedIssues);
        Assert.AreEqual(Context.ReportedIssuesChapter.Count(), returnedIssues.Count);

        foreach (ReportedIssueChapter issue in Context.ReportedIssuesChapter)
        {
            ReportedIssueChapterDto? returnedIssue = returnedIssues.FirstOrDefault(i => i.Id == issue.Id);
            Assert.IsNotNull(returnedIssue);
            Assert.AreEqual(issue.ChapterId, returnedIssue.ChapterId);
            Assert.AreEqual(issue.UserId, returnedIssue.UserId);
            Assert.AreEqual(issue.ProblemId, returnedIssue.ProblemId);
        }
    }

    [TestMethod]
    public async Task GetAllReportedChapterIssuesTypes_ReturnsAllTypes()
    {
        List<ReportedIssueChapterTypeDto> result = await Controller.GetAllReportedChapterIssuesTypes();

        Assert.IsNotNull(result);
        Assert.AreEqual(Context.ReportedIssueChapterTypes.Count(), result.Count);

        foreach (ReportedIssueChapterType type in Context.ReportedIssueChapterTypes)
        {
            ReportedIssueChapterTypeDto? returnedType = result.FirstOrDefault(t => t.Id == type.Id);
            Assert.IsNotNull(returnedType);
            Assert.AreEqual(type.Name, returnedType.Name);
        }
    }

    [TestMethod]
    public async Task GetAllReportedSerieIssues_ReturnsAllIssues()
    {
        ActionResult<IEnumerable<ReportedIssueSerieDto>> result = await Controller.GetAllReportedSerieIssues();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<ReportedIssueSerieDto>? returnedIssues = result.Value as List<ReportedIssueSerieDto>;
        Assert.IsNotNull(returnedIssues);
        Assert.AreEqual(Context.ReportedIssuesSerie.Count(), returnedIssues.Count);

        foreach (ReportedIssueSerie issue in Context.ReportedIssuesSerie)
        {
            ReportedIssueSerieDto? returnedIssue = returnedIssues.FirstOrDefault(i => i.Id == issue.Id);
            Assert.IsNotNull(returnedIssue);
            Assert.AreEqual(issue.SerieId, returnedIssue.SerieId);
            Assert.AreEqual(issue.UserId, returnedIssue.UserId);
            Assert.AreEqual(issue.ProblemId, returnedIssue.ProblemId);
        }
    }

    [TestMethod]
    public async Task GetAllReportedSerieIssuesTypes_ReturnsAllTypes()
    {
        ActionResult<IEnumerable<ReportedIssueSerieType>> result = await Controller.GetAllReportedSerieIssuesTypes();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        List<ReportedIssueSerieType>? returnedTypes = result.Value as List<ReportedIssueSerieType>;
        Assert.IsNotNull(returnedTypes);
        Assert.AreEqual(Context.ReportedIssueSerieTypes.Count(), returnedTypes.Count);

        foreach (ReportedIssueSerieType type in Context.ReportedIssueSerieTypes)
        {
            ReportedIssueSerieType? returnedType = returnedTypes.FirstOrDefault(t => t.Id == type.Id);
            Assert.IsNotNull(returnedType);
            Assert.AreEqual(type.Name, returnedType.Name);
        }
    }

    [TestMethod]
    public async Task GetAllAutomaticChapterIssues_VerifyCorrectMapping()
    {
        ActionResult<IEnumerable<AutomaticIssueChapterDto>> result = await Controller.GetAllAutomaticChapterIssues();

        List<AutomaticIssueChapterDto>? returnedIssues = result.Value as List<AutomaticIssueChapterDto>;
        Assert.IsNotNull(returnedIssues);

        AutomaticIssueChapter expectedIssue = Context.AutomaticIssuesChapter.First();
        AutomaticIssueChapterDto? returnedIssue = returnedIssues.FirstOrDefault(i => i.ChapterId == expectedIssue.ChapterId);
        Assert.IsNotNull(returnedIssue);
        Assert.AreEqual(expectedIssue.ChapterId, returnedIssue.ChapterId);
        Assert.AreEqual(expectedIssue.Problem, returnedIssue.Problem);
        Assert.AreEqual(expectedIssue.CreatedAt, returnedIssue.CreatedAt);
    }

    [TestMethod]
    public async Task GetAllAutomaticSerieIssues_VerifyCorrectMapping()
    {
        ActionResult<IEnumerable<AutomaticIssueSerieDto>> result = await Controller.GetAllAutomaticSerieIssues();

        List<AutomaticIssueSerieDto>? returnedIssues = result.Value as List<AutomaticIssueSerieDto>;
        Assert.IsNotNull(returnedIssues);

        AutomaticIssueSerie expectedIssue = Context.AutomaticIssuesSerie.First();
        AutomaticIssueSerieDto? returnedIssue = returnedIssues.FirstOrDefault(i => i.SerieId == expectedIssue.SerieId);
        Assert.IsNotNull(returnedIssue);
        Assert.AreEqual(expectedIssue.SerieId, returnedIssue.SerieId);
        Assert.AreEqual(expectedIssue.Problem, returnedIssue.Problem);
        Assert.AreEqual(expectedIssue.CreatedAt, returnedIssue.CreatedAt);
    }

    [TestMethod]
    public async Task GetAllReportedChapterIssuesTypes_VerifyCorrectCount()
    {
        List<ReportedIssueChapterTypeDto> result = await Controller.GetAllReportedChapterIssuesTypes();

        Assert.IsNotNull(result);
        Assert.HasCount(3, result);
    }

    [TestMethod]
    public async Task GetAllReportedSerieIssuesTypes_VerifyCorrectCount()
    {
        ActionResult<IEnumerable<ReportedIssueSerieType>> result = await Controller.GetAllReportedSerieIssuesTypes();

        List<ReportedIssueSerieType>? returnedTypes = result.Value as List<ReportedIssueSerieType>;
        Assert.IsNotNull(returnedTypes);
        Assert.AreEqual(3, returnedTypes.Count);
    }
}
