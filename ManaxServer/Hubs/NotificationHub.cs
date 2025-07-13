using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ManaxServer.Hubs;

[Authorize]
public class NotificationHub(ILogger<NotificationHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        try
        {
            logger.LogInformation("Client connecté: {ConnectionId} - User: {UserId}", 
                Context.ConnectionId, Context.User?.Identity?.Name ?? "Unknown");
            await base.OnConnectedAsync();
            
            // Envoyer une confirmation de connexion uniquement au client appelant
            await Clients.Caller.SendAsync("Connected", "Connexion établie avec succès");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la connexion du client {ConnectionId}", Context.ConnectionId);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            if (exception != null)
            {
                logger.LogWarning(exception, "Client déconnecté avec erreur: {ConnectionId}", Context.ConnectionId);
            }
            else
            {
                logger.LogInformation("Client déconnecté: {ConnectionId}", Context.ConnectionId);
            }
            
            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la déconnexion du client {ConnectionId}", Context.ConnectionId);
        }
    }
}
