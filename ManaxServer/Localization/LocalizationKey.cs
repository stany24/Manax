namespace ManaxServer.Localization;

public enum LocalizationKey
{
    ChapterAlreadyExists,
    ChapterDoesNotExist,
    ChapterFileNotExistOrInvalid,
    ChapterNotFound,

    HubConnected,
    HubConnectionError,
    HubConnectionSuccess,
    HubDisconnected,
    HubDisconnectedError,
    HubMessageError,
    HubMessageErrorSingle,
    HubMessageSent,
    HubMessageSentSingle,

    InvalidImageFile,
    InvalidZipFile,

    IssueNotFound,

    LibraryAlreadyCreated,
    LibraryNameExists,
    LibraryNameOrPathNotUnique,
    LibraryNameRequired,
    LibraryNotFound,

    MustBeLoggedInGetRanking,
    MustBeLoggedInSetRank,

    PageNumberTooBig,

    PosterAlreadyExists,
    PosterNotFound,
    RankNotFound,

    SavePointNameExists,
    SavePointNone,
    SavePointPathNotExists,

    SerieAlreadyExists,
    SerieCreationFailed,
    SerieNotFound,
    SerieTitleRequired,

    ServiceInitialized,

    SettingsUpdateNotForced,

    TaskChapterFix,
    TaskError,
    TaskPosterFix,
    TaskSerieFix,

    Unauthorized,

    UserCannotDeleteAdminOrOwner,
    UserCannotDeleteSelf,
    UserClaimNotAllowed,
    UserInvalidLogin,
    UserMustBeLoggedInDelete,
    UserMustBeLoggedInRead,
    UserNotFound,
    UserOrChapterNotFound,

    PasswordEmpty,
    PasswordTooShort,
    PasswordNoLowercase,
    PasswordNoUppercase,
    PasswordNoSpecialCharacterOrDigit,

    TagNotFound,
    
    FeatureDisabled
}