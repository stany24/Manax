using ManaxServer.Models.Issue.Reported;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.IssueTests;

[TestClass]
public class CloseIssueTests : IssueTestsSetup
{
    [TestMethod]
    public async Task CloseChapterIssueWithValidIdRemovesIssue()
    {
        ReportedIssueChapter issue = Context.ReportedIssuesChapter.First();
        IActionResult result = await Controller.CloseChapterIssue(issue.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        ReportedIssueChapter? deletedIssue = await Context.ReportedIssuesChapter.FindAsync(issue.Id);
        Assert.IsNull(deletedIssue);
    }

    [TestMethod]
    public async Task CloseChapterIssueWithInvalidIdReturnsNotFound()
    {
        IActionResult result = await Controller.CloseChapterIssue(999999);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task CloseChapterIssueVerifyIssueCountDecreases()
    {
        int initialCount = Context.ReportedIssuesChapter.Count();
        ReportedIssueChapter issue = Context.ReportedIssuesChapter.First();

        IActionResult result = await Controller.CloseChapterIssue(issue.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        int finalCount = Context.ReportedIssuesChapter.Count();
        Assert.AreEqual(initialCount - 1, finalCount);
    }

    [TestMethod]
    public async Task CloseSerieIssueWithValidIdRemovesIssue()
    {
        ReportedIssueSerie issue = Context.ReportedIssuesSerie.First();
        IActionResult result = await Controller.CloseSerieIssue(issue.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        ReportedIssueSerie? deletedIssue = await Context.ReportedIssuesSerie.FindAsync(issue.Id);
        Assert.IsNull(deletedIssue);
    }

    [TestMethod]
    public async Task CloseSerieIssueWithInvalidIdReturnsNotFound()
    {
        IActionResult result = await Controller.CloseSerieIssue(999999);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task CloseSerieIssueVerifyIssueCountDecreases()
    {
        int initialCount = Context.ReportedIssuesSerie.Count();
        ReportedIssueSerie issue = Context.ReportedIssuesSerie.First();

        IActionResult result = await Controller.CloseSerieIssue(issue.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        int finalCount = Context.ReportedIssuesSerie.Count();
        Assert.AreEqual(initialCount - 1, finalCount);
    }

    [TestMethod]
    public async Task CloseChapterIssueOnlyRemovesSpecificIssue()
    {
        ReportedIssueChapter issueToDelete = Context.ReportedIssuesChapter.First();
        ReportedIssueChapter otherIssue = Context.ReportedIssuesChapter.First(i => i.Id != issueToDelete.Id);

        IActionResult result = await Controller.CloseChapterIssue(issueToDelete.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        ReportedIssueChapter? deletedIssue = await Context.ReportedIssuesChapter.FindAsync(issueToDelete.Id);
        Assert.IsNull(deletedIssue);

        ReportedIssueChapter? remainingIssue = await Context.ReportedIssuesChapter.FindAsync(otherIssue.Id);
        Assert.IsNotNull(remainingIssue);
    }

    [TestMethod]
    public async Task CloseSerieIssueOnlyRemovesSpecificIssue()
    {
        ReportedIssueSerie issueToDelete = Context.ReportedIssuesSerie.First();
        ReportedIssueSerie otherIssue = Context.ReportedIssuesSerie.First(i => i.Id != issueToDelete.Id);

        IActionResult result = await Controller.CloseSerieIssue(issueToDelete.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        ReportedIssueSerie? deletedIssue = await Context.ReportedIssuesSerie.FindAsync(issueToDelete.Id);
        Assert.IsNull(deletedIssue);

        ReportedIssueSerie? remainingIssue = await Context.ReportedIssuesSerie.FindAsync(otherIssue.Id);
        Assert.IsNotNull(remainingIssue);
    }
}