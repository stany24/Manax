using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.DTO.User;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;
using ManaxServer.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ManaxServer.Services;

[Authorize]
public class NotificationService(IHubContext<NotificationService> hubContext) : Hub
{
    public override async Task OnConnectedAsync()
    {
        try
        {
            Logger.LogInfo(Localizer.Format("HubConnected", Context.ConnectionId, Context.User?.Identity?.Name ?? "Unknown"));
            await base.OnConnectedAsync();
            
            await Clients.Caller.SendAsync("Connected", Localizer.Format("HubConnectionSuccess"));
        }
        catch (Exception ex)
        {
            
            Logger.LogError(Localizer.Format("HubConnectionError", Context.ConnectionId),ex,Environment.StackTrace);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            if (exception != null)
            {
                Logger.LogError(Localizer.Format("HubDisconnectedError",Context.ConnectionId),exception, Environment.StackTrace);
            }
            else
            {
                Logger.LogInfo(Localizer.Format("HubDisconnected",Context.ConnectionId));
            }
            
            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            Logger.LogError(Localizer.Format("HubDisconnectedError",Context.ConnectionId),ex, Environment.StackTrace);
        }
    }
    
    private void TrySendToAllClientsAsync(NotificationType type, object? arg)
    {
        string methodName = type.ToString();
        try
        {
            hubContext.Clients.All.SendAsync(methodName, arg);
            Logger.LogInfo(Localizer.Format("HubMessageSent",methodName));
        }
        catch (Exception ex)
        {
            Logger.LogError(Localizer.Format("HubMessageError",methodName),ex, Environment.StackTrace );
        }
    }
    
    public void NotifyLibraryCreatedAsync(LibraryDto library)
    {
        TrySendToAllClientsAsync(NotificationType.LibraryCreated, library);
    }

    public void NotifyLibraryDeletedAsync(long id)
    {
        TrySendToAllClientsAsync(NotificationType.LibraryDeleted, id);
    }

    public void NotifyLibraryUpdatedAsync(LibraryDto library)
    {
        TrySendToAllClientsAsync(NotificationType.LibraryUpdated, library);
    }

    public void NotifySerieCreatedAsync(SerieDto serie)
    {
        TrySendToAllClientsAsync(NotificationType.SerieCreated, serie);
    }

    public void NotifySerieUpdatedAsync(SerieDto serie)
    {
        TrySendToAllClientsAsync(NotificationType.SerieUpdated, serie);
    }
    
    public void NotifySerieDeletedAsync(long serieId)
    {
        TrySendToAllClientsAsync(NotificationType.SerieDeleted, serieId);
    }
    
    public void NotifyRankCreatedAsync(RankDto rank)
    {
        TrySendToAllClientsAsync(NotificationType.RankCreated, rank);
    }

    public void NotifyRankUpdatedAsync(RankDto rank)
    {
        TrySendToAllClientsAsync(NotificationType.RankUpdated, rank);
    }
    
    public void NotifyRankDeletedAsync(long rankId)
    {
        TrySendToAllClientsAsync(NotificationType.RankDeleted, rankId);
    }

    public void NotifyChapterAddedAsync(ChapterDto chapter)
    {
        TrySendToAllClientsAsync(NotificationType.ChapterAdded, chapter);
    }

    public void NotifyChapterRemovedAsync(long chapterId)
    {
        TrySendToAllClientsAsync(NotificationType.ChapterRemoved, chapterId);
    }
    
    public void NotifyUserCreatedAsync(UserDto user)
    {
        TrySendToAllClientsAsync(NotificationType.UserCreated, user);
    }
    
    public void NotifyUserDeletedAsync(long userId)
    {
        TrySendToAllClientsAsync(NotificationType.UserDeleted, userId);
    }

    public void NotifyRunningTasksAsync(Dictionary<string, int> tasks)
    {
        TrySendToAllClientsAsync(NotificationType.RunningTasks, tasks);
    }

    public void NotifyPosterModifiedAsync(long serieId)
    {
        TrySendToAllClientsAsync(NotificationType.PosterModified, serieId);
    }
}
