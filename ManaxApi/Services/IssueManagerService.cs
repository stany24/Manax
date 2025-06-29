// Fichier : ManaxApi/Services/IssueManagerService.cs

using ManaxApi.Models;
using ManaxApi.Models.Issue;

namespace ManaxApi.Services;

internal static class IssueManagerService
{
    private static IServiceScopeFactory _scopeFactory = null!;

    internal static void Initialize(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    internal static bool CreateSerieIssue(long serieId, SerieIssueTypeEnum problem)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ManaxContext context = scope.ServiceProvider.GetRequiredService<ManaxContext>();

        SerieIssueType? issueType = context.SerieIssueTypes.FirstOrDefault(i => i.Name == problem.ToString());
        if (issueType == null) return false;
        
        if (context.SerieIssues.Any(i => i.ProblemId == issueType.Id && i.SerieId == serieId))
        {
            return false;
        }

        SerieIssue issue = new()
        {
            SerieId = serieId,
            ProblemId = issueType.Id,
            CreatedAt = DateTime.UtcNow
        };

        context.SerieIssues.Add(issue);
        context.SaveChanges();

        return true;
    }

    internal static bool CreateChapterIssue(long chapterId, ChapterIssueTypeEnum problem)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ManaxContext context = scope.ServiceProvider.GetRequiredService<ManaxContext>();

        ChapterIssueType? issueType = context.ChapterIssueTypes.FirstOrDefault(i => i.Name == problem.ToString());
        if (issueType == null) return false;
        
        if (context.ChapterIssues.Any(i => i.ProblemId == issueType.Id && i.ChapterId == chapterId))
        {
            return false;
        }

        ChapterIssue issue = new()
        {
            ChapterId = chapterId,
            ProblemId = issueType.Id,
            CreatedAt = DateTime.UtcNow
        };

        context.ChapterIssues.Add(issue);
        context.SaveChanges();

        return true;
    }
}