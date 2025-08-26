using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.DTO.User;

namespace ManaxServer.Services.Notification;

public interface INotificationService
{
    void NotifyUserCreatedAsync(UserDto user);
    void NotifyUserDeletedAsync(long userId);

    void NotifySerieCreatedAsync(SerieDto serie);
    void NotifySerieUpdatedAsync(SerieDto serie);
    void NotifySerieDeletedAsync(long serieId);

    void NotifyPosterModifiedAsync(long serieId);

    void NotifyLibraryCreatedAsync(LibraryDto library);
    void NotifyLibraryUpdatedAsync(LibraryDto library);
    void NotifyLibraryDeletedAsync(long libraryId);

    void NotifyChapterAddedAsync(ChapterDto chapter);
    void NotifyChapterRemovedAsync(long chapterId);

    void NotifyRankCreatedAsync(RankDto rank);
    void NotifyRankUpdatedAsync(RankDto rank);
    void NotifyRankDeletedAsync(long rankId);

    void NotifyRunningTasksAsync(Dictionary<string, int> tasks);
    void NotifyReadCreated(ReadDto existingRead);
    void NotifyReadRemoved(ReadDto existingRead);
    
    void NotifySerieIssueCreatedAsync(ReportedIssueSerieDto issue);
    void NotifyChapterIssueCreatedAsync(ReportedIssueChapterDto issue);
    void NotifyChapterIssueDeletedAsync(long issueId);
    void NotifySerieIssueDeletedAsync(long issueId);
}