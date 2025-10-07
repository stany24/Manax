using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.DTO.Tag;
using ManaxLibrary.DTO.User;
using ManaxLibrary.Logging;
using Microsoft.AspNetCore.SignalR.Client;

namespace ManaxLibrary.Notifications;

public static class ServerNotification
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
    public static event Action<ChapterDto>? OnChapterModified;
    public static event Action<long>? OnChapterDeleted;

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

        _hubConnection.On<LibraryDto>(nameof(NotificationType.LibraryCreated),
            libraryData => { OnLibraryCreated?.Invoke(libraryData); });

        _hubConnection.On<long>(nameof(NotificationType.LibraryDeleted),
            libraryId => { OnLibraryDeleted?.Invoke(libraryId); });

        _hubConnection.On<LibraryDto>(nameof(NotificationType.LibraryUpdated),
            libraryData => { OnLibraryUpdated?.Invoke(libraryData); });

        _hubConnection.On<SerieDto>(nameof(NotificationType.SerieCreated),
            serieData => { OnSerieCreated?.Invoke(serieData); });

        _hubConnection.On<SerieDto>(nameof(NotificationType.SerieUpdated),
            serieData => { OnSerieUpdated?.Invoke(serieData); });

        _hubConnection.On<long>(nameof(NotificationType.SerieDeleted),
            serieId => { OnSerieDeleted?.Invoke(serieId); });

        _hubConnection.On<RankDto>(nameof(NotificationType.RankCreated),
            rankData => { OnRankCreated?.Invoke(rankData); });

        _hubConnection.On<RankDto>(nameof(NotificationType.RankUpdated),
            rankData => { OnRankUpdated?.Invoke(rankData); });

        _hubConnection.On<long>(nameof(NotificationType.RankDeleted),
            rankId => { OnRankDeleted?.Invoke(rankId); });

        _hubConnection.On<ChapterDto>(nameof(NotificationType.ChapterAdded),
            chapterData => { OnChapterAdded?.Invoke(chapterData); });

        _hubConnection.On<ChapterDto>(nameof(NotificationType.ChapterUpdated),
            chapterData => { OnChapterModified?.Invoke(chapterData); });

        _hubConnection.On<long>(nameof(NotificationType.ChapterRemoved),
            chapterId => { OnChapterDeleted?.Invoke(chapterId); });

        _hubConnection.On<UserDto>(nameof(NotificationType.UserCreated),
            userData => { OnUserCreated?.Invoke(userData); });

        _hubConnection.On<UserDto>(nameof(NotificationType.UserUpdated),
            userData => { OnUserUpdated?.Invoke(userData); });

        _hubConnection.On<long>(nameof(NotificationType.UserDeleted),
            userId => { OnUserDeleted?.Invoke(userId); });

        _hubConnection.On<Dictionary<string, int>>(nameof(NotificationType.RunningTasks),
            tasks => { OnRunningTasks?.Invoke(tasks); });

        _hubConnection.On<long>(nameof(NotificationType.PosterUpdated),
            serieId => { OnPosterModified?.Invoke(serieId); });

        _hubConnection.On<List<Permission>>(nameof(NotificationType.PermissionModified),
            serieId => { OnPermissionModified?.Invoke(serieId); });

        _hubConnection.On<ReadDto>(nameof(NotificationType.ReadCreated),
            readData => { OnReadCreated?.Invoke(readData); });

        _hubConnection.On<long>(nameof(NotificationType.ReadDeleted),
            readId => { OnReadDeleted?.Invoke(readId); });

        _hubConnection.On<IssueChapterReportedDto>(nameof(NotificationType.ReportedChapterIssueCreated),
            issueData => { OnReportedChapterIssueCreated?.Invoke(issueData); });

        _hubConnection.On<long>(nameof(NotificationType.ReportedChapterIssueDeleted),
            issueId => { OnReportedChapterIssueDeleted?.Invoke(issueId); });

        _hubConnection.On<IssueSerieReportedDto>(nameof(NotificationType.ReportedSerieIssueCreated),
            issueData => { OnReportedSerieIssueCreated?.Invoke(issueData); });

        _hubConnection.On<long>(nameof(NotificationType.ReportedSerieIssueDeleted),
            issueId => { OnReportedSerieIssueDeleted?.Invoke(issueId); });

        _hubConnection.On<TagDto>(nameof(NotificationType.TagCreated),
            tagData => { OnTagCreated?.Invoke(tagData); });

        _hubConnection.On<TagDto>(nameof(NotificationType.TagUpdated),
            tagData => { OnTagUpdated?.Invoke(tagData); });

        _hubConnection.On<long>(nameof(NotificationType.TagDeleted),
            tagId => { OnTagDeleted?.Invoke(tagId); });

        _hubConnection.On<string>(nameof(NotificationType.Connected),
            message => { Logger.LogInfo("SignalR Server: " + message); });

        _hubConnection.Closed += async exception =>
        {
            if (exception != null)
                Logger.LogError("SignalR: Connexion fermée", exception);
            else
                Logger.LogInfo("SignalR: Connexion fermée");
            await Task.Delay(5000);
            await ConnectAsync();
        };

        _hubConnection.Reconnecting += exception =>
        {
            if (exception != null)
                Logger.LogError("SignalR: Tentative de reconnexion", exception);
            else
                Logger.LogInfo("SignalR: Tentative de reconnexion");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += connectionId =>
        {
            Logger.LogInfo("SignalR: Reconnecté avec ID: " + connectionId);
            return Task.CompletedTask;
        };

        await ConnectAsync();
    }

    private static async Task ConnectAsync()
    {
        try
        {
            if (_hubConnection == null) return;

            Logger.LogInfo("SignalR: Tentative de connexion...");
            await _hubConnection.StartAsync();
            Logger.LogInfo("SignalR: Connexion établie avec succès");
        }
        catch (Exception ex)
        {
            Logger.LogError("SignalR: Erreur de connexion", ex);
            await Task.Delay(5000);
            await ConnectAsync();
        }
    }
}