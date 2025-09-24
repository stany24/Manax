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
    PosterModified,

    RankCreated,
    RankUpdated,
    RankDeleted,

    ChapterAdded,
    ChapterModified,
    ChapterRemoved,

    UserCreated,
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