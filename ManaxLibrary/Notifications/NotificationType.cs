namespace ManaxLibrary.Notifications;

public enum NotificationType
{
    PermissionModified,
    
    LibraryCreated,
    LibraryDeleted,
    LibraryUpdated,

    SerieCreated,
    SerieUpdated,
    SerieDeleted,
    PosterUpdated,

    RankCreated,
    RankUpdated,
    RankDeleted,

    ChapterAdded,
    ChapterUpdated,
    ChapterRemoved,

    UserCreated,
    UserUpdated,
    UserDeleted,

    RunningTasks,

    Connected,

    ReadCreated,
    ReadDeleted,

    ReportedChapterIssueCreated,
    ReportedChapterIssueDeleted,
    ReportedSerieIssueCreated,
    ReportedSerieIssueDeleted,

    TagCreated,
    TagUpdated,
    TagDeleted
}