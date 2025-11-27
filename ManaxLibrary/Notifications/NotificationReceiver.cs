using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Feature;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.DTO.Person;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.DTO.Role;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.DTO.Tag;
using ManaxLibrary.DTO.User;
using ManaxLibrary.Logging;
using Microsoft.AspNetCore.SignalR.Client;

namespace ManaxLibrary.Notifications;

public static class NotificationReceiver
{
    private static HubConnection? _hubConnection;
    private static string _serverUrl = null!;
    private static string _token = null!;

    public static event Action<List<Permission>>? OnPermissionModified;
    public static event Action<Dictionary<string, int>>? OnRunningTasks;
    public static event Action<long>? OnPosterModified;

    public static event Action<LibraryDto>? OnLibraryCreated;
    public static event Action<long>? OnLibraryDeleted;
    public static event Action<LibraryDto>? OnLibraryUpdated;

    public static event Action<SerieDto>? OnSerieCreated;
    public static event Action<SerieDto>? OnSerieUpdated;
    public static event Action<long>? OnSerieDeleted;

    public static event Action<RankDto>? OnRankCreated;
    public static event Action<RankDto>? OnRankUpdated;
    public static event Action<long>? OnRankDeleted;

    public static event Action<ChapterDto>? OnChapterAdded;
    public static event Action<ChapterDto>? OnChapterUpdated;
    public static event Action<long>? OnChapterDeleted;
    public static event Action<string>? OnChapterUploadFailed;

    public static event Action<UserDto>? OnUserCreated;
    public static event Action<UserDto>? OnUserUpdated;
    public static event Action<long>? OnUserDeleted;

    public static event Action<ReadDto>? OnReadCreated;
    public static event Action<long>? OnReadDeleted;

    public static event Action<IssueChapterReportedDto>? OnReportedChapterIssueCreated;
    public static event Action<long>? OnReportedChapterIssueDeleted;
    public static event Action<IssueSerieReportedDto>? OnReportedSerieIssueCreated;
    public static event Action<long>? OnReportedSerieIssueDeleted;

    public static event Action<TagDto>? OnTagCreated;
    public static event Action<TagDto>? OnTagUpdated;
    public static event Action<long>? OnTagDeleted;

    public static event Action<PersonDto>? OnPersonCreated;
    public static event Action<PersonDto>? OnPersonUpdated;
    public static event Action<long>? OnPersonDeleted;

    public static event Action<RoleDto>? OnRoleCreated;
    public static event Action<RoleDto>? OnRoleUpdated;
    public static event Action<long>? OnRoleDeleted;

    public static event Action<Feature>? OnFeatureModified;


    public static async Task InitializeAsync(Uri host, string token)
    {
        _serverUrl = host.AbsoluteUri;
        _token = token;
        if (_hubConnection != null) return;

        string baseUrl = _serverUrl.EndsWith('/') ? _serverUrl : _serverUrl + "/";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}notificationHub",
                options => { options.AccessTokenProvider = () => Task.FromResult(_token)!; })
            .WithAutomaticReconnect()
            .Build();

        RegisterNotification(NotificationType.LibraryCreated, OnLibraryCreated);
        RegisterNotification(NotificationType.LibraryDeleted, OnLibraryDeleted);
        RegisterNotification(NotificationType.LibraryUpdated, OnLibraryUpdated);

        RegisterNotification(NotificationType.SerieCreated, OnSerieCreated);
        RegisterNotification(NotificationType.SerieUpdated, OnSerieUpdated);
        RegisterNotification(NotificationType.SerieDeleted, OnSerieDeleted);

        RegisterNotification(NotificationType.RankCreated, OnRankCreated);
        RegisterNotification(NotificationType.RankUpdated, OnRankUpdated);
        RegisterNotification(NotificationType.RankDeleted, OnRankDeleted);

        RegisterNotification(NotificationType.ChapterAdded, OnChapterAdded);
        RegisterNotification(NotificationType.ChapterUpdated, OnChapterUpdated);
        RegisterNotification(NotificationType.ChapterRemoved, OnChapterDeleted);
        RegisterNotification(NotificationType.ChapterUploadFailed, OnChapterUploadFailed);

        RegisterNotification(NotificationType.UserCreated, OnUserCreated);
        RegisterNotification(NotificationType.UserUpdated, OnUserUpdated);
        RegisterNotification(NotificationType.UserDeleted, OnUserDeleted);

        RegisterNotification(NotificationType.RunningTasks, OnRunningTasks);
        RegisterNotification(NotificationType.PosterUpdated, OnPosterModified);
        RegisterNotification(NotificationType.PermissionModified, OnPermissionModified);

        RegisterNotification(NotificationType.ReadCreated, OnReadCreated);
        RegisterNotification(NotificationType.ReadDeleted, OnReadDeleted);

        RegisterNotification(NotificationType.ReportedChapterIssueCreated, OnReportedChapterIssueCreated);
        RegisterNotification(NotificationType.ReportedChapterIssueDeleted, OnReportedChapterIssueDeleted);
        RegisterNotification(NotificationType.ReportedSerieIssueCreated, OnReportedSerieIssueCreated);
        RegisterNotification(NotificationType.ReportedSerieIssueDeleted, OnReportedSerieIssueDeleted);

        RegisterNotification(NotificationType.TagCreated, OnTagCreated);
        RegisterNotification(NotificationType.TagUpdated, OnTagUpdated);
        RegisterNotification(NotificationType.TagDeleted, OnTagDeleted);

        RegisterNotification(NotificationType.PersonCreated, OnPersonCreated);
        RegisterNotification(NotificationType.PersonUpdated, OnPersonUpdated);
        RegisterNotification(NotificationType.PersonDeleted, OnPersonDeleted);

        RegisterNotification(NotificationType.RoleCreated, OnRoleCreated);
        RegisterNotification(NotificationType.RoleUpdated, OnRoleUpdated);
        RegisterNotification(NotificationType.RoleDeleted, OnRoleDeleted);

        RegisterNotification(NotificationType.FeatureModified, OnFeatureModified);

        _hubConnection.On<string>(nameof(NotificationType.Connected),
            message => { Logger.LogInfo("SignalR Server: " + message); });

        _hubConnection.Closed += async exception =>
        {
            if (exception != null)
                Logger.LogError("SignalR: Connection closed", exception);
            else
                Logger.LogInfo("SignalR: Connection closed");
            await Task.Delay(5000);
            await ConnectAsync();
        };

        _hubConnection.Reconnecting += exception =>
        {
            if (exception != null)
                Logger.LogError("SignalR: Attempting to reconnect", exception);
            else
                Logger.LogInfo("SignalR: Attempting to reconnect");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += connectionId =>
        {
            Logger.LogInfo("SignalR: Reconnected with ID: " + connectionId);
            return Task.CompletedTask;
        };

        await ConnectAsync();
    }
    
    private static void RegisterNotification<T>(NotificationType type, Action<T>? handler)
    {
        _hubConnection!.On<T>(type.ToString(), data => handler?.Invoke(data));
    }

    private static async Task ConnectAsync()
    {
        try
        {
            if (_hubConnection == null) return;

            Logger.LogInfo("SignalR: Attempting to connect...");
            await _hubConnection.StartAsync();
            Logger.LogInfo("SignalR: Connection established successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError("SignalR: Connection error", ex);
            await Task.Delay(5000);
            Logger.LogInfo("SignalR: Will retry connection in 5 seconds...");
            await ConnectAsync();
        }
    }
}