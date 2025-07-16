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
    
    private static void TrySendToAllClientsAsync(string methodName, object? arg)
    {
        try
        {
            _hubContext.Clients.All.SendAsync(methodName, arg);
            _logger?.LogDebug("Message envoyé à tous les clients: {Method}", methodName);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Erreur lors de l'envoi de la notification {Method}", methodName);
        }
    }
    
    public static void NotifyLibraryCreatedAsync(LibraryDTO library)
    {
        TrySendToAllClientsAsync("LibraryCreated", library);
    }

    public static void NotifyLibraryDeletedAsync(long id)
    {
        TrySendToAllClientsAsync("LibraryDeleted", id);
    }

    public static void NotifyLibraryUpdatedAsync(LibraryDTO library)
    {
        TrySendToAllClientsAsync("LibraryUpdated", library);
    }

    public static void NotifySerieCreatedAsync(SerieDTO serie)
    {
        TrySendToAllClientsAsync("SerieCreated", serie);
    }

    public static void NotifySerieUpdatedAsync(SerieDTO serie)
    {
        TrySendToAllClientsAsync("SerieUpdated", serie);
    }
    
    public static void NotifySerieDeletedAsync(long serieId)
    {
        TrySendToAllClientsAsync("SerieDeleted", serieId);
    }

    public static void NotifyChapterAddedAsync(ChapterDTO chapter)
    {
        TrySendToAllClientsAsync("ChapterAdded", chapter);
    }

    public static void NotifyChapterRemovedAsync(long chapterId)
    {
        TrySendToAllClientsAsync("ChapterRemoved", chapterId);
    }
    
    public static void NotifyUserCreatedAsync(UserDTO user)
    {
        TrySendToAllClientsAsync("UserCreated", user);
    }
    
    public static void NotifyUserDeletedAsync(long userId)
    {
        TrySendToAllClientsAsync("UserDeleted", userId);
    }

    public static void NotifyRunningTasksAsync(Dictionary<string, int> tasks)
    {
        TrySendToAllClientsAsync("RunningTasks", tasks);
    }

    public static void NotifyPosterModifiedAsync(long serieId)
    {
        TrySendToAllClientsAsync("PosterModified", serieId);
    }
}
