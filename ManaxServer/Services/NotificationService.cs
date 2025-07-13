using ManaxLibrary.DTOs;
using ManaxLibrary.DTOs.Library;
using ManaxLibrary.DTOs.Serie;
using ManaxLibrary.DTOs.User;
using Microsoft.AspNetCore.SignalR;

namespace ManaxServer.Services;

public class NotificationService(IHubContext<Hub> hubContext)
{
    public async Task NotifyLibraryCreatedAsync(LibraryDTO library)
    {
        await hubContext.Clients.All.SendAsync("LibraryCreated", library);
    }

    public async Task NotifyLibraryDeletedAsync(long id)
    {
        await hubContext.Clients.All.SendAsync("LibraryDeleted", id.ToString());
    }

    public async Task NotifyLibraryUpdatedAsync(LibraryDTO library)
    {
        await hubContext.Clients.All.SendAsync("LibraryUpdated", library);
    }

    public async Task NotifySerieCreatedAsync(SerieDTO serie)
    {
        await hubContext.Clients.All.SendAsync("SerieCreated", serie);
    }

    public async Task NotifySerieUpdatedAsync(SerieDTO serie)
    {
        await hubContext.Clients.All.SendAsync("SerieUpdated", serie);
    }
    
    public async Task NotifySerieDeletedAsync(long serieId)
    {
        await hubContext.Clients.All.SendAsync("SerieDeleted", serieId.ToString());
    }

    public async Task NotifyChapterAddedAsync(ChapterDTO chapter)
    {
        await hubContext.Clients.All.SendAsync("ChapterAdded", chapter);
        await hubContext.Clients.All.SendAsync("ChapterAdded", chapter);
    }

    public async Task NotifyChapterRemovedAsync(long chapterId)
    {
        await hubContext.Clients.All.SendAsync("ChapterRemoved", chapterId.ToString());
    }
    
    public async Task NotifyUserCreatedAsync(UserDTO user)
    {
        await hubContext.Clients.All.SendAsync("UserCreated", user);
    }
    
    public async Task NotifyUserDeletedAsync(long userId)
    {
        await hubContext.Clients.All.SendAsync("UserDeleted", userId.ToString());
    }
}
