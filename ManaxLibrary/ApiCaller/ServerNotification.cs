using ManaxLibrary.DTOs;
using ManaxLibrary.DTOs.Library;
using ManaxLibrary.DTOs.Serie;
using ManaxLibrary.DTOs.User;
using Microsoft.AspNetCore.SignalR.Client;

namespace ManaxLibrary.ApiCaller;

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
        
        _hubConnection.On<LibraryDTO>("LibraryCreated", libraryData =>
        {
            OnLibraryCreated?.Invoke(libraryData);
        });
        
        _hubConnection.On<long>("LibraryDeleted", libraryId =>
        {
            OnLibraryDeleted?.Invoke(libraryId);
        });
        
        _hubConnection.On<LibraryDTO>("LibraryUpdated", libraryData =>
        {
            OnLibraryUpdated?.Invoke(libraryData);
        });
        
        _hubConnection.On<SerieDTO>("SerieCreated", serieData =>
        {
            OnSerieCreated?.Invoke(serieData);
        });
        
        _hubConnection.On<SerieDTO>("SerieUpdated", serieData =>
        {
            OnSerieUpdated?.Invoke(serieData);
        });
        
        _hubConnection.On<long>("SerieDeleted", serieId =>
        {
            OnSerieDeleted?.Invoke(serieId);
        });
        
        _hubConnection.On<ChapterDTO>("ChapterAdded", chapterData =>
        {
            OnChapterAdded?.Invoke(chapterData);
        });
        
        _hubConnection.On<long>("ChapterRemoved", chapterId =>
        {
            OnChapterDeleted?.Invoke(chapterId);
        });
        
        _hubConnection.On<UserDTO>("UserCreated", userData =>
        {
            OnUserCreated?.Invoke(userData);
        });
        
        _hubConnection.On<long>("UserDeleted", userId =>
        {
            OnUserDeleted?.Invoke(userId);
        });
        
        _hubConnection.On<Dictionary<string, int>>("RunningTasks", tasks =>
        {
            OnRunningTasks?.Invoke(tasks);
        });

        _hubConnection.On<long>("PosterModified", serieId =>
        {
            OnPosterModified?.Invoke(serieId);
        });
        
        _hubConnection.On<string>("Connected", message =>
        {
            Console.WriteLine($"SignalR: {message}");
        });

        _hubConnection.Closed += async exception =>
        {
            Console.WriteLine($"SignalR: Connexion fermée{(exception != null ? $" avec erreur: {exception.Message}" : "")}");
            await Task.Delay(5000);
            await ConnectAsync();
        };
        
        _hubConnection.Reconnecting += exception =>
        {
            Console.WriteLine($"SignalR: Tentative de reconnexion... {(exception != null ? exception.Message : "")}");
            return Task.CompletedTask;
        };
        
        _hubConnection.Reconnected += connectionId =>
        {
            Console.WriteLine($"SignalR: Reconnecté avec ID: {connectionId}");
            return Task.CompletedTask;
        };

        await ConnectAsync();
    }
    
    private static async Task ConnectAsync()
    {
        try
        {
            if (_hubConnection == null) return;
            
            Console.WriteLine("SignalR: Tentative de connexion...");
            await _hubConnection.StartAsync();
            Console.WriteLine("SignalR: Connexion établie avec succès!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR: Erreur de connexion: {ex.Message}");
            // Attendre avant de réessayer
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
                Console.WriteLine($"SignalR: Erreur lors de la déconnexion: {ex.Message}");
            }
        }
    }
}
