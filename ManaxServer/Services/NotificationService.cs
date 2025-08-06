using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.DTO.User;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;
using Microsoft.AspNetCore.SignalR;
using ManaxServer.Hubs;
using ManaxServer.Localization;

namespace ManaxServer.Services;

public static class NotificationService
{
    private static IHubContext<NotificationHub> _hubContext = null!;
    
    public static void Initialize(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
        Logger.LogInfo("NotificationService initialisé");
    }
    
    private static void TrySendToAllClientsAsync(NotificationType type, object? arg)
    {
        string methodName = type.ToString();
        try
        {
            _hubContext.Clients.All.SendAsync(methodName, arg);
            Logger.LogInfo(Localizer.Format("HubMessageSent",methodName));
        }
        catch (Exception ex)
        {
            Logger.LogError(Localizer.Format("HubMessageError",methodName),ex, Environment.StackTrace );
        }
    }
    
    public static void NotifyLibraryCreatedAsync(LibraryDto library)
    {
        TrySendToAllClientsAsync(NotificationType.LibraryCreated, library);
    }

    public static void NotifyLibraryDeletedAsync(long id)
    {
        TrySendToAllClientsAsync(NotificationType.LibraryDeleted, id);
    }

    public static void NotifyLibraryUpdatedAsync(LibraryDto library)
    {
        TrySendToAllClientsAsync(NotificationType.LibraryUpdated, library);
    }

    public static void NotifySerieCreatedAsync(SerieDto serie)
    {
        TrySendToAllClientsAsync(NotificationType.SerieCreated, serie);
    }

    public static void NotifySerieUpdatedAsync(SerieDto serie)
    {
        TrySendToAllClientsAsync(NotificationType.SerieUpdated, serie);
    }
    
    public static void NotifySerieDeletedAsync(long serieId)
    {
        TrySendToAllClientsAsync(NotificationType.SerieDeleted, serieId);
    }
    
    public static void NotifyRankCreatedAsync(RankDto rank)
    {
        TrySendToAllClientsAsync(NotificationType.RankCreated, rank);
    }

    public static void NotifyRankUpdatedAsync(RankDto rank)
    {
        TrySendToAllClientsAsync(NotificationType.RankUpdated, rank);
    }
    
    public static void NotifyRankDeletedAsync(long rankId)
    {
        TrySendToAllClientsAsync(NotificationType.RankDeleted, rankId);
    }

    public static void NotifyChapterAddedAsync(ChapterDto chapter)
    {
        TrySendToAllClientsAsync(NotificationType.ChapterAdded, chapter);
    }

    public static void NotifyChapterRemovedAsync(long chapterId)
    {
        TrySendToAllClientsAsync(NotificationType.ChapterRemoved, chapterId);
    }
    
    public static void NotifyUserCreatedAsync(UserDto user)
    {
        TrySendToAllClientsAsync(NotificationType.UserCreated, user);
    }
    
    public static void NotifyUserDeletedAsync(long userId)
    {
        TrySendToAllClientsAsync(NotificationType.UserDeleted, userId);
    }

    public static void NotifyRunningTasksAsync(Dictionary<string, int> tasks)
    {
        TrySendToAllClientsAsync(NotificationType.RunningTasks, tasks);
    }

    public static void NotifyPosterModifiedAsync(long serieId)
    {
        TrySendToAllClientsAsync(NotificationType.PosterModified, serieId);
    }
}
