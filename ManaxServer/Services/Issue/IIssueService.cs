using ManaxLibrary.DTO.Issue.Automatic;

namespace ManaxServer.Services.Issue;

public interface IIssueService
{
    void CreateSerieIssue(long serieId, IssueSerieAutomaticType problem);
    void ManageSerieIssue(long serieId, IssueSerieAutomaticType problem, bool create);
    void RemoveSerieIssue(long serieId, IssueSerieAutomaticType problem);

    void CreateChapterIssue(long chapterId, IssueChapterAutomaticType problem);
    void ManageChapterIssue(long chapterId, IssueChapterAutomaticType problem, bool create);
    void RemoveChapterIssue(long chapterId, IssueChapterAutomaticType problem);
}