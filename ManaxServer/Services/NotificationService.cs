using ManaxLibrary.DTOs;
using ManaxLibrary.DTOs.Library;
using ManaxLibrary.DTOs.Serie;
using ManaxLibrary.DTOs.User;
using Microsoft.AspNetCore.SignalR;
using ManaxServer.Hubs;

namespace ManaxServer.Services;

public static class NotificationService
{
    private static IHubContext<NotificationHub> _hubContext = null!;
    private static ILogger<NotificationHub>? _logger;
    
    public static void Initialize(IHubContext<NotificationHub> hubContext, ILogger<NotificationHub>? logger = null)
    {
        _hubContext = hubContext;
        _logger = logger;
        _logger?.LogInformation("NotificationService initialisé");
    }
    
    private static async Task TrySendToAllClientsAsync(string methodName, object? arg)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync(methodName, arg);
            _logger?.LogDebug("Message envoyé à tous les clients: {Method}", methodName);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Erreur lors de l'envoi de la notification {Method}", methodName);
        }
    }
    
    public static async Task NotifyLibraryCreatedAsync(LibraryDTO library)
    {
        await TrySendToAllClientsAsync("LibraryCreated", library);
    }

    public static async Task NotifyLibraryDeletedAsync(long id)
    {
        await TrySendToAllClientsAsync("LibraryDeleted", id.ToString());
    }

    public static async Task NotifyLibraryUpdatedAsync(LibraryDTO library)
    {
        await TrySendToAllClientsAsync("LibraryUpdated", library);
    }

    public static async Task NotifySerieCreatedAsync(SerieDTO serie)
    {
        await TrySendToAllClientsAsync("SerieCreated", serie);
    }

    public static async Task NotifySerieUpdatedAsync(SerieDTO serie)
    {
        await TrySendToAllClientsAsync("SerieUpdated", serie);
    }
    
    public static async Task NotifySerieDeletedAsync(long serieId)
    {
        await TrySendToAllClientsAsync("SerieDeleted", serieId.ToString());
    }

    public static async Task NotifyChapterAddedAsync(ChapterDTO chapter)
    {
        await TrySendToAllClientsAsync("ChapterAdded", chapter);
    }

    public static async Task NotifyChapterRemovedAsync(long chapterId)
    {
        await TrySendToAllClientsAsync("ChapterRemoved", chapterId.ToString());
    }
    
    public static async Task NotifyUserCreatedAsync(UserDTO user)
    {
        await TrySendToAllClientsAsync("UserCreated", user);
    }
    
    public static async Task NotifyUserDeletedAsync(long userId)
    {
        await TrySendToAllClientsAsync("UserDeleted", userId.ToString());
    }

    public static async Task NotifyRunningTasksAsync(Dictionary<string, int> tasks)
    {
        await TrySendToAllClientsAsync("RunningTasks", tasks);
    }
}
