namespace ManaxLibrary;

public enum ErrorCode
{
    ChapterDoesNotExist = 1,
    ChapterFileDoesNotExist = 2,
    PageDoesNotExist = 3,
    ChapterAlreadyExists = 4,
    InvalidChapterData = 5,
    ChapterFileAlreadyExists = 6,

    SerieDoesNotExist = 11,
    PosterDoesNotExist = 12,
    SerieHasNoPoster = 13,
    SerieHasNoBanner = 14,
    SerieAlreadyExists = 15,
    InvalidSerieData = 16,

    UserDoesNotExist = 21,
    InvalidPassword = 22,
    CannotDeleteSelf = 23,
    InsufficientPermissions = 24,

    LibraryDoesNotExist = 31,
    InvalidLibraryData = 32,

    TagDoesNotExist = 41,
    InvalidTagData = 43,

    PersonDoesNotExist = 51,
    InvalidPersonData = 51,

    RoleDoesNotExist = 61,
    InvalidRoleData = 62,

    RankDoesNotExist = 71,
    InvalidRankData = 72,

    IssueDoesNotExist = 81,
    IssueAlreadyExists = 82,

    SavePointAlreadyExists = 91,
    NoSavePointAvailable = 92,
    SavePointPathDoesNotExist = 93,

    FeatureDisabled = 101,

    InvalidSettings = 111,

    TokenRevoked = 121,
    InvalidToken = 122,
    TokenRequired = 123,
    TokenDoesNotHavePermission = 124,

    ServerAlreadyClaimed = 131
}