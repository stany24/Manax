using ManaxApi.Models;
using ManaxApi.Models.Issue.Internal;
using ManaxApi.Models.Issue.User;

namespace ManaxApi.Services;

internal static class IssueManagerService
{
    private static IServiceScopeFactory _scopeFactory = null!;

    internal static void Initialize(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    internal static bool CreateSerieIssue(long serieId, InternalSerieIssueTypeEnum problem)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ManaxContext context = scope.ServiceProvider.GetRequiredService<ManaxContext>();

        UserSerieIssueType? issueType = context.SerieIssueTypes.FirstOrDefault(i => i.Name == problem.ToString());
        if (issueType == null) return false;

        if (context.InternalSerieIssues.Any(i => i.Problem == problem && i.SerieId == serieId)) return false;

        InternalSerieIssue issue = new()
        {
            SerieId = serieId,
            Problem = problem,
            CreatedAt = DateTime.UtcNow
        };

        context.InternalSerieIssues.Add(issue);
        context.SaveChanges();

        return true;
    }

    internal static bool CreateChapterIssue(long chapterId, InternalChapterIssueTypeEnum problem)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ManaxContext context = scope.ServiceProvider.GetRequiredService<ManaxContext>();

        UserChapterIssueType? issueType = context.ChapterIssueTypes.FirstOrDefault(i => i.Name == problem.ToString());
        if (issueType == null) return false;

        if (context.InternalChapterIssues.Any(i => i.Problem== problem && i.ChapterId == chapterId)) return false;

        InternalChapterIssue issue = new()
        {
            ChapterId = chapterId,
            Problem = problem,
            CreatedAt = DateTime.UtcNow
        };

        context.InternalChapterIssues.Add(issue);
        context.SaveChanges();

        return true;
    }
}