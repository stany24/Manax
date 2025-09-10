using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.DTO.User;
using ManaxServer.Services.Notification;

namespace ManaxTests.Mocks;

public class MockNotificationService: INotificationService
{
    public void NotifyUserCreatedAsync(UserDto user)
    {
    }

    public void NotifyUserDeletedAsync(long userId)
    {
    }

    public void NotifySerieCreatedAsync(SerieDto serie)
    {
    }

    public void NotifySerieUpdatedAsync(SerieDto serie)
    {
    }

    public void NotifySerieDeletedAsync(long serieId)
    {
    }

    public void NotifyPosterModifiedAsync(long serieId)
    {
    }

    public void NotifyLibraryCreatedAsync(LibraryDto library)
    {
    }

    public void NotifyLibraryUpdatedAsync(LibraryDto library)
    {
    }

    public void NotifyLibraryDeletedAsync(long libraryId)
    {
    }

    public void NotifyChapterAddedAsync(ChapterDto chapter)
    {
    }

    public void NotifyChapterModifiedAsync(ChapterDto chapter)
    {
    }

    public void NotifyChapterRemovedAsync(long chapterId)
    {
    }

    public void NotifyRankCreatedAsync(RankDto rank)
    {
    }

    public void NotifyRankUpdatedAsync(RankDto rank)
    {
    }

    public void NotifyRankDeletedAsync(long rankId)
    {
    }

    public void NotifyRunningTasksAsync(Dictionary<string, int> tasks)
    {
    }

    public void NotifyReadCreated(ReadDto existingRead)
    {
    }

    public void NotifyReadRemoved(ReadDto existingRead)
    {
    }

    public void NotifySerieIssueCreatedAsync(ReportedIssueSerieDto issue)
    {
    }

    public void NotifyChapterIssueCreatedAsync(ReportedIssueChapterDto issue)
    {
    }

    public void NotifyChapterIssueDeletedAsync(long issueId)
    {
    }

    public void NotifySerieIssueDeletedAsync(long issueId)
    {
    }
}