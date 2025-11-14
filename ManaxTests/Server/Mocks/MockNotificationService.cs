using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Feature;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.DTO.Person;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.DTO.Role;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.DTO.Tag;
using ManaxLibrary.DTO.User;
using ManaxServer.Services.Notification;

namespace ManaxTests.Server.Mocks;

public class MockNotificationService : INotificationService
{
    public UserDto? UserCreated { get; private set; }
    public DateTime UserCreatedAt { get; private set; }
    public ReadDto? ReadCreated { get; private set; }
    public TagDto? TagCreated { get; private set; }
    public TagDto? TagUpdated { get; private set; }
    public long TagDeletedId { get; private set; }
    public Feature? FeatureChanged { get; set; }

    public void NotifyPermissionModifiedAsync(long userId, List<Permission> permissions)
    {
    }

    public void NotifyUserCreatedAsync(UserDto user)
    {
        UserCreated = user;
        UserCreatedAt = DateTime.UtcNow;
    }

    public void NotifyUserUpdatedAsync(UserDto user)
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

    public void NotifyPosterUpdatedAsync(long serieId)
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
        ReadCreated = existingRead;
    }

    public void NotifyReadRemoved(ReadDto existingRead)
    {
    }

    public void NotifySerieIssueCreatedAsync(IssueSerieReportedDto issue)
    {
    }

    public void NotifyChapterIssueCreatedAsync(IssueChapterReportedDto issue)
    {
    }

    public void NotifyChapterIssueDeletedAsync(long issueId)
    {
    }

    public void NotifySerieIssueDeletedAsync(long issueId)
    {
    }

    public void NotifyTagCreatedAsync(TagDto tag)
    {
        TagCreated = tag;
    }

    public void NotifyTagUpdatedAsync(TagDto tag)
    {
        TagUpdated = tag;
    }

    public void NotifyTagDeletedAsync(long tagId)
    {
        TagDeletedId = tagId;
    }

    public void NotifyFeatureChanged(Feature feature)
    {
        FeatureChanged = feature;
    }

    public void NotifyPersonCreatedAsync(PersonDto person)
    {
    }

    public void NotifyPersonUpdatedAsync(PersonDto person)
    {
    }

    public void NotifyPersonDeletedAsync(long personId)
    {
    }

    public void NotifyRoleCreatedAsync(RoleDto role)
    {
    }

    public void NotifyRoleUpdatedAsync(RoleDto role)
    {
    }

    public void NotifyRoleDeletedAsync(long roleId)
    {
    }
}