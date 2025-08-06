using ManaxLibrary.DTO.Issue.Automatic;
using ManaxServer.Models;
using ManaxServer.Models.Issue.Automatic;

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
    
    internal static void ManageSerieIssue(long serieId, AutomaticIssueSerieType problem, bool create)
    {
        if (create)
        {
            CreateSerieIssue(serieId, problem);
        }
        else
        {
            RemoveSerieIssue(serieId, problem);
        }
    }

    internal static void RemoveSerieIssue(long serieId, AutomaticIssueSerieType problem)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ManaxContext context = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        AutomaticIssueSerie? issue = context.AutomaticIssuesSerie
            .FirstOrDefault(i => i.Problem == problem && i.SerieId == serieId);
        if (issue == null) return;
        context.AutomaticIssuesSerie.Remove(issue);
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
    
    internal static void ManageChapterIssue(long serieId, AutomaticIssueChapterType problem, bool create)
    {
        if (create)
        {
            CreateChapterIssue(serieId, problem);
        }
        else
        {
            RemoveChapterIssue(serieId, problem);
        }
    }

    internal static void RemoveChapterIssue(long serieId, AutomaticIssueChapterType problem)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ManaxContext context = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        AutomaticIssueChapter? issue = context.AutomaticIssuesChapter
            .FirstOrDefault(i => i.Problem == problem && i.ChapterId == serieId);
        if (issue == null) return;
        context.AutomaticIssuesChapter.Remove(issue);
        context.SaveChanges();
    }
}