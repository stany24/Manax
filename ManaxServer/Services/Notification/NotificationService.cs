using System.Security.Claims;
using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.DTO.User;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;
using ManaxServer.Localization;
using ManaxServer.Models.Read;
using Microsoft.AspNetCore.SignalR;

namespace ManaxServer.Services.Notification;

public class NotificationService(IHubContext<NotificationService> hubContext) : Hub, INotificationService
{
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
        TrySendToAdminsAsync(NotificationType.UserCreated, user);
    }

    public void NotifyUserDeletedAsync(long userId)
    {
        TrySendToAdminsAsync(NotificationType.UserDeleted, userId);
    }

    public void NotifyRunningTasksAsync(Dictionary<string, int> tasks)
    {
        TrySendToAdminsAsync(NotificationType.RunningTasks, tasks);
    }

    public void NotifyPosterModifiedAsync(long serieId)
    {
        TrySendToAllClientsAsync(NotificationType.PosterModified, serieId);
    }
    
    public void NotifyReadCreated(ReadDto existingRead)
    {
        TrySendToSingleClientAsync(existingRead.UserId, NotificationType.ReadCreated, existingRead);
    }
    
    public void NotifyReadRemoved(ReadDto existingRead)
    {
        TrySendToSingleClientAsync(existingRead.UserId, NotificationType.ReadCreated, existingRead.ChapterId);
    }

    public override async System.Threading.Tasks.Task OnConnectedAsync()
    {
        try
        {
            Logger.LogInfo(Localizer.Format("HubConnected", Context.ConnectionId,
                Context.User?.Identity?.Name ?? "Unknown"));
            ClaimsPrincipal? user = Context.User;
            if (user != null)
            {
                bool isAdmin = user.HasClaim(c => c.Type is "role" or ClaimTypes.Role && c.Value is "Admin" or "Owner");
                bool isOwner = user.HasClaim(c => c.Type is "role" or ClaimTypes.Role && c.Value == "Owner");
                if (isAdmin) await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                if (isOwner) await Groups.AddToGroupAsync(Context.ConnectionId, "Owner");
            }

            await base.OnConnectedAsync();
            await Clients.Caller.SendAsync("Connected", Localizer.Format("HubConnectionSuccess"));
        }
        catch (Exception ex)
        {
            Logger.LogError(Localizer.Format("HubConnectionError", Context.ConnectionId), ex, Environment.StackTrace);
        }
    }

    public override async System.Threading.Tasks.Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            if (exception != null)
                Logger.LogError(Localizer.Format("HubDisconnectedError", Context.ConnectionId), exception,
                    Environment.StackTrace);
            else
                Logger.LogInfo(Localizer.Format("HubDisconnected", Context.ConnectionId));

            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            Logger.LogError(Localizer.Format("HubDisconnectedError", Context.ConnectionId), ex, Environment.StackTrace);
        }
    }

    private void TrySendToAllClientsAsync(NotificationType type, object? arg)
    {
        string methodName = type.ToString();
        try
        {
            hubContext.Clients.All.SendAsync(methodName, arg);
            Logger.LogInfo(Localizer.Format("HubMessageSent", methodName));
        }
        catch (Exception ex)
        {
            Logger.LogError(Localizer.Format("HubMessageError", methodName), ex, Environment.StackTrace);
        }
    }

    private void TrySendToAdminsAsync(NotificationType type, object? arg)
    {
        string methodName = type.ToString();
        try
        {
            hubContext.Clients.Group("Admins").SendAsync(methodName, arg);
            Logger.LogInfo(Localizer.Format("HubMessageSentAdmins", methodName));
        }
        catch (Exception ex)
        {
            Logger.LogError(Localizer.Format("HubMessageErrorAdmins", methodName), ex, Environment.StackTrace);
        }
    }

    private void TrySendToOwnerAsync(NotificationType type, object? arg)
    {
        string methodName = type.ToString();
        try
        {
            hubContext.Clients.Group("Owner").SendAsync(methodName, arg);
            Logger.LogInfo(Localizer.Format("HubMessageSentOwner", methodName));
        }
        catch (Exception ex)
        {
            Logger.LogError(Localizer.Format("HubMessageErrorOwner", methodName), ex, Environment.StackTrace);
        }
    }
    
    private void TrySendToSingleClientAsync(long id, NotificationType type, object? arg)
    {
        string methodName = type.ToString();
        try
        {
            hubContext.Clients.User(id.ToString()).SendAsync(methodName, arg);
            Logger.LogInfo(Localizer.Format("HubMessageSentOwner", methodName));
        }
        catch (Exception ex)
        {
            Logger.LogError(Localizer.Format("HubMessageErrorOwner", methodName), ex, Environment.StackTrace);
        }
    }
}