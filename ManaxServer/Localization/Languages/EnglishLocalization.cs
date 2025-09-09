namespace ManaxServer.Localization.Languages;

public class EnglishLocalization: Localization
{
    public override Dictionary<LocalizationKey, string> GetLocalization()
    {
        return new Dictionary<LocalizationKey, string>
        {
            { LocalizationKey.ChapterNotFound, "Chapter with ID {0} does not exist." },
            { LocalizationKey.LibraryNotFound, "Library with ID {0} does not exist." },
            { LocalizationKey.LibraryNameExists, "A library with name '{0}' already exists." },
            { LocalizationKey.LibraryAlreadyCreated, "Library already created." },
            { LocalizationKey.LibraryNameOrPathNotUnique, "Name or path is not unique." },
            { LocalizationKey.SerieNotFound, "Serie with ID {0} does not exist." },
            { LocalizationKey.PosterNotFound, "Poster not found for serie with ID {0}." },
            { LocalizationKey.SerieAlreadyExists, "Serie already exists." },
            { LocalizationKey.InvalidZipFile, "Invalid zip file." },
            { LocalizationKey.UserNotFound, "User with ID {0} does not exist." },
            { LocalizationKey.UserMustBeLoggedInDelete, "You must be logged in to delete a user." },
            { LocalizationKey.UserCannotDeleteSelf, "You cannot delete yourself." },
            { LocalizationKey.UserCannotDeleteAdminOrOwner, "You cannot delete another admin or owner." },
            { LocalizationKey.UserInvalidLogin, "Invalid username or password." },
            { LocalizationKey.UserClaimNotAllowed, "Claim is not allowed, users already exist." },
            { LocalizationKey.RankNotFound, "Rank with ID {0} does not exist." },
            { LocalizationKey.MustBeLoggedInSetRank, "You must be logged in to set a rank." },
            { LocalizationKey.MustBeLoggedInGetRanking, "You must be logged in to get your ranking." },
            { LocalizationKey.UserOrChapterNotFound, "User or chapter not found." },
            { LocalizationKey.ChapterAlreadyExists, "The chapter already exists." },
            { LocalizationKey.PosterAlreadyExists, "The poster already exists." },
            { LocalizationKey.InvalidImageFile, "Invalid image file: {0}" },
            { LocalizationKey.IssueNotFound, "Issue with ID {0} does not exist." },
            { LocalizationKey.ChapterFileNotExistOrInvalid, "Chapter file does not exist or is not a valid CBZ file." },
            { LocalizationKey.PageNumberTooBig, "Page number {0} too big for chapter with {1} pages." },
            { LocalizationKey.UserMustBeLoggedInRead, "You must be logged in to read chapters." },
            { LocalizationKey.HubConnected, "Client connected to notification hub: {0} - User: {1}" },
            { LocalizationKey.HubConnectionError, "Erreur when connecting client {0}" },
            { LocalizationKey.HubConnectionSuccess, "Connexion sucessfully established" },
            { LocalizationKey.HubDisconnectedError, "Client disconected with error: {0}" },
            { LocalizationKey.HubDisconnected, "Client disconnected: {0}" },
            { LocalizationKey.HubMessageSent, "Message sent to all clients: {0}" },
            { LocalizationKey.HubMessageError, "Erreur while sending notification tu users {0}" },
            { LocalizationKey.TaskError, "Error executing task: {0}" },
            { LocalizationKey.SettingsUpdateNotForced, "The settings given have an issue, fix it or force the change." },
            { LocalizationKey.ServiceInitialized, "Service {0} was initialized." },
            { LocalizationKey.HubMessageSentAdmins, "Message sent to all admins: {0}" },
            { LocalizationKey.HubMessageSentOwner, "Message sent to the owner: {0}" },
            { LocalizationKey.HubMessageSentSingle, "Message sent to {0}: {1}" },
            { LocalizationKey.TaskChapterFix, "Chapter fix" },
            { LocalizationKey.TaskPosterFix, "Poster fix" },
            { LocalizationKey.TaskSerieFix, "Serie fix" },
            { LocalizationKey.HubMessageErrorAdmins, "Erreur while sending notification to admins {0}" },
            { LocalizationKey.SavePointNameExists, "A save point with this path '{0}' already exists." },
            { LocalizationKey.SavePointPathNotExists, "The save point path '{0}' does not exist." },
            { LocalizationKey.LibraryNameRequired, "Library name is required." },
            { LocalizationKey.SerieTitleRequired, "Serie title is required." },
            { LocalizationKey.ChapterDoesNotExist, "Chapter does not exist." },
            { LocalizationKey.SavePointNone, "No save point available." },
            { LocalizationKey.SerieCreationFailed, "Serie creation failed." },
            { LocalizationKey.Unauthorized, "Unauthorized access." },
            { LocalizationKey.HubMessageErrorOwner, "Error while sending notification to owner {0}" },
            { LocalizationKey.HubMessageErrorSingle, "Error while sending notification to {0}: {1}" }
        };
    }
}