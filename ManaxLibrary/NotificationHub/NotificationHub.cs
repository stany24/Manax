using Microsoft.AspNetCore.SignalR.Client;
using ManaxLibrary.DTOs;
using ManaxLibrary.DTOs.User;
using ManaxLibrary.DTOs.Library;
using ManaxLibrary.DTOs.Serie;

namespace ManaxLibrary.NotificationHub;

public static class NotificationHub
{
    private static HubConnection? _hubConnection;
    private static string _serverUrl = null!;
    private static string _token = null!;
    
    public static event Action<LibraryDTO> OnLibraryCreated;
    public static event Action<string> OnLibraryDeleted;
    public static event Action<LibraryDTO> OnLibraryUpdated;
    
    public static event Action<SerieDTO> OnSerieCreated;
    public static event Action<SerieDTO> OnSerieUpdated;
    public static event Action<string> OnSerieDeleted;
    
    public static event Action<ChapterDTO> OnChapterAdded;
    public static event Action<string> OnChapterDeleted;
    
    public static event Action<UserDTO> OnUserCreated;
    public static event Action<string> OnUserDeleted;
    
    public static async Task InitializeAsync(Uri host, string token)
    {
        _serverUrl = host.AbsoluteUri;
        _token = token;
        if (_hubConnection != null) { return; }

        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{_serverUrl}/notificationHub?access_token={_token}")
            .WithAutomaticReconnect()
            .Build();
        
        _hubConnection.On<LibraryDTO>("LibraryCreated", libraryData =>
        {
            OnLibraryCreated(libraryData);
        });
        
        _hubConnection.On<string>("LibraryDeleted", libraryId =>
        {
            OnLibraryDeleted.Invoke(libraryId);
        });
        
        _hubConnection.On<LibraryDTO>("LibraryUpdated", libraryData =>
        {
            OnLibraryUpdated.Invoke(libraryData);
        });
        
        _hubConnection.On<SerieDTO>("SerieCreated", serieData =>
        {
            OnSerieCreated.Invoke(serieData);
        });
        
        _hubConnection.On<SerieDTO>("SerieUpdated", serieData =>
        {
            OnSerieUpdated.Invoke(serieData);
        });
        
        _hubConnection.On<string>("SerieDeleted", serieId =>
        {
            OnSerieDeleted.Invoke(serieId);
        });
        
        _hubConnection.On<ChapterDTO>("ChapterAdded", chapterData =>
        {
            OnChapterAdded.Invoke(chapterData);
        });
        
        _hubConnection.On<string>("ChapterRemoved", chapterId =>
        {
            OnChapterDeleted.Invoke(chapterId);
        });
        
        _hubConnection.On<UserDTO>("UserCreated", userData =>
        {
            OnUserCreated.Invoke(userData);
        });
        
        _hubConnection.On<string>("UserDeleted", userId =>
        {
            OnUserDeleted.Invoke(userId);
        });

        _hubConnection.Closed += async _ =>
        {
            await Task.Delay(5000);
            await ConnectAsync();
        };

        _hubConnection.Reconnected += _ => Task.CompletedTask;

        await ConnectAsync();
    }
    
    private static async Task ConnectAsync()
    {
        try
        {
            await _hubConnection.StartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur de connexion SignalR: {ex.Message}");
        }
    }
    
    public static async Task StopAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }
    }
}
