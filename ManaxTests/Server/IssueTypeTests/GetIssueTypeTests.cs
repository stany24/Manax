using ManaxLibrary.DTO.Issue.Reported;
using ManaxServer.Models.Issue.Reported;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.IssueTypeTests;

[TestClass]
public class GetIssueTypeTests : IssueTypeTestsSetup
{
    [TestMethod]
    public async Task GetAllReportedChapterIssuesTypesReturnsAllTypes()
    {
        ActionResult<IEnumerable<IssueChapterReportedTypeDto>> result =
            await Controller.GetAllReportedChapterIssuesTypes();
        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);

        List<IssueChapterReportedTypeDto>? returnedTypes = okResult.Value as List<IssueChapterReportedTypeDto>;
        Assert.IsNotNull(returnedTypes);
        Assert.HasCount(Context.ReportedIssueChapterTypes.Count(), returnedTypes);

        foreach (IssueChapterReportedType type in Context.ReportedIssueChapterTypes)
        {
            IssueChapterReportedTypeDto? returnedType = returnedTypes.FirstOrDefault(t => t.Id == type.Id);
            Assert.IsNotNull(returnedType);
            Assert.AreEqual(type.Name, returnedType.Name);
        }
    }

    [TestMethod]
    public async Task GetAllReportedSerieIssuesTypesReturnsAllTypes()
    {
        ActionResult<IEnumerable<IssueSerieReportedTypeDto>> result = await Controller.GetAllReportedSerieIssuesTypes();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);

        List<IssueSerieReportedTypeDto>? returnedTypes = okResult.Value as List<IssueSerieReportedTypeDto>;
        Assert.IsNotNull(returnedTypes);
        Assert.HasCount(Context.ReportedIssueSerieTypes.Count(), returnedTypes);

        foreach (IssueSerieReportedType type in Context.ReportedIssueSerieTypes)
        {
            IssueSerieReportedTypeDto? returnedType = returnedTypes.FirstOrDefault(t => t.Id == type.Id);
            Assert.IsNotNull(returnedType);
            Assert.AreEqual(type.Name, returnedType.Name);
        }
    }

    [TestMethod]
    public async Task GetAllReportedChapterIssuesTypesVerifyCorrectCount()
    {
        ActionResult<IEnumerable<IssueChapterReportedTypeDto>> result =
            await Controller.GetAllReportedChapterIssuesTypes();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);

        List<IssueChapterReportedTypeDto>? returnedTypes = okResult.Value as List<IssueChapterReportedTypeDto>;
        Assert.IsNotNull(returnedTypes);
        Assert.HasCount(3, returnedTypes);
    }

    [TestMethod]
    public async Task GetAllReportedSerieIssuesTypesVerifyCorrectCount()
    {
        ActionResult<IEnumerable<IssueSerieReportedTypeDto>> result = await Controller.GetAllReportedSerieIssuesTypes();

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);

        List<IssueSerieReportedTypeDto>? returnedTypes = okResult.Value as List<IssueSerieReportedTypeDto>;
        Assert.IsNotNull(returnedTypes);
        Assert.HasCount(3, returnedTypes);
    }
}