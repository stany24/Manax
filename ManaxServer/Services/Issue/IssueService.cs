using ManaxLibrary.DTO.Issue.Automatic;
using ManaxServer.Models;
using ManaxServer.Models.Issue.Automatic;

namespace ManaxServer.Services.Issue;

public class IssueService(IServiceScopeFactory scopeFactory) : Service, IIssueService
{
    public void CreateSerieIssue(long serieId, AutomaticIssueSerieType problem)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
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
    
    public void ManageSerieIssue(long serieId, AutomaticIssueSerieType problem, bool create)
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

    public void RemoveSerieIssue(long serieId, AutomaticIssueSerieType problem)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        ManaxContext context = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        AutomaticIssueSerie? issue = context.AutomaticIssuesSerie
            .FirstOrDefault(i => i.Problem == problem && i.SerieId == serieId);
        if (issue == null) return;
        context.AutomaticIssuesSerie.Remove(issue);
        context.SaveChanges();
    }

    public void CreateChapterIssue(long chapterId, AutomaticIssueChapterType problem)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
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
    
    public void ManageChapterIssue(long serieId, AutomaticIssueChapterType problem, bool create)
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

    public void RemoveChapterIssue(long serieId, AutomaticIssueChapterType problem)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        ManaxContext context = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        AutomaticIssueChapter? issue = context.AutomaticIssuesChapter
            .FirstOrDefault(i => i.Problem == problem && i.ChapterId == serieId);
        if (issue == null) return;
        context.AutomaticIssuesChapter.Remove(issue);
        context.SaveChanges();
    }
}