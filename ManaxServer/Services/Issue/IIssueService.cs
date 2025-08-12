// filepath: /media/Pgm3/Git/Manax/ManaxServer/Services/IIssueService.cs

using ManaxLibrary.DTO.Issue.Automatic;

namespace ManaxServer.Services.Issue;

public interface IIssueService
{
    void CreateSerieIssue(long serieId, AutomaticIssueSerieType problem);
    void ManageSerieIssue(long serieId, AutomaticIssueSerieType problem, bool create);
    void RemoveSerieIssue(long serieId, AutomaticIssueSerieType problem);
    
    void CreateChapterIssue(long chapterId, AutomaticIssueChapterType problem);
    void ManageChapterIssue(long chapterId, AutomaticIssueChapterType problem, bool create);
    void RemoveChapterIssue(long chapterId, AutomaticIssueChapterType problem);
}
