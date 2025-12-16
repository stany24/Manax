namespace ManaxLibrary.Notifications;

public enum NotificationType
{
    PermissionModified = 0,

    LibraryCreated = 10,
    LibraryDeleted = 11,
    LibraryUpdated = 12,

    SerieCreated = 20,
    SerieUpdated = 21,
    SerieDeleted = 22,
    PosterUpdated = 23,

    RankCreated = 30,
    RankUpdated = 31,
    RankDeleted = 32,

    ChapterAdded = 40,
    ChapterUpdated = 41,
    ChapterRemoved = 42,
    ChapterUploadFailed = 43,

    UserCreated = 50,
    UserUpdated = 51,
    UserDeleted = 52,

    ReadCreated = 60,
    ReadDeleted = 61,

    ReportedChapterIssueCreated = 70,
    ReportedChapterIssueDeleted = 71,
    ReportedSerieIssueCreated = 72,
    ReportedSerieIssueDeleted = 73,

    TagCreated = 80,
    TagUpdated = 81,
    TagDeleted = 82,

    PersonCreated = 90,
    PersonUpdated = 91,
    PersonDeleted = 92,

    RoleCreated = 100,
    RoleUpdated = 101,
    RoleDeleted = 102,
    
    RunningTasks = 110,
    FeatureModified = 111,
    Connected = 112
}