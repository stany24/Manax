using ManaxLibrary.DTOs.Issue.Automatic;
using ManaxServer.Models;
using ManaxServer.Models.Issue.Automatic;
using ManaxServer.Models.Issue.Reported;

namespace ManaxServer.Services;

internal static class IssueManagerService
{
    private static IServiceScopeFactory _scopeFactory = null!;

    internal static void Initialize(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    internal static bool CreateSerieIssue(long serieId, AutomaticIssueSerieType problem)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ManaxContext context = scope.ServiceProvider.GetRequiredService<ManaxContext>();

        ReportedIssueSerieType? issueType = context.ReportedIssueSerieTypes.FirstOrDefault(i => i.Name == problem.ToString());
        if (issueType == null) return false;

        if (context.AutomaticIssuesSerie.Any(i => i.Problem == problem && i.SerieId == serieId)) return false;

        AutomaticIssueSerie issue = new()
        {
            SerieId = serieId,
            Problem = problem,
            CreatedAt = DateTime.UtcNow
        };

        context.AutomaticIssuesSerie.Add(issue);
        context.SaveChanges();

        return true;
    }

    internal static bool CreateChapterIssue(long chapterId, AutomaticIssueChapterType problem)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ManaxContext context = scope.ServiceProvider.GetRequiredService<ManaxContext>();

        ReportedIssueChapterType? issueType = context.ReportedIssueChapterTypes.FirstOrDefault(i => i.Name == problem.ToString());
        if (issueType == null) return false;

        if (context.AutomaticIssuesChapter.Any(i => i.Problem== problem && i.ChapterId == chapterId)) return false;

        AutomaticIssueChapter issue = new()
        {
            ChapterId = chapterId,
            Problem = problem,
            CreatedAt = DateTime.UtcNow
        };

        context.AutomaticIssuesChapter.Add(issue);
        context.SaveChanges();

        return true;
    }
}