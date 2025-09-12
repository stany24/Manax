namespace ManaxServer.Localization;

public static partial class Localizer
{
    
    public static string ChapterNotFound(long chapterId)
    {
        return string.Format(_currentLocalization[LocalizationKey.ChapterNotFound], chapterId);
    }

    public static string LibraryNotFound(long libraryId)
    {
        return string.Format(_currentLocalization[LocalizationKey.LibraryNotFound], libraryId);
    }

    public static string LibraryNameExists(string name)
    {
        return string.Format(_currentLocalization[LocalizationKey.LibraryNameExists], name);
    }

    public static string LibraryAlreadyCreated()
    {
        return _currentLocalization[LocalizationKey.LibraryAlreadyCreated];
    }

    public static string LibraryNameOrPathNotUnique()
    {
        return _currentLocalization[LocalizationKey.LibraryNameOrPathNotUnique];
    }

    public static string SerieNotFound(long serieId)
    {
        return string.Format(_currentLocalization[LocalizationKey.SerieNotFound], serieId);
    }

    public static string PosterNotFound(long serieId)
    {
        return string.Format(_currentLocalization[LocalizationKey.PosterNotFound], serieId);
    }

    public static string SerieAlreadyExists()
    {
        return _currentLocalization[LocalizationKey.SerieAlreadyExists];
    }

    public static string InvalidZipFile()
    {
        return _currentLocalization[LocalizationKey.InvalidZipFile];
    }

    public static string UserNotFound(long userId)
    {
        return string.Format(_currentLocalization[LocalizationKey.UserNotFound], userId);
    }

    public static string UserMustBeLoggedInDelete()
    {
        return _currentLocalization[LocalizationKey.UserMustBeLoggedInDelete];
    }

    public static string UserCannotDeleteSelf()
    {
        return _currentLocalization[LocalizationKey.UserCannotDeleteSelf];
    }

    public static string UserCannotDeleteAdminOrOwner()
    {
        return _currentLocalization[LocalizationKey.UserCannotDeleteAdminOrOwner];
    }

    public static string UserInvalidLogin()
    {
        return _currentLocalization[LocalizationKey.UserInvalidLogin];
    }

    public static string UserClaimNotAllowed()
    {
        return _currentLocalization[LocalizationKey.UserClaimNotAllowed];
    }

    public static string RankNotFound(long rankId)
    {
        return string.Format(_currentLocalization[LocalizationKey.RankNotFound], rankId);
    }

    public static string MustBeLoggedInSetRank()
    {
        return _currentLocalization[LocalizationKey.MustBeLoggedInSetRank];
    }

    public static string MustBeLoggedInGetRanking()
    {
        return _currentLocalization[LocalizationKey.MustBeLoggedInGetRanking];
    }

    public static string UserOrChapterNotFound()
    {
        return _currentLocalization[LocalizationKey.UserOrChapterNotFound];
    }

    public static string ChapterAlreadyExists()
    {
        return _currentLocalization[LocalizationKey.ChapterAlreadyExists];
    }

    public static string PosterAlreadyExists()
    {
        return _currentLocalization[LocalizationKey.PosterAlreadyExists];
    }

    public static string InvalidImageFile(string fileName)
    {
        return string.Format(_currentLocalization[LocalizationKey.InvalidImageFile], fileName);
    }

    public static string IssueNotFound(long issueId)
    {
        return string.Format(_currentLocalization[LocalizationKey.IssueNotFound], issueId);
    }

    public static string ChapterFileNotExistOrInvalid()
    {
        return _currentLocalization[LocalizationKey.ChapterFileNotExistOrInvalid];
    }

    public static string PageNumberTooBig(int pageNumber, int totalPages)
    {
        return string.Format(_currentLocalization[LocalizationKey.PageNumberTooBig], pageNumber, totalPages);
    }

    public static string UserMustBeLoggedInRead()
    {
        return _currentLocalization[LocalizationKey.UserMustBeLoggedInRead];
    }

    public static string HubConnected(string connectionId, string userName)
    {
        return string.Format(_currentLocalization[LocalizationKey.HubConnected], connectionId, userName);
    }

    public static string HubConnectionError(string connectionId)
    {
        return string.Format(_currentLocalization[LocalizationKey.HubConnectionError], connectionId);
    }

    public static string HubConnectionSuccess()
    {
        return _currentLocalization[LocalizationKey.HubConnectionSuccess];
    }

    public static string HubDisconnectedError(string connectionId)
    {
        return string.Format(_currentLocalization[LocalizationKey.HubDisconnectedError], connectionId);
    }

    public static string HubDisconnected(string connectionId)
    {
        return string.Format(_currentLocalization[LocalizationKey.HubDisconnected], connectionId);
    }

    public static string HubMessageSent(string message)
    {
        return string.Format(_currentLocalization[LocalizationKey.HubMessageSent], message);
    }

    public static string HubMessageError(string message)
    {
        return string.Format(_currentLocalization[LocalizationKey.HubMessageError], message);
    }

    public static string TaskError(string taskName)
    {
        return string.Format(_currentLocalization[LocalizationKey.TaskError], taskName);
    }

    public static string SettingsUpdateNotForced()
    {
        return _currentLocalization[LocalizationKey.SettingsUpdateNotForced];
    }

    public static string ServiceInitialized(string serviceName)
    {
        return string.Format(_currentLocalization[LocalizationKey.ServiceInitialized], serviceName);
    }

    public static string HubMessageSentAdmins(string message)
    {
        return string.Format(_currentLocalization[LocalizationKey.HubMessageSentAdmins], message);
    }

    public static string HubMessageSentOwner(string message)
    {
        return string.Format(_currentLocalization[LocalizationKey.HubMessageSentOwner], message);
    }
    
    public static string HubMessageSentSingle(long user,string message)
    {
        return string.Format(_currentLocalization[LocalizationKey.HubMessageSentSingle],user, message);
    }

    public static string TaskChapterFix()
    {
        return _currentLocalization[LocalizationKey.TaskChapterFix];
    }

    public static string TaskPosterFix()
    {
        return _currentLocalization[LocalizationKey.TaskPosterFix];
    }

    public static string TaskSerieFix()
    {
        return _currentLocalization[LocalizationKey.TaskSerieFix];
    }

    public static string HubMessageErrorAdmins(string message)
    {
        return string.Format(_currentLocalization[LocalizationKey.HubMessageErrorAdmins], message);
    }
    
    
    public static string SavePointNameExists(string path)
    {
        return string.Format(_currentLocalization[LocalizationKey.SavePointNameExists], path);
    }

    public static string SavePointPathNotExists(string path)
    {
        return string.Format(_currentLocalization[LocalizationKey.SavePointPathNotExists], path);
    }

    public static string LibraryNameRequired()
    {
        return _currentLocalization[LocalizationKey.LibraryNameRequired];
    }

    public static string SerieTitleRequired()
    {
        return _currentLocalization[LocalizationKey.SerieTitleRequired];
    }

    public static string ChapterDoesNotExist()
    {
        return _currentLocalization[LocalizationKey.ChapterDoesNotExist];
    }

    public static string NoSavePoint()
    {
        return _currentLocalization[LocalizationKey.SavePointNone];
    }

    public static string SerieCreationFailed()
    {
        return _currentLocalization[LocalizationKey.SerieCreationFailed];
    }

    public static string Unauthorized()
    {
        return _currentLocalization[LocalizationKey.Unauthorized];
    }

    public static string HubMessageErrorOwner(string methodName)
    {
        return string.Format(_currentLocalization[LocalizationKey.HubMessageErrorOwner], methodName);
    }
    
    public static string HubMessageErrorSingle(long user,string methodName)
    {
        return string.Format(_currentLocalization[LocalizationKey.HubMessageErrorSingle],user, methodName);
    }
    
    public static string PasswordEmpty()
    {
        return _currentLocalization[LocalizationKey.PasswordEmpty];
    }
    
    public static string PasswordTooShort()
    {
        return _currentLocalization[LocalizationKey.PasswordTooShort];
    }
    
    public static string PasswordNoLowercase()
    {
        return _currentLocalization[LocalizationKey.PasswordNoLowercase];
    }
    
    public static string PasswordNoUppercase()
    {
        return _currentLocalization[LocalizationKey.PasswordNoUppercase];
    }
    
    public static string PasswordNoSpecialCharacterOrDigit()
    {
        return _currentLocalization[LocalizationKey.PasswordNoSpecialCharacterOrDigit];
    }

    public static string TagNotFound(long tagId)
    {
        
        return string.Format(_currentLocalization[LocalizationKey.TagNotFound],tagId);
    }
}