using ManaxLibrary.DTOs;
using ManaxLibrary.DTOs.Library;
using ManaxLibrary.DTOs.Rank;
using ManaxLibrary.DTOs.Serie;
using ManaxLibrary.DTOs.User;
using ManaxLibrary.Logging;
using Microsoft.AspNetCore.SignalR.Client;

namespace ManaxLibrary.Notifications;

public static class ServerNotification
{
    private static HubConnection? _hubConnection;
    private static string _serverUrl = null!;
    private static string _token = null!;
    
    public static event Action<Dictionary<string, int>>? OnRunningTasks;
    public static event Action<long>? OnPosterModified;
    
    public static event Action<LibraryDTO>? OnLibraryCreated;
    public static event Action<long>? OnLibraryDeleted;
    public static event Action<LibraryDTO>? OnLibraryUpdated;
    
    public static event Action<SerieDTO>? OnSerieCreated;
    public static event Action<SerieDTO>? OnSerieUpdated;
    public static event Action<long>? OnSerieDeleted;
    
    public static event Action<RankDTO>? OnRankCreated;
    public static event Action<RankDTO>? OnRankUpdated;
    public static event Action<long>? OnRankDeleted;
    
    public static event Action<ChapterDTO>? OnChapterAdded;
    public static event Action<long>? OnChapterDeleted;
    
    public static event Action<UserDTO>? OnUserCreated;
    public static event Action<long>? OnUserDeleted;
    
    public static async Task InitializeAsync(Uri host, string token)
    {
        _serverUrl = host.AbsoluteUri;
        _token = token;
        if (_hubConnection != null) { return; }

        string baseUrl = _serverUrl.EndsWith('/') ? _serverUrl : _serverUrl + "/";
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}notificationHub?access_token={_token}")
            .WithAutomaticReconnect()
            .Build();
        
        _hubConnection.On<LibraryDTO>(nameof(NotificationType.LibraryCreated), libraryData =>
        {
            OnLibraryCreated?.Invoke(libraryData);
        });
        
        _hubConnection.On<long>(nameof(NotificationType.LibraryDeleted), libraryId =>
        {
            OnLibraryDeleted?.Invoke(libraryId);
        });
        
        _hubConnection.On<LibraryDTO>(nameof(NotificationType.LibraryUpdated), libraryData =>
        {
            OnLibraryUpdated?.Invoke(libraryData);
        });
        
        _hubConnection.On<SerieDTO>(nameof(NotificationType.SerieCreated), serieData =>
        {
            OnSerieCreated?.Invoke(serieData);
        });
        
        _hubConnection.On<SerieDTO>(nameof(NotificationType.SerieUpdated), serieData =>
        {
            OnSerieUpdated?.Invoke(serieData);
        });
        
        _hubConnection.On<long>(nameof(NotificationType.SerieDeleted), serieId =>
        {
            OnSerieDeleted?.Invoke(serieId);
        });
        
        _hubConnection.On<RankDTO>(nameof(NotificationType.RankCreated), rankData =>
        {
            OnRankCreated?.Invoke(rankData);
        });
        
        _hubConnection.On<RankDTO>(nameof(NotificationType.RankUpdated), rankData =>
        {
            OnRankUpdated?.Invoke(rankData);
        });
        
        _hubConnection.On<long>(nameof(NotificationType.RankDeleted), rankId =>
        {
            OnRankDeleted?.Invoke(rankId);
        });
        
        _hubConnection.On<ChapterDTO>(nameof(NotificationType.ChapterAdded), chapterData =>
        {
            OnChapterAdded?.Invoke(chapterData);
        });
        
        _hubConnection.On<long>(nameof(NotificationType.ChapterRemoved), chapterId =>
        {
            OnChapterDeleted?.Invoke(chapterId);
        });
        
        _hubConnection.On<UserDTO>(nameof(NotificationType.UserCreated), userData =>
        {
            OnUserCreated?.Invoke(userData);
        });
        
        _hubConnection.On<long>(nameof(NotificationType.UserDeleted), userId =>
        {
            OnUserDeleted?.Invoke(userId);
        });
        
        _hubConnection.On<Dictionary<string, int>>(nameof(NotificationType.RunningTasks), tasks =>
        {
            OnRunningTasks?.Invoke(tasks);
        });

        _hubConnection.On<long>(nameof(NotificationType.PosterModified), serieId =>
        {
            OnPosterModified?.Invoke(serieId);
        });
        
        _hubConnection.On<string>(nameof(NotificationType.Connected), message =>
        {
            Logger.LogInfo("SignalR Server: " + message);
        });

        _hubConnection.Closed += async exception =>
        {
            if (exception != null) { Logger.LogError("SignalR: Connexion fermée", exception, Environment.StackTrace); }
            else { Logger.LogInfo("SignalR: Connexion fermée"); }
            await Task.Delay(5000);
            await ConnectAsync();
        };
        
        _hubConnection.Reconnecting += exception =>
        {
            if (exception != null) { Logger.LogError("SignalR: Tentative de reconnexion", exception, Environment.StackTrace); }
            else { Logger.LogInfo("SignalR: Tentative de reconnexion"); }
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
            Logger.LogError("SignalR: Erreur de connexion", ex, Environment.StackTrace);
            await Task.Delay(5000);
            await ConnectAsync();
        }
    }
    
    public static async Task StopAsync()
    {
        if (_hubConnection != null)
        {
            try
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
            catch (Exception ex)
            {
                Logger.LogError("SignalR: Erreur lors de l'arrêt de la connexion", ex, Environment.StackTrace);
            }
        }
    }
}
