using ManaxLibrary.DTOs.Issue.Automatic;
using ManaxLibrary.Logging;
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

    internal static void CreateSerieIssue(long serieId, AutomaticIssueSerieType problem)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ManaxContext context = scope.ServiceProvider.GetRequiredService<ManaxContext>();

        if (context.AutomaticIssuesSerie.Any(i => i.Problem == problem && i.SerieId == serieId)) {return;}

        AutomaticIssueSerie issue = new()
        {
            SerieId = serieId,
            Problem = problem,
            CreatedAt = DateTime.UtcNow
        };

        context.AutomaticIssuesSerie.Add(issue);
        context.SaveChanges();
    }

    internal static void CreateChapterIssue(long chapterId, AutomaticIssueChapterType problem)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ManaxContext context = scope.ServiceProvider.GetRequiredService<ManaxContext>();

        if (context.AutomaticIssuesChapter.Any(i => i.Problem== problem && i.ChapterId == chapterId)) return;

        AutomaticIssueChapter issue = new()
        {
            ChapterId = chapterId,
            Problem = problem,
            CreatedAt = DateTime.UtcNow
        };

        context.AutomaticIssuesChapter.Add(issue);
        context.SaveChanges();
    }
}