using ManaxLibrary.Logging;
using ManaxServer.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ManaxServer.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        try
        {
            Logger.LogInfo(Localizer.Format("HubConnected", Context.ConnectionId, Context.User?.Identity?.Name ?? "Unknown"));
            await base.OnConnectedAsync();
            
            await Clients.Caller.SendAsync("Connected", Localizer.Format("HubConnectionSuccess"));
        }
        catch (Exception ex)
        {
            
            Logger.LogError(Localizer.Format("HubConnectionError", Context.ConnectionId),ex,Environment.StackTrace);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            if (exception != null)
            {
                Logger.LogError(Localizer.Format("HubDisconnectedError",Context.ConnectionId),exception, Environment.StackTrace);
            }
            else
            {
                Logger.LogInfo(Localizer.Format("HubDisconnected",Context.ConnectionId));
            }
            
            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            Logger.LogError(Localizer.Format("HubDisconnectedError",Context.ConnectionId),ex, Environment.StackTrace);
        }
    }
}
