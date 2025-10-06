using ManaxServer.Models.Issue.Reported;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.IssueTests;

[TestClass]
public class CloseIssueTests : IssueTestsSetup
{
    [TestMethod]
    public async Task CloseChapterIssueWithValidIdRemovesIssue()
    {
        IssueChapterReported issue = Context.ReportedIssuesChapter.First();
        IActionResult result = await Controller.CloseChapterIssue(issue.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        IssueChapterReported? deletedIssue = await Context.ReportedIssuesChapter.FindAsync(issue.Id);
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
        IssueChapterReported issue = Context.ReportedIssuesChapter.First();

        IActionResult result = await Controller.CloseChapterIssue(issue.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        int finalCount = Context.ReportedIssuesChapter.Count();
        Assert.AreEqual(initialCount - 1, finalCount);
    }

    [TestMethod]
    public async Task CloseSerieIssueWithValidIdRemovesIssue()
    {
        IssueSerieReported issue = Context.ReportedIssuesSerie.First();
        IActionResult result = await Controller.CloseSerieIssue(issue.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        IssueSerieReported? deletedIssue = await Context.ReportedIssuesSerie.FindAsync(issue.Id);
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
        IssueSerieReported issue = Context.ReportedIssuesSerie.First();

        IActionResult result = await Controller.CloseSerieIssue(issue.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        int finalCount = Context.ReportedIssuesSerie.Count();
        Assert.AreEqual(initialCount - 1, finalCount);
    }

    [TestMethod]
    public async Task CloseChapterIssueOnlyRemovesSpecificIssue()
    {
        IssueChapterReported issueToDelete = Context.ReportedIssuesChapter.First();
        IssueChapterReported otherIssue = Context.ReportedIssuesChapter.First(i => i.Id != issueToDelete.Id);

        IActionResult result = await Controller.CloseChapterIssue(issueToDelete.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        IssueChapterReported? deletedIssue = await Context.ReportedIssuesChapter.FindAsync(issueToDelete.Id);
        Assert.IsNull(deletedIssue);

        IssueChapterReported? remainingIssue = await Context.ReportedIssuesChapter.FindAsync(otherIssue.Id);
        Assert.IsNotNull(remainingIssue);
    }

    [TestMethod]
    public async Task CloseSerieIssueOnlyRemovesSpecificIssue()
    {
        IssueSerieReported issueToDelete = Context.ReportedIssuesSerie.First();
        IssueSerieReported otherIssue = Context.ReportedIssuesSerie.First(i => i.Id != issueToDelete.Id);

        IActionResult result = await Controller.CloseSerieIssue(issueToDelete.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        IssueSerieReported? deletedIssue = await Context.ReportedIssuesSerie.FindAsync(issueToDelete.Id);
        Assert.IsNull(deletedIssue);

        IssueSerieReported? remainingIssue = await Context.ReportedIssuesSerie.FindAsync(otherIssue.Id);
        Assert.IsNotNull(remainingIssue);
    }
}