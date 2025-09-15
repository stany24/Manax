using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Claims;
using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.DTO.Tag;
using ManaxLibrary.DTO.User;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;
using ManaxServer.Localization;
using ManaxServer.Services.Permission;
using Microsoft.AspNetCore.SignalR;

namespace ManaxServer.Services.Notification;

public class NotificationService(IHubContext<NotificationService> hubContext, IPermissionService permissionService) : Hub, INotificationService
{
    private static readonly ConcurrentDictionary<string, long> Connections = new();

    public void NotifyLibraryCreatedAsync(LibraryDto library)
    {
        TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission.ReadLibrary, NotificationType.LibraryCreated, library);
    }

    public void NotifyLibraryDeletedAsync(long libraryId)
    {
        TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission.ReadLibrary, NotificationType.LibraryDeleted, libraryId);
    }

    public void NotifyLibraryUpdatedAsync(LibraryDto library)
    {
        TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission.ReadLibrary, NotificationType.LibraryUpdated, library);
    }

    public void NotifySerieCreatedAsync(SerieDto serie)
    {
        TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission.ReadSeries, NotificationType.SerieCreated, serie);
    }

    public void NotifySerieUpdatedAsync(SerieDto serie)
    {
        TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission.ReadSeries, NotificationType.SerieUpdated, serie);
    }

    public void NotifySerieDeletedAsync(long serieId)
    {
        TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission.ReadSeries, NotificationType.SerieDeleted, serieId);
    }

    public void NotifyRankCreatedAsync(RankDto rank)
    {
        TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission.ReadRanks, NotificationType.RankCreated, rank);
    }

    public void NotifyRankUpdatedAsync(RankDto rank)
    {
        TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission.ReadRanks, NotificationType.RankUpdated, rank);
    }

    public void NotifyRankDeletedAsync(long rankId)
    {
        TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission.ReadRanks, NotificationType.RankDeleted, rankId);
    }

    public void NotifyChapterAddedAsync(ChapterDto chapter)
    {
        TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission.ReadChapters, NotificationType.ChapterAdded, chapter);
    }
    
    public void NotifyChapterModifiedAsync(ChapterDto chapter)
    {
        TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission.ReadChapters, NotificationType.ChapterModified, chapter);
    }

    public void NotifyChapterRemovedAsync(long chapterId)
    {
        TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission.ReadChapters, NotificationType.ChapterRemoved, chapterId);
    }

    public void NotifyUserCreatedAsync(UserDto user)
    {
        TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission.ReadUsers, NotificationType.UserCreated, user);
    }

    public void NotifyUserDeletedAsync(long userId)
    {
        TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission.ReadUsers, NotificationType.UserDeleted, userId);
    }

    public void NotifyRunningTasksAsync(Dictionary<string, int> tasks)
    {
        TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission.ReadServerStats, NotificationType.RunningTasks, tasks);
    }

    public void NotifyPosterModifiedAsync(long serieId)
    {
        TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission.ReadSeries, NotificationType.PosterModified, serieId);
    }

    public void NotifyReadCreated(ReadDto existingRead)
    {
        TrySendToSingleClientAsync(existingRead.UserId, NotificationType.ReadCreated, existingRead);
    }

    public void NotifyReadRemoved(ReadDto existingRead)
    {
        TrySendToSingleClientAsync(existingRead.UserId, NotificationType.ReadCreated, existingRead.ChapterId);
    }

    public void NotifySerieIssueCreatedAsync(ReportedIssueSerieDto issue)
    {
        TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission.ReadAllIssues, NotificationType.ReportedSerieIssueCreated, issue);
    }

    public void NotifyChapterIssueCreatedAsync(ReportedIssueChapterDto issue)
    {
        TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission.ReadAllIssues, NotificationType.ReportedChapterIssueCreated, issue);
    }

    public void NotifyChapterIssueDeletedAsync(long issueId)
    {
        TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission.ReadAllIssues, NotificationType.ReportedChapterIssueDeleted, issueId);
    }

    public void NotifySerieIssueDeletedAsync(long issueId)
    {
        TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission.ReadAllIssues, NotificationType.ReportedSerieIssueDeleted, issueId);
    }
    
    public void NotifyTagCreatedAsync(TagDto tag)
    {
        TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission.ReadTags, NotificationType.TagCreated, tag);
    }

    public void NotifyTagUpdatedAsync(TagDto tag)
    {
        TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission.ReadTags, NotificationType.TagUpdated, tag);
    }

    public void NotifyTagDeletedAsync(long tagId)
    {
        TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission.ReadTags, NotificationType.TagDeleted, tagId);
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            Logger.LogInfo(Localizer.HubConnected(Context.ConnectionId, Context.User?.Identity?.Name ?? "Unknown"));
            
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                if (long.TryParse(Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out long userId))
                {
                    Connections.TryAdd(Context.ConnectionId, userId);
                }
            }

            await base.OnConnectedAsync();
            await Clients.Caller.SendAsync("Connected", Localizer.HubConnectionSuccess());
        }
        catch (Exception ex)
        {
            Logger.LogError(Localizer.HubConnectionError(Context.ConnectionId), ex, Environment.StackTrace);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            Connections.TryRemove(Context.ConnectionId, out _);
            
            if (exception != null)
                Logger.LogError(Localizer.HubDisconnectedError(Context.ConnectionId), exception,
                    Environment.StackTrace);
            else
                Logger.LogInfo(Localizer.HubDisconnected(Context.ConnectionId));

            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            Logger.LogError(Localizer.HubDisconnectedError(Context.ConnectionId), ex, Environment.StackTrace);
        }
    }

    private void TrySendToClientsWithPermissionAsync(ManaxLibrary.DTO.User.Permission permission, NotificationType type, object? arg)
    {
        string methodName = type.ToString();
        try
        {
            List<string> connectionIds = [];
            foreach (KeyValuePair<string, long> connection in Connections)
            {
                if (permissionService.UserHasPermission(connection.Value, permission))
                {
                    connectionIds.Add(connection.Key);
                }
            }

            if (connectionIds.Count <= 0) return;
            hubContext.Clients.Clients(connectionIds).SendAsync(methodName, arg);
            Logger.LogInfo(Localizer.HubMessageSent(methodName));
        }
        catch (Exception ex)
        {
            Logger.LogError(Localizer.HubMessageError(methodName), ex, Environment.StackTrace);
        }
    }

    private void TrySendToSingleClientAsync(long id, NotificationType type, object? arg)
    {
        string methodName = type.ToString();
        try
        {
            hubContext.Clients.User(id.ToString(CultureInfo.InvariantCulture)).SendAsync(methodName, arg);
            Logger.LogInfo(Localizer.HubMessageSentSingle(id, methodName));
        }
        catch (Exception ex)
        {
            Logger.LogError(Localizer.HubMessageErrorSingle(id, methodName), ex, Environment.StackTrace);
        }
    }
}
