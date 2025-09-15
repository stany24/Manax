namespace ManaxServer.Localization.Languages;

public class FrenchLocalization : Localization
{
    public override Dictionary<LocalizationKey, string> GetLocalization()
    {
        return new Dictionary<LocalizationKey, string>
        {
            { LocalizationKey.ChapterNotFound, "Le chapitre avec l'ID {0} n'existe pas" },
            { LocalizationKey.LibraryNotFound, "La librairie avec l'id {0} n'existe pas." },
            { LocalizationKey.LibraryNameExists, "Une librairie avec le nom '{0}' existe déjà." },
            { LocalizationKey.LibraryAlreadyCreated, "La librairie est déjà crée." },
            { LocalizationKey.LibraryNameOrPathNotUnique, "Le nom ou le chemin n'est pas unique." },
            { LocalizationKey.SerieNotFound, "La série avec l'id {0} n'existe pas." },
            { LocalizationKey.PosterNotFound, "Le poster pour la série {0} n'a pas été trouvé." },
            { LocalizationKey.SerieAlreadyExists, "La série existe déjà." },
            { LocalizationKey.InvalidZipFile, "Fichier zip invalide." },
            { LocalizationKey.UserNotFound, "L'utilisateur avec l'ID {0} n'existe pas." },
            { LocalizationKey.UserMustBeLoggedInDelete, "Vous devez être connecté pour supprimer un utilisateur." },
            { LocalizationKey.UserCannotDeleteSelf, "Vous ne pouvez pas vous supprimer vous-même." },
            {
                LocalizationKey.UserCannotDeleteAdminOrOwner,
                "Vous ne pouvez pas supprimer un autre admin ou propriétaire."
            },
            { LocalizationKey.UserInvalidLogin, "Nom d'utilisateur ou mot de passe invalide." },
            { LocalizationKey.UserClaimNotAllowed, "Réclamation non autorisée, des utilisateurs existent déjà." },
            { LocalizationKey.RankNotFound, "Le rank avec l'id {0} n'existe pas." },
            { LocalizationKey.MustBeLoggedInSetRank, "Vous devez être connecté pour définir un rang." },
            { LocalizationKey.MustBeLoggedInGetRanking, "Vous devez être connecté pour obtenir votre classement." },
            { LocalizationKey.UserOrChapterNotFound, "Utilisateur ou chapitre non trouvé." },
            { LocalizationKey.ChapterAlreadyExists, "Le chapitre existe déjà." },
            { LocalizationKey.PosterAlreadyExists, "Le poster existe déjà." },
            { LocalizationKey.InvalidImageFile, "Image invalide: {0}" },
            { LocalizationKey.IssueNotFound, "Le problème avec l'id {0} n'existe pas" },
            {
                LocalizationKey.ChapterFileNotExistOrInvalid,
                "Le chapitre n'existe pas ou n'est pas un fichier CBZ valide."
            },
            {
                LocalizationKey.PageNumberTooBig,
                "Le numéro de page {0} est trop grand pour un chapitre avec {1} pages."
            },
            { LocalizationKey.UserMustBeLoggedInRead, "Vous devez être connecté pour lire les chapitres." },
            { LocalizationKey.HubConnected, "Client connecté au hub de notifications: {0} - Utilisateur: {1}" },
            { LocalizationKey.HubConnectionError, "Erreur lors de la connexion du client {0}" },
            { LocalizationKey.HubConnectionSuccess, "Connexion établie avec succès" },
            { LocalizationKey.HubDisconnectedError, "Client déconnecté avec erreur: {0}" },
            { LocalizationKey.HubDisconnected, "Client déconnecté: {0}" },
            { LocalizationKey.HubMessageSent, "Message envoyé à tous les clients:  {0}" },
            {
                LocalizationKey.HubMessageError,
                "Erreur lors de l'envoi de la notification pendant l'envoi de {0} aux utilisateurs"
            },
            { LocalizationKey.TaskError, "Erreur pendant l'execution de la tache: {0}" },
            {
                LocalizationKey.SettingsUpdateNotForced,
                "Les paramètres donnée on un problème, corriger le ou forcez le changement."
            },
            { LocalizationKey.ServiceInitialized, "Le service {0} à été initializé." },
            { LocalizationKey.HubMessageSentAdmins, "Message envoyé à tous les administrateurs:  {0}" },
            { LocalizationKey.HubMessageSentOwner, "Message envoyé au propriétaire:  {0}" },
            { LocalizationKey.HubMessageSentSingle, "Message envoyé a {0}: {1}" },
            { LocalizationKey.TaskChapterFix, "Vérification du chapitre" },
            { LocalizationKey.TaskPosterFix, "Vérification du poster" },
            { LocalizationKey.TaskSerieFix, "Vérification de la série" },
            {
                LocalizationKey.HubMessageErrorAdmins,
                "Erreur lors de l'envoi de la notification pendant l'envoi de {0} aux administrateurs"
            },
            { LocalizationKey.SavePointNameExists, "Un point de sauvegarde avec ce chemin '{0}' existe déjà." },
            { LocalizationKey.SavePointPathNotExists, "Le chemin du point de sauvegarde '{0}' n'existe pas." },
            { LocalizationKey.LibraryNameRequired, "Le nom de la librairie est requis." },
            { LocalizationKey.SerieTitleRequired, "Le titre de la série est requis." },
            { LocalizationKey.ChapterDoesNotExist, "Le chapitre n'existe pas." },
            { LocalizationKey.SavePointNone, "Aucun point de sauvegarde disponible." },
            { LocalizationKey.SerieCreationFailed, "La création de la série a échoué." },
            { LocalizationKey.Unauthorized, "Accès non autorisé." },
            { LocalizationKey.HubMessageErrorOwner, "Erreur lors de l'envoi de la notification au propriétaire {0}" },
            { LocalizationKey.HubMessageErrorSingle, "Erreur lors de l'envoi de la notification a {0} : {1}" },
            { LocalizationKey.PasswordEmpty, "Le mot de passe est requis." },
            { LocalizationKey.PasswordTooShort, "Le mot de passe doit contenir au moins 14 caractères." },
            { LocalizationKey.PasswordNoLowercase, "Le mot de passe doit contenir au moins une lettre minuscule." },
            { LocalizationKey.PasswordNoUppercase, "Le mot de passe doit contenir au moins une lettre majuscule." },
            {
                LocalizationKey.PasswordNoSpecialCharacterOrDigit,
                "Le mot de passe doit contenir au moins un caractère spécial ou un chiffre."
            },
            { LocalizationKey.TagNotFound, "Le tag avec l'ID {0} n'existe pas." }
        };
    }
}