using ManaxLibrary.DTO.Issue.Automatic;
using ManaxServer.Models;
using ManaxServer.Models.Issue.Automatic;

namespace ManaxServer.Services.Issue;

public class IssueService(IServiceScopeFactory scopeFactory) : Service, IIssueService
{
    public void CreateSerieIssue(long serieId, IssueSerieAutomaticType problem)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        ManaxContext context = scope.ServiceProvider.GetRequiredService<ManaxContext>();

        if (context.AutomaticIssuesSerie.Any(i => i.Problem == problem && i.SerieId == serieId)) return;

        AutomaticIssueSerie issue = new()
        {
            SerieId = serieId,
            Problem = problem,
            CreatedAt = DateTime.UtcNow
        };

        context.AutomaticIssuesSerie.Add(issue);
        context.SaveChanges();
    }

    public void ManageSerieIssue(long serieId, IssueSerieAutomaticType problem, bool create)
    {
        if (create)
            CreateSerieIssue(serieId, problem);
        else
            RemoveSerieIssue(serieId, problem);
    }

    public void RemoveSerieIssue(long serieId, IssueSerieAutomaticType problem)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        ManaxContext context = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        AutomaticIssueSerie? issue = context.AutomaticIssuesSerie
            .FirstOrDefault(i => i.Problem == problem && i.SerieId == serieId);
        if (issue == null) return;
        context.AutomaticIssuesSerie.Remove(issue);
        context.SaveChanges();
    }

    public void CreateChapterIssue(long chapterId, IssueChapterAutomaticType problem)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        ManaxContext context = scope.ServiceProvider.GetRequiredService<ManaxContext>();

        if (context.AutomaticIssuesChapter.Any(i => i.Problem == problem && i.ChapterId == chapterId)) return;

        IssueChapterAutomatic issue = new()
        {
            ChapterId = chapterId,
            Problem = problem,
            CreatedAt = DateTime.UtcNow
        };

        context.AutomaticIssuesChapter.Add(issue);
        context.SaveChanges();
    }

    public void ManageChapterIssue(long chapterId, IssueChapterAutomaticType problem, bool create)
    {
        if (create)
            CreateChapterIssue(chapterId, problem);
        else
            RemoveChapterIssue(chapterId, problem);
    }

    public void RemoveChapterIssue(long chapterId, IssueChapterAutomaticType problem)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        ManaxContext context = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        IssueChapterAutomatic? issue = context.AutomaticIssuesChapter
            .FirstOrDefault(i => i.Problem == problem && i.ChapterId == chapterId);
        if (issue == null) return;
        context.AutomaticIssuesChapter.Remove(issue);
        context.SaveChanges();
    }
}